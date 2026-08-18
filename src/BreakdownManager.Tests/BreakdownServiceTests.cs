using BreakdownManager.Data;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Services;
using BreakdownManager.Services.Dtos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BreakdownManager.Tests;

/// <summary>
/// Uses a real (in-memory) SQLite connection rather than the EF InMemory provider, so these
/// tests exercise the actual SQL translation the app will run in production.
/// </summary>
public class BreakdownServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BreakdownManagerDbContext _db;
    private readonly BreakdownService _sut;

    public BreakdownServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BreakdownManagerDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new BreakdownManagerDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new BreakdownService(_db);

        SeedBasicData();
    }

    private void SeedBasicData()
    {
        _db.Machines.Add(new Machine { Id = 1, Code = "BAT-101", Name = "Lead Pasting Machine" });
        _db.Users.Add(new User { Id = 1, FullName = "Sarah Nkosi", Username = "sarah.n", Role = UserRole.Supervisor });
        _db.Users.Add(new User { Id = 2, FullName = "John Mokoena", Username = "john.m", Role = UserRole.Technician });
        _db.SaveChanges();
    }

    [Fact]
    public async Task ReportBreakdownAsync_GeneratesTicketNumber_WithCurrentYear()
    {
        var request = new NewBreakdownRequest
        {
            MachineId = 1,
            ReportedByUserId = 1,
            Category = FaultCategory.Mechanical,
            Priority = Priority.High,
            Description = "Conveyor stopped unexpectedly"
        };

        var breakdown = await _sut.ReportBreakdownAsync(request);

        Assert.StartsWith($"BD-{DateTime.Now.Year}-", breakdown.TicketNumber);
        Assert.Equal(BreakdownStatus.New, breakdown.Status);
    }

    [Fact]
    public async Task AssignTechnicianAsync_MovesTicketToAssigned_AndSetsAssignedAt()
    {
        var breakdown = await _sut.ReportBreakdownAsync(new NewBreakdownRequest
        {
            MachineId = 1,
            ReportedByUserId = 1,
            Category = FaultCategory.Electrical,
            Priority = Priority.Critical,
            Description = "Line 3 formation machine tripped"
        });

        await _sut.AssignTechnicianAsync(breakdown.Id, technicianUserId: 2);

        var updated = await _sut.GetByIdAsync(breakdown.Id);
        Assert.NotNull(updated);
        Assert.Equal(BreakdownStatus.Assigned, updated!.Status);
        Assert.Equal(2, updated.AssignedTechnicianId);
        Assert.NotNull(updated.AssignedAt);
    }

    [Fact]
    public async Task CompleteRepairAsync_SetsRootCauseAndCompletedAt()
    {
        var breakdown = await _sut.ReportBreakdownAsync(new NewBreakdownRequest
        {
            MachineId = 1,
            ReportedByUserId = 1,
            Category = FaultCategory.Plc,
            Priority = Priority.Medium,
            Description = "PLC fault code E204"
        });

        await _sut.AssignTechnicianAsync(breakdown.Id, 2);
        await _sut.CompleteRepairAsync(breakdown.Id, rootCause: "Loose terminal", repairActions: "Re-terminated and tested");

        var updated = await _sut.GetByIdAsync(breakdown.Id);
        Assert.Equal(BreakdownStatus.Completed, updated!.Status);
        Assert.Equal("Loose terminal", updated.RootCause);
        Assert.NotNull(updated.CompletedAt);
        Assert.NotNull(updated.Mttr);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
