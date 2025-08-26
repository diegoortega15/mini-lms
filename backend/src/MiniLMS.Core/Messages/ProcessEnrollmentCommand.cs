namespace MiniLMS.Core.Messages;

public interface ProcessEnrollmentCommand
{
    int JobId { get; }
    int CourseId { get; }
    string FilePath { get; }
    DateTime CreatedAt { get; }
}
