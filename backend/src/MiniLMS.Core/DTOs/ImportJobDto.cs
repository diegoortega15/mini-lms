using MiniLMS.Core.Models;

namespace MiniLMS.Core.DTOs;

public class ImportJobDto
{
    public int Id { get; set; }
    public ImportJobStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int TotalRows { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public int IgnoredDuplicates { get; set; }
    public string? Note { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
}

public class CsvRowDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int RowNumber { get; set; }
}
