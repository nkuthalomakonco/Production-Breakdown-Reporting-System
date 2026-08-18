using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;

namespace BreakdownManager.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetByRoleAsync(UserRole role);
    Task<User?> GetByIdAsync(int id);
}
