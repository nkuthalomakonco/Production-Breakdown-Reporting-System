using BreakdownManager.Data;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Domain.Security;
using BreakdownManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BreakdownManager.Services;

public class UserService : IUserService
{
    private readonly BreakdownManagerDbContext _db;

    public UserService(BreakdownManagerDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetByRoleAsync(UserRole role)
    {
        return await _db.Users
            .Where(u => u.Role == role && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var normalizedUsername = username.Trim();

        var user = await _db.Users
            .SingleOrDefaultAsync(u => u.Username == normalizedUsername && u.IsActive);

        if (user is null)
            return null;

        return PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
    }
}
