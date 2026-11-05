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

    /// <summary>All users, active and inactive, ordered by name — for the user-management screen.</summary>
    Task<List<User>> GetAllAsync();

    /// <summary>The roles the signed-in user is allowed to hand out when creating an account.</summary>
    IReadOnlyList<UserRole> GetAssignableRoles();

    /// <summary>
    /// Creates a new account. Only Maintenance Managers and Admins may do this (enforced here, not
    /// just in the UI). Throws <see cref="UnauthorizedAccessException"/> if the signed-in user isn't
    /// allowed, or <see cref="InvalidOperationException"/> with a user-friendly message if the
    /// details are invalid (blank fields, short password, duplicate username, disallowed role).
    /// </summary>
    Task<User> CreateUserAsync(string fullName, string username, string password, UserRole role);
}
