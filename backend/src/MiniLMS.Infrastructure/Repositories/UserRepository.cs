using Microsoft.EntityFrameworkCore;
using MiniLMS.Core.Interfaces;
using MiniLMS.Core.Models;
using MiniLMS.Infrastructure.Data;

namespace MiniLMS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User> CreateAsync(User user)
    {
        user.Email = user.Email.ToLower().Trim();
        user.Name = user.Name.Trim();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpsertByEmailAsync(string email, string name)
    {
        email = email.ToLower().Trim();
        name = name.Trim();
        
        var existingUser = await GetByEmailAsync(email);
        if (existingUser != null)
        {
            if (existingUser.Name != name)
            {
                existingUser.Name = name;
                await UpdateAsync(existingUser);
            }
            return existingUser;
        }

        return await CreateAsync(new User { Email = email, Name = name });
    }
}
