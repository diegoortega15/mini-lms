using MiniLMS.Core.Interfaces;
using MiniLMS.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace MiniLMS.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ICsvProcessor _csvProcessor;
    private readonly IUserRepository _userRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IImportJobRepository _importJobRepository;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        ICsvProcessor csvProcessor,
        IUserRepository userRepository,
        IEnrollmentRepository enrollmentRepository,
        IImportJobRepository importJobRepository,
        ILogger<EnrollmentService> logger)
    {
        _csvProcessor = csvProcessor;
        _userRepository = userRepository;
        _enrollmentRepository = enrollmentRepository;
        _importJobRepository = importJobRepository;
        _logger = logger;
    }

    public async Task<int> ProcessEnrollmentAsync(int jobId, int courseId, string filePath)
    {
        var job = await _importJobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            _logger.LogError($"Import job {jobId} not found");
            return 0;
        }

        try
        {
            // Update job status to Processing
            job.Status = ImportJobStatus.Processing;
            await _importJobRepository.UpdateAsync(job);

            // Parse CSV
            var allRows = await _csvProcessor.ParseCsvAsync(filePath);
            job.TotalRows = allRows.Count;

            // Deduplicate by email
            var deduplicatedRows = _csvProcessor.DeduplicateByEmail(allRows);
            job.IgnoredDuplicates = allRows.Count - deduplicatedRows.Count;

            int succeeded = 0;
            int failed = 0;

            foreach (var row in deduplicatedRows)
            {
                try
                {
                    // Validate row data
                    if (string.IsNullOrWhiteSpace(row.Email) || 
                        string.IsNullOrWhiteSpace(row.Name) ||
                        !IsValidEmail(row.Email))
                    {
                        failed++;
                        _logger.LogWarning($"Invalid row data at line {row.RowNumber}: Email='{row.Email}', Name='{row.Name}'");
                        continue;
                    }

                    // Upsert user
                    var user = await _userRepository.UpsertByEmailAsync(row.Email, row.Name);

                    // Check if enrollment already exists
                    if (await _enrollmentRepository.ExistsAsync(user.Id, courseId))
                    {
                        // Already enrolled, count as ignored duplicate
                        job.IgnoredDuplicates++;
                        continue;
                    }

                    // Create enrollment
                    var enrollment = new Enrollment
                    {
                        UserId = user.Id,
                        CourseId = courseId
                    };

                    await _enrollmentRepository.CreateAsync(enrollment);
                    succeeded++;

                    _logger.LogDebug($"Successfully enrolled user {user.Email} in course {courseId}");
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogError(ex, $"Error processing row {row.RowNumber}: {ex.Message}");
                }
            }

            // Update job with final results
            job.Succeeded = succeeded;
            job.Failed = failed;
            job.Status = ImportJobStatus.Completed;
            job.FinishedAt = DateTime.UtcNow;

            await _importJobRepository.UpdateAsync(job);

            _logger.LogInformation($"Job {jobId} completed. Total: {job.TotalRows}, Succeeded: {succeeded}, Failed: {failed}, Ignored: {job.IgnoredDuplicates}");

            return succeeded;
        }
        catch (Exception ex)
        {
            job.Status = ImportJobStatus.Failed;
            job.Note = ex.Message;
            job.FinishedAt = DateTime.UtcNow;
            await _importJobRepository.UpdateAsync(job);

            _logger.LogError(ex, $"Job {jobId} failed: {ex.Message}");
            throw;
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}
