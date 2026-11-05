using BreakdownManager.Data;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Domain.Security;
using BreakdownManager.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BreakdownManager.Tests;

public class UserServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BreakdownManagerDbContext _db;
    private readonly CurrentUserContext _currentUser = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BreakdownManagerDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new BreakdownManagerDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new UserService(_db, _currentUser);

        _db.Users.AddRange(
            new User { FullName = "Sarah Nkosi", Username = "sarah.n", Role = UserRole.Supervisor, PasswordHash = PasswordHasher.Hash("sarah123") },
            new User { FullName = "Deactivated Dave", Username = "dave.d", Role = UserRole.Technician, PasswordHash = PasswordHasher.Hash("dave123"), IsActive = false },
            new User { FullName = "Mandla Manager", Username = "mandla.m", Role = UserRole.MaintenanceManager, PasswordHash = PasswordHasher.Hash("mandla123") },
            new User { FullName = "Ada Admin", Username = "ada.a", Role = UserRole.Admin, PasswordHash = PasswordHasher.Hash("ada12345") }
        );
        _db.SaveChanges();
    }

    [Fact]
    public async Task AuthenticateAsync_WithCorrectCredentials_ReturnsTheUser()
    {
        var user = await _sut.AuthenticateAsync("sarah.n", "sarah123");

        Assert.NotNull(user);
        Assert.Equal("Sarah Nkosi", user!.FullName);
    }

    [Fact]
    public async Task AuthenticateAsync_WithWrongPassword_ReturnsNull()
    {
        var user = await _sut.AuthenticateAsync("sarah.n", "wrong-password");

        Assert.Null(user);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUnknownUsername_ReturnsNull()
    {
        var user = await _sut.AuthenticateAsync("nobody", "whatever");

        Assert.Null(user);
    }

    [Fact]
    public async Task AuthenticateAsync_ForDeactivatedUser_ReturnsNullEvenWithCorrectPassword()
    {
        var user = await _sut.AuthenticateAsync("dave.d", "dave123");

        Assert.Null(user);
    }

    private void SignInAs(string username) =>
        _currentUser.SignIn(_db.Users.Single(u => u.Username == username));

    [Fact]
    public async Task CreateUserAsync_AsMaintenanceManager_CreatesUserWhoCanThenLogIn()
    {
        SignInAs("mandla.m");

        var created = await _sut.CreateUserAsync("  Thabo Tech ", " thabo.t ", "s3cret-pass", UserRole.Technician);

        Assert.Equal("Thabo Tech", created.FullName);
        Assert.Equal("thabo.t", created.Username);
        Assert.NotEqual("s3cret-pass", created.PasswordHash); // stored hashed, never plaintext

        // Log back out and confirm the new account can actually authenticate.
        _currentUser.SignOut();
        var loggedIn = await _sut.AuthenticateAsync("thabo.t", "s3cret-pass");
        Assert.NotNull(loggedIn);
        Assert.Equal(UserRole.Technician, loggedIn!.Role);
    }

    [Fact]
    public async Task CreateUserAsync_AsSupervisor_IsRejected()
    {
        SignInAs("sarah.n");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.CreateUserAsync("New Person", "new.p", "longenough1", UserRole.Technician));
    }

    [Fact]
    public async Task CreateUserAsync_WhenNobodyIsSignedIn_IsRejected()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.CreateUserAsync("New Person", "new.p", "longenough1", UserRole.Technician));
    }

    [Fact]
    public async Task CreateUserAsync_ManagerCannotCreateAdmin()
    {
        SignInAs("mandla.m");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateUserAsync("Sneaky", "sneaky", "longenough1", UserRole.Admin));

        Assert.Contains("permission", ex.Message);
        Assert.DoesNotContain(UserRole.Admin, _sut.GetAssignableRoles());
    }

    [Fact]
    public async Task CreateUserAsync_AdminCanCreateAdmin()
    {
        SignInAs("ada.a");

        var created = await _sut.CreateUserAsync("Second Admin", "admin2", "longenough1", UserRole.Admin);

        Assert.Equal(UserRole.Admin, created.Role);
    }

    [Theory]
    [InlineData("SARAH.N")]  // same name, different case
    [InlineData("sarah.n")]
    public async Task CreateUserAsync_DuplicateUsername_IsRejectedCaseInsensitively(string duplicate)
    {
        SignInAs("mandla.m");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateUserAsync("Another Sarah", duplicate, "longenough1", UserRole.Supervisor));

        Assert.Contains("already taken", ex.Message);
    }

    [Theory]
    [InlineData("", "user1", "longenough1")]          // blank name
    [InlineData("Name", "", "longenough1")]           // blank username
    [InlineData("Name", "has space", "longenough1")]  // whitespace in username
    [InlineData("Name", "user1", "short")]            // password too short
    public async Task CreateUserAsync_InvalidInput_IsRejected(string fullName, string username, string password)
    {
        SignInAs("mandla.m");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateUserAsync(fullName, username, password, UserRole.Supervisor));
    }

    [Fact]
    public async Task GetAllAsync_AsSupervisor_IsRejected()
    {
        SignInAs("sarah.n");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_AsManager_ReturnsActiveAndInactiveUsers()
    {
        SignInAs("mandla.m");

        var all = await _sut.GetAllAsync();

        Assert.Contains(all, u => u.Username == "dave.d" && !u.IsActive);
        Assert.Contains(all, u => u.Username == "sarah.n");
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
