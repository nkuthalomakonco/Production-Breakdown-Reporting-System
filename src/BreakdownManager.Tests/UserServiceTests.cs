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
        _sut = new UserService(_db);

        _db.Users.AddRange(
            new User { FullName = "Sarah Nkosi", Username = "sarah.n", Role = UserRole.Supervisor, PasswordHash = PasswordHasher.Hash("sarah123") },
            new User { FullName = "Deactivated Dave", Username = "dave.d", Role = UserRole.Technician, PasswordHash = PasswordHasher.Hash("dave123"), IsActive = false }
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

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
