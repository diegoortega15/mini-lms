using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MiniLMS.Core.DTOs;
using MiniLMS.Core.Interfaces;
using MiniLMS.Core.Messages;
using MiniLMS.Core.Models;

namespace MiniLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly IImportJobRepository _importJobRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(
        IImportJobRepository importJobRepository,
        ICourseRepository courseRepository,
        IPublishEndpoint publishEndpoint,
        IMapper mapper,
        ILogger<EnrollmentsController> logger)
    {
        _importJobRepository = importJobRepository;
        _courseRepository = courseRepository;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportJobDto>> ImportEnrollments([FromQuery] int courseId, IFormFile file)
    {
        try
        {
            // Validate course exists
            if (!await _courseRepository.ExistsAsync(courseId))
            {
                return BadRequest($"Course with ID {courseId} does not exist");
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only CSV files are supported");
            }

            // Save file to imports directory
            var importsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "imports");
            Directory.CreateDirectory(importsDirectory);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(importsDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create import job
            var importJob = new ImportJob
            {
                CourseId = courseId,
                FileName = file.FileName,
                FilePath = filePath,
                Status = ImportJobStatus.Queued
            };

            var createdJob = await _importJobRepository.CreateAsync(importJob);

            // Publish message to queue
            await _publishEndpoint.Publish<ProcessEnrollmentCommand>(new
            {
                JobId = createdJob.Id,
                CourseId = courseId,
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation($"Import job {createdJob.Id} created and queued for course {courseId}");

            // Get the job with course information for response
            var jobWithCourse = await _importJobRepository.GetByIdAsync(createdJob.Id);
            var jobDto = _mapper.Map<ImportJobDto>(jobWithCourse);

            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating import job");
            return StatusCode(500, "An error occurred while processing the import request");
        }
    }

    [HttpGet("import/{jobId}")]
    public async Task<ActionResult<ImportJobDto>> GetImportJob(int jobId)
    {
        try
        {
            var job = await _importJobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return NotFound($"Import job {jobId} not found");
            }

            var jobDto = _mapper.Map<ImportJobDto>(job);
            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving import job {jobId}");
            return StatusCode(500, "An error occurred while retrieving the import job");
        }
    }
}
