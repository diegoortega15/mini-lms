using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MiniLMS.Core.DTOs;
using MiniLMS.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MiniLMS.Infrastructure.Services;

public class CsvProcessor : ICsvProcessor
{
    private readonly ILogger<CsvProcessor> _logger;

    public CsvProcessor(ILogger<CsvProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<List<CsvRowDto>> ParseCsvAsync(string filePath)
    {
        var rows = new List<CsvRowDto>();
        
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            });

            await csv.ReadAsync();
            csv.ReadHeader();
            
            int rowNumber = 1;
            
            while (await csv.ReadAsync())
            {
                rowNumber++;
                
                try
                {
                    var email = csv.GetField("email")?.Trim() ?? string.Empty;
                    var name = csv.GetField("name")?.Trim() ?? string.Empty;

                    rows.Add(new CsvRowDto
                    {
                        Email = email,
                        Name = name,
                        RowNumber = rowNumber
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error parsing row {rowNumber}: {ex.Message}");
                    rows.Add(new CsvRowDto
                    {
                        Email = string.Empty,
                        Name = string.Empty,
                        RowNumber = rowNumber
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading CSV file: {filePath}");
            throw;
        }

        return rows;
    }

    public List<CsvRowDto> DeduplicateByEmail(List<CsvRowDto> rows)
    {
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var deduplicatedRows = new List<CsvRowDto>();

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Email))
            {
                deduplicatedRows.Add(row);
                continue;
            }

            var normalizedEmail = row.Email.ToLower().Trim();
            
            if (!seenEmails.Contains(normalizedEmail))
            {
                seenEmails.Add(normalizedEmail);
                deduplicatedRows.Add(row);
            }
        }

        return deduplicatedRows;
    }
}
