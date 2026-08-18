using BreakdownManager.Data;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
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
}
