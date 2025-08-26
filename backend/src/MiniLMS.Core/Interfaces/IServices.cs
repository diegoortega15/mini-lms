using MiniLMS.Core.DTOs;

namespace MiniLMS.Core.Interfaces;

public interface ICsvProcessor
{
    Task<List<CsvRowDto>> ParseCsvAsync(string filePath);
    List<CsvRowDto> DeduplicateByEmail(List<CsvRowDto> rows);
}

public interface IEnrollmentService
{
    Task<int> ProcessEnrollmentAsync(int jobId, int courseId, string filePath);
}
