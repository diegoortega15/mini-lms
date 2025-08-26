using MiniLMS.Core.Models;

namespace MiniLMS.Core.Interfaces;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task<Course> CreateAsync(Course course);
    Task<Course> UpdateAsync(Course course);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<User> UpsertByEmailAsync(string email, string name);
}

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByUserAndCourseAsync(int userId, int courseId);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<bool> ExistsAsync(int userId, int courseId);
}

public interface IImportJobRepository
{
    Task<ImportJob> CreateAsync(ImportJob importJob);
    Task<ImportJob> UpdateAsync(ImportJob importJob);
    Task<ImportJob?> GetByIdAsync(int id);
}
