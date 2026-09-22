using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;

namespace BreakdownManager.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetByRoleAsync(UserRole role);
    Task<User?> GetByIdAsync(int id);

    /// <summary>Checks a username/password pair against the stored hash. Returns null for any failure
    /// (unknown username, wrong password, or an inactive account) without saying which.</summary>
    Task<User?> AuthenticateAsync(string username, string password);
}
