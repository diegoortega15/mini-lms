using Microsoft.EntityFrameworkCore;
using MiniLMS.Core.Interfaces;
using MiniLMS.Core.Models;
using MiniLMS.Infrastructure.Data;

namespace MiniLMS.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetByUserAndCourseAsync(int userId, int courseId)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        enrollment.CreatedAt = DateTime.UtcNow;
        
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<bool> ExistsAsync(int userId, int courseId)
    {
        return await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
    }
}

public class ImportJobRepository : IImportJobRepository
{
    private readonly ApplicationDbContext _context;

    public ImportJobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ImportJob> CreateAsync(ImportJob importJob)
    {
        importJob.CreatedAt = DateTime.UtcNow;
        
        _context.ImportJobs.Add(importJob);
        await _context.SaveChangesAsync();
        return importJob;
    }

    public async Task<ImportJob> UpdateAsync(ImportJob importJob)
    {
        _context.Entry(importJob).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return importJob;
    }

    public async Task<ImportJob?> GetByIdAsync(int id)
    {
        return await _context.ImportJobs
            .Include(j => j.Course)
            .FirstOrDefaultAsync(j => j.Id == id);
    }
}
