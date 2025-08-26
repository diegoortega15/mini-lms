using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniLMS.Core.Interfaces;
using MiniLMS.Core.Models;
using MiniLMS.Infrastructure.Data;
using MiniLMS.Infrastructure.Repositories;
using MiniLMS.Infrastructure.Services;
using Moq;
using Xunit;

namespace MiniLMS.Tests;

public class EnrollmentServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICsvProcessor> _csvProcessorMock;
    private readonly Mock<ILogger<EnrollmentService>> _loggerMock;
    private readonly IUserRepository _userRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IImportJobRepository _importJobRepository;
    private readonly EnrollmentService _enrollmentService;

    public EnrollmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _csvProcessorMock = new Mock<ICsvProcessor>();
        _loggerMock = new Mock<ILogger<EnrollmentService>>();
        
        _userRepository = new UserRepository(_context);
        _enrollmentRepository = new EnrollmentRepository(_context);
        _importJobRepository = new ImportJobRepository(_context);

        _enrollmentService = new EnrollmentService(
            _csvProcessorMock.Object,
            _userRepository,
            _enrollmentRepository,
            _importJobRepository,
            _loggerMock.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var course = new Course
        {
            Id = 1,
            Title = "Test Course",
            Category = "Test",
            IsActive = true
        };

        _context.Courses.Add(course);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ProcessEnrollmentAsync_WithValidData_CreatesEnrollments()
    {
        // Arrange
        var importJob = new ImportJob
        {
            CourseId = 1,
            Status = ImportJobStatus.Queued,
            FileName = "test.csv",
            FilePath = "/test/path.csv"
        };
        await _importJobRepository.CreateAsync(importJob);

        var csvRows = new List<Core.DTOs.CsvRowDto>
        {
            new() { Email = "user1@example.com", Name = "User 1", RowNumber = 1 },
            new() { Email = "user2@example.com", Name = "User 2", RowNumber = 2 }
        };

        _csvProcessorMock.Setup(x => x.ParseCsvAsync(It.IsAny<string>()))
            .ReturnsAsync(csvRows);
        _csvProcessorMock.Setup(x => x.DeduplicateByEmail(It.IsAny<List<Core.DTOs.CsvRowDto>>()))
            .Returns(csvRows);

        // Act
        var result = await _enrollmentService.ProcessEnrollmentAsync(importJob.Id, 1, "/test/path.csv");

        // Assert
        Assert.Equal(2, result);

        var updatedJob = await _importJobRepository.GetByIdAsync(importJob.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal(ImportJobStatus.Completed, updatedJob.Status);
        Assert.Equal(2, updatedJob.TotalRows);
        Assert.Equal(2, updatedJob.Succeeded);
        Assert.Equal(0, updatedJob.Failed);
        Assert.Equal(0, updatedJob.IgnoredDuplicates);

        // Verify users were created
        var user1 = await _userRepository.GetByEmailAsync("user1@example.com");
        var user2 = await _userRepository.GetByEmailAsync("user2@example.com");
        Assert.NotNull(user1);
        Assert.NotNull(user2);

        // Verify enrollments were created
        var enrollment1 = await _enrollmentRepository.GetByUserAndCourseAsync(user1.Id, 1);
        var enrollment2 = await _enrollmentRepository.GetByUserAndCourseAsync(user2.Id, 1);
        Assert.NotNull(enrollment1);
        Assert.NotNull(enrollment2);
    }

    [Fact]
    public async Task ProcessEnrollmentAsync_WithDuplicateUsers_IgnoresDuplicates()
    {
        // Arrange
        // Pre-create a user and enrollment
        var existingUser = await _userRepository.CreateAsync(new User
        {
            Email = "existing@example.com",
            Name = "Existing User"
        });
        await _enrollmentRepository.CreateAsync(new Enrollment
        {
            UserId = existingUser.Id,
            CourseId = 1
        });

        var importJob = new ImportJob
        {
            CourseId = 1,
            Status = ImportJobStatus.Queued,
            FileName = "test.csv",
            FilePath = "/test/path.csv"
        };
        await _importJobRepository.CreateAsync(importJob);

        var csvRows = new List<Core.DTOs.CsvRowDto>
        {
            new() { Email = "existing@example.com", Name = "Existing User", RowNumber = 1 }, // Already enrolled
            new() { Email = "new@example.com", Name = "New User", RowNumber = 2 }
        };

        _csvProcessorMock.Setup(x => x.ParseCsvAsync(It.IsAny<string>()))
            .ReturnsAsync(csvRows);
        _csvProcessorMock.Setup(x => x.DeduplicateByEmail(It.IsAny<List<Core.DTOs.CsvRowDto>>()))
            .Returns(csvRows);

        // Act
        var result = await _enrollmentService.ProcessEnrollmentAsync(importJob.Id, 1, "/test/path.csv");

        // Assert
        Assert.Equal(1, result); // Only one new enrollment

        var updatedJob = await _importJobRepository.GetByIdAsync(importJob.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal(ImportJobStatus.Completed, updatedJob.Status);
        Assert.Equal(2, updatedJob.TotalRows);
        Assert.Equal(1, updatedJob.Succeeded);
        Assert.Equal(0, updatedJob.Failed);
        Assert.Equal(1, updatedJob.IgnoredDuplicates); // Existing enrollment counted as duplicate
    }

    [Fact]
    public async Task ProcessEnrollmentAsync_WithInvalidEmails_CountsFailures()
    {
        // Arrange
        var importJob = new ImportJob
        {
            CourseId = 1,
            Status = ImportJobStatus.Queued,
            FileName = "test.csv",
            FilePath = "/test/path.csv"
        };
        await _importJobRepository.CreateAsync(importJob);

        var csvRows = new List<Core.DTOs.CsvRowDto>
        {
            new() { Email = "", Name = "Invalid User 1", RowNumber = 1 }, // Invalid email
            new() { Email = "invalid-email", Name = "Invalid User 2", RowNumber = 2 }, // Invalid email
            new() { Email = "valid@example.com", Name = "Valid User", RowNumber = 3 }
        };

        _csvProcessorMock.Setup(x => x.ParseCsvAsync(It.IsAny<string>()))
            .ReturnsAsync(csvRows);
        _csvProcessorMock.Setup(x => x.DeduplicateByEmail(It.IsAny<List<Core.DTOs.CsvRowDto>>()))
            .Returns(csvRows);

        // Act
        var result = await _enrollmentService.ProcessEnrollmentAsync(importJob.Id, 1, "/test/path.csv");

        // Assert
        Assert.Equal(1, result); // Only valid user enrolled

        var updatedJob = await _importJobRepository.GetByIdAsync(importJob.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal(ImportJobStatus.Completed, updatedJob.Status);
        Assert.Equal(3, updatedJob.TotalRows);
        Assert.Equal(1, updatedJob.Succeeded);
        Assert.Equal(2, updatedJob.Failed); // Two invalid emails
        Assert.Equal(0, updatedJob.IgnoredDuplicates);
    }

    [Fact]
    public async Task ProcessEnrollmentAsync_ReprocessingSameFile_IsIdempotent()
    {
        // Arrange
        var importJob1 = new ImportJob
        {
            CourseId = 1,
            Status = ImportJobStatus.Queued,
            FileName = "test.csv",
            FilePath = "/test/path.csv"
        };
        await _importJobRepository.CreateAsync(importJob1);

        var csvRows = new List<Core.DTOs.CsvRowDto>
        {
            new() { Email = "user@example.com", Name = "Test User", RowNumber = 1 }
        };

        _csvProcessorMock.Setup(x => x.ParseCsvAsync(It.IsAny<string>()))
            .ReturnsAsync(csvRows);
        _csvProcessorMock.Setup(x => x.DeduplicateByEmail(It.IsAny<List<Core.DTOs.CsvRowDto>>()))
            .Returns(csvRows);

        // First processing
        await _enrollmentService.ProcessEnrollmentAsync(importJob1.Id, 1, "/test/path.csv");

        // Create second job for same file
        var importJob2 = new ImportJob
        {
            CourseId = 1,
            Status = ImportJobStatus.Queued,
            FileName = "test.csv",
            FilePath = "/test/path.csv"
        };
        await _importJobRepository.CreateAsync(importJob2);

        // Act - Process same data again
        var result = await _enrollmentService.ProcessEnrollmentAsync(importJob2.Id, 1, "/test/path.csv");

        // Assert
        Assert.Equal(0, result); // No new enrollments

        var updatedJob = await _importJobRepository.GetByIdAsync(importJob2.Id);
        Assert.NotNull(updatedJob);
        Assert.Equal(ImportJobStatus.Completed, updatedJob.Status);
        Assert.Equal(1, updatedJob.TotalRows);
        Assert.Equal(0, updatedJob.Succeeded);
        Assert.Equal(0, updatedJob.Failed);
        Assert.Equal(1, updatedJob.IgnoredDuplicates); // User already enrolled

        // Verify only one enrollment exists
        var user = await _userRepository.GetByEmailAsync("user@example.com");
        Assert.NotNull(user);
        var enrollments = _context.Enrollments.Where(e => e.UserId == user.Id && e.CourseId == 1).ToList();
        Assert.Single(enrollments);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
