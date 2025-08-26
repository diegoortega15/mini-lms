using MassTransit;
using MiniLMS.Core.Interfaces;
using MiniLMS.Core.Messages;

namespace MiniLMS.Worker;

public class ProcessEnrollmentConsumer : IConsumer<ProcessEnrollmentCommand>
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILogger<ProcessEnrollmentConsumer> _logger;

    public ProcessEnrollmentConsumer(IEnrollmentService enrollmentService, ILogger<ProcessEnrollmentConsumer> logger)
    {
        _enrollmentService = enrollmentService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessEnrollmentCommand> context)
    {
        var message = context.Message;
        
        _logger.LogInformation($"Processing enrollment job {message.JobId} for course {message.CourseId}");

        try
        {
            var successCount = await _enrollmentService.ProcessEnrollmentAsync(
                message.JobId, 
                message.CourseId, 
                message.FilePath);

            _logger.LogInformation($"Successfully processed job {message.JobId}. {successCount} enrollments created.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to process enrollment job {message.JobId}: {ex.Message}");
            throw; // Re-throw to trigger retry logic if configured
        }
    }
}
