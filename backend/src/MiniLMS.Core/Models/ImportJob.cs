namespace MiniLMS.Core.Models;

public enum ImportJobStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}

public class ImportJob
{
    public int Id { get; set; }
    public ImportJobStatus Status { get; set; } = ImportJobStatus.Queued;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public int TotalRows { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public int IgnoredDuplicates { get; set; }
    public string? Note { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int CourseId { get; set; }

    // Navigation properties
    public Course Course { get; set; } = null!;
}
