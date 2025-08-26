using MiniLMS.Core.DTOs;
using MiniLMS.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MiniLMS.Tests;

public class CsvProcessorTests
{
    private readonly Mock<ILogger<CsvProcessor>> _loggerMock;
    private readonly CsvProcessor _csvProcessor;

    public CsvProcessorTests()
    {
        _loggerMock = new Mock<ILogger<CsvProcessor>>();
        _csvProcessor = new CsvProcessor(_loggerMock.Object);
    }

    [Fact]
    public void DeduplicateByEmail_RemovesDuplicateEmails_CountsIgnoredDuplicates()
    {
        // Arrange
        var rows = new List<CsvRowDto>
        {
            new() { Email = "ana@example.com", Name = "Ana Silva", RowNumber = 1 },
            new() { Email = "ANA@EXAMPLE.COM", Name = "Ana Silva", RowNumber = 2 }, // Duplicate (different case)
            new() { Email = "joao@example.com", Name = "João Souza", RowNumber = 3 },
            new() { Email = "ana@example.com", Name = "Ana Silva", RowNumber = 4 }, // Duplicate
            new() { Email = "maria@example.com", Name = "Maria Santos", RowNumber = 5 }
        };

        // Act
        var deduplicatedRows = _csvProcessor.DeduplicateByEmail(rows);

        // Assert
        Assert.Equal(3, deduplicatedRows.Count); // Only unique emails
        Assert.Contains(deduplicatedRows, r => r.Email == "ana@example.com");
        Assert.Contains(deduplicatedRows, r => r.Email == "joao@example.com");
        Assert.Contains(deduplicatedRows, r => r.Email == "maria@example.com");
        
        // Should keep the first occurrence of each email
        var anaRow = deduplicatedRows.First(r => r.Email == "ana@example.com");
        Assert.Equal(1, anaRow.RowNumber);
    }

    [Fact]
    public void DeduplicateByEmail_HandlesEmptyEmails_KeepsInvalidRows()
    {
        // Arrange
        var rows = new List<CsvRowDto>
        {
            new() { Email = "", Name = "Invalid User 1", RowNumber = 1 },
            new() { Email = "valid@example.com", Name = "Valid User", RowNumber = 2 },
            new() { Email = " ", Name = "Invalid User 2", RowNumber = 3 },
            new() { Email = "valid@example.com", Name = "Valid User Duplicate", RowNumber = 4 }
        };

        // Act
        var deduplicatedRows = _csvProcessor.DeduplicateByEmail(rows);

        // Assert
        Assert.Equal(3, deduplicatedRows.Count); // Empty emails are kept, valid email deduplicated
        Assert.Contains(deduplicatedRows, r => r.Email == "");
        Assert.Contains(deduplicatedRows, r => r.Email == " ");
        Assert.Contains(deduplicatedRows, r => r.Email == "valid@example.com");
        
        // Should keep the first occurrence of valid email
        var validRow = deduplicatedRows.First(r => r.Email == "valid@example.com");
        Assert.Equal(2, validRow.RowNumber);
    }

    [Fact]
    public void DeduplicateByEmail_IsCaseInsensitive()
    {
        // Arrange
        var rows = new List<CsvRowDto>
        {
            new() { Email = "Test@Example.Com", Name = "Test User", RowNumber = 1 },
            new() { Email = "test@example.com", Name = "Test User", RowNumber = 2 },
            new() { Email = "TEST@EXAMPLE.COM", Name = "Test User", RowNumber = 3 }
        };

        // Act
        var deduplicatedRows = _csvProcessor.DeduplicateByEmail(rows);

        // Assert
        Assert.Single(deduplicatedRows);
        Assert.Equal("Test@Example.Com", deduplicatedRows.First().Email);
        Assert.Equal(1, deduplicatedRows.First().RowNumber);
    }

    [Fact]
    public void DeduplicateByEmail_PreservesOriginalEmailCase()
    {
        // Arrange
        var rows = new List<CsvRowDto>
        {
            new() { Email = "User@Domain.COM", Name = "User", RowNumber = 1 },
            new() { Email = "another@example.com", Name = "Another", RowNumber = 2 }
        };

        // Act
        var deduplicatedRows = _csvProcessor.DeduplicateByEmail(rows);

        // Assert
        Assert.Equal(2, deduplicatedRows.Count);
        Assert.Equal("User@Domain.COM", deduplicatedRows.First().Email); // Original case preserved
        Assert.Equal("another@example.com", deduplicatedRows.Last().Email);
    }
}
