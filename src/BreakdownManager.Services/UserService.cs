using BreakdownManager.Data;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Domain.Security;
using BreakdownManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BreakdownManager.Services;

public class UserService : IUserService
{
    /// <summary>Minimum length for a new account's password.</summary>
    public const int MinPasswordLength = 8;

    private readonly BreakdownManagerDbContext _db;
    private readonly ICurrentUserContext _currentUserContext;

    public UserService(BreakdownManagerDbContext db, ICurrentUserContext currentUserContext)
    {
        _db = db;
        _currentUserContext = currentUserContext;
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

    public async Task<List<User>> GetAllAsync()
    {
        EnsureCanManageUsers();

        return await _db.Users
            .OrderBy(u => u.FullName)
            .ThenBy(u => u.Username)
            .ToListAsync();
    }

    public IReadOnlyList<UserRole> GetAssignableRoles()
    {
        return _currentUserContext.CurrentUser?.Role switch
        {
            // Admins can create anyone.
            UserRole.Admin => new[] { UserRole.Supervisor, UserRole.Technician, UserRole.MaintenanceManager, UserRole.Admin },
            // Managers can't mint Admins — otherwise "add users" would be a back door to full control.
            UserRole.MaintenanceManager => new[] { UserRole.Supervisor, UserRole.Technician, UserRole.MaintenanceManager },
            _ => Array.Empty<UserRole>()
        };
    }

    public async Task<User> CreateUserAsync(string fullName, string username, string password, UserRole role)
    {
        EnsureCanManageUsers();

        fullName = (fullName ?? string.Empty).Trim();
        username = (username ?? string.Empty).Trim();

        if (fullName.Length == 0)
            throw new InvalidOperationException("Full name is required.");
        if (username.Length == 0)
            throw new InvalidOperationException("Username is required.");
        if (username.Length > 100)
            throw new InvalidOperationException("Username can't be longer than 100 characters.");
        if (username.Any(char.IsWhiteSpace))
            throw new InvalidOperationException("Username can't contain spaces.");
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            throw new InvalidOperationException($"Password must be at least {MinPasswordLength} characters.");
        if (!GetAssignableRoles().Contains(role))
            throw new InvalidOperationException("You don't have permission to create a user with that role.");

        // Case-insensitive duplicate check, so "John.M" can't sit next to "john.m" and confuse people.
        var lowered = username.ToLower();
        if (await _db.Users.AnyAsync(u => u.Username.ToLower() == lowered))
            throw new InvalidOperationException($"The username '{username}' is already taken.");

        var user = new User
        {
            FullName = fullName,
            Username = username,
            PasswordHash = PasswordHasher.Hash(password),
            Role = role,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private void EnsureCanManageUsers()
    {
        var role = _currentUserContext.CurrentUser?.Role;
        if (role is not (UserRole.MaintenanceManager or UserRole.Admin))
            throw new UnauthorizedAccessException("Only Maintenance Managers and Admins can manage users.");
    }
}
