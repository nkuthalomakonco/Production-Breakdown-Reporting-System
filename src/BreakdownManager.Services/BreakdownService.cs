using BreakdownManager.Data;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Services.Dtos;
using BreakdownManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BreakdownManager.Services;

public class BreakdownService : IBreakdownService
{
    private readonly BreakdownManagerDbContext _db;

    public BreakdownService(BreakdownManagerDbContext db)
    {
        _db = db;
    }

    public async Task<Breakdown> ReportBreakdownAsync(NewBreakdownRequest request)
    {
        var breakdown = new Breakdown
        {
            TicketNumber = await GenerateTicketNumberAsync(),
            MachineId = request.MachineId,
            ReportedByUserId = request.ReportedByUserId,
            Category = request.Category,
            Priority = request.Priority,
            Description = request.Description,
            Shift = request.Shift,
            Status = BreakdownStatus.New,
            ReportedAt = DateTime.Now
        };

        if (!string.IsNullOrWhiteSpace(request.PhotoPath))
        {
            breakdown.Attachments.Add(new Attachment
            {
                FilePath = request.PhotoPath,
                Type = AttachmentType.Photo,
                UploadedAt = DateTime.Now
            });
        }

        _db.Breakdowns.Add(breakdown);
        await _db.SaveChangesAsync();
        return breakdown;
    }

    public async Task<List<Breakdown>> GetOpenBreakdownsAsync()
    {
        return await _db.Breakdowns
            .Include(b => b.Machine)
            .Include(b => b.AssignedTechnician)
            .Where(b => b.Status != BreakdownStatus.Closed)
            .OrderByDescending(b => b.Priority)
            .ThenBy(b => b.ReportedAt)
            .ToListAsync();
    }

    public async Task<List<Breakdown>> GetUnassignedBreakdownsAsync()
    {
        return await _db.Breakdowns
            .Include(b => b.Machine)
            .Where(b => b.Status == BreakdownStatus.New)
            .OrderByDescending(b => b.Priority)
            .ThenBy(b => b.ReportedAt)
            .ToListAsync();
    }

    public async Task<List<Breakdown>> GetAssignedToTechnicianAsync(int technicianUserId)
    {
        return await _db.Breakdowns
            .Include(b => b.Machine)
            .Where(b => b.AssignedTechnicianId == technicianUserId && b.Status != BreakdownStatus.Closed)
            .OrderByDescending(b => b.Priority)
            .ThenBy(b => b.ReportedAt)
            .ToListAsync();
    }

    public async Task<Breakdown?> GetByIdAsync(int id)
    {
        return await _db.Breakdowns
            .Include(b => b.Machine)
            .Include(b => b.AssignedTechnician)
            .Include(b => b.ReportedBy)
            .Include(b => b.Attachments)
            .Include(b => b.SparePartsUsed)
            .ThenInclude(sp => sp.SparePart)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task AssignTechnicianAsync(int breakdownId, int technicianUserId)
    {
        var breakdown = await RequireBreakdownAsync(breakdownId);
        breakdown.AssignedTechnicianId = technicianUserId;
        breakdown.AssignedAt = DateTime.Now;
        breakdown.Status = BreakdownStatus.Assigned;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int breakdownId, BreakdownStatus newStatus)
    {
        var breakdown = await RequireBreakdownAsync(breakdownId);
        breakdown.Status = newStatus;

        var now = DateTime.Now;
        switch (newStatus)
        {
            case BreakdownStatus.Travelling: breakdown.TravellingAt ??= now; break;
            case BreakdownStatus.Diagnosing: breakdown.DiagnosingAt ??= now; break;
            case BreakdownStatus.WaitingForParts: breakdown.WaitingForPartsAt ??= now; break;
            case BreakdownStatus.Repairing: breakdown.RepairingAt ??= now; break;
            case BreakdownStatus.Testing: breakdown.TestingAt ??= now; break;
            case BreakdownStatus.Completed: breakdown.CompletedAt ??= now; break;
            case BreakdownStatus.Closed: breakdown.ClosedAt ??= now; break;
        }

        await _db.SaveChangesAsync();
    }

    public async Task CompleteRepairAsync(int breakdownId, string rootCause, string repairActions)
    {
        var breakdown = await RequireBreakdownAsync(breakdownId);
        breakdown.RootCause = rootCause;
        breakdown.RepairActions = repairActions;
        breakdown.Status = BreakdownStatus.Completed;
        breakdown.CompletedAt ??= DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task CloseBreakdownAsync(int breakdownId)
    {
        var breakdown = await RequireBreakdownAsync(breakdownId);
        breakdown.Status = BreakdownStatus.Closed;
        breakdown.ClosedAt ??= DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var today = DateTime.Today;
        var all = await _db.Breakdowns.Include(b => b.Machine).ToListAsync();

        var open = all.Where(b => b.Status != BreakdownStatus.Closed).ToList();
        var completedToday = all.Where(b => b.CompletedAt?.Date == today).ToList();
        var reportedToday = all.Where(b => b.ReportedAt.Date == today).ToList();

        var responseTimes = all.Where(b => b.ResponseTime.HasValue).Select(b => b.ResponseTime!.Value).ToList();
        var mttrValues = all.Where(b => b.Mttr.HasValue).Select(b => b.Mttr!.Value).ToList();

        var topMachines = all
            .GroupBy(b => b.Machine.Name)
            .Select(g => (MachineName: g.Key, BreakdownCount: g.Count()))
            .OrderByDescending(x => x.BreakdownCount)
            .Take(10)
            .ToList();

        return new DashboardStats
        {
            OpenBreakdowns = open.Count,
            InProgress = open.Count(b => b.Status is BreakdownStatus.Travelling or BreakdownStatus.Diagnosing or BreakdownStatus.Repairing or BreakdownStatus.Testing),
            WaitingForParts = open.Count(b => b.Status == BreakdownStatus.WaitingForParts),
            CompletedToday = completedToday.Count,
            MachinesDown = open.Select(b => b.MachineId).Distinct().Count(),
            AverageResponseTime = responseTimes.Count > 0
                ? TimeSpan.FromTicks((long)responseTimes.Average(t => t.Ticks))
                : null,
            AverageMttr = mttrValues.Count > 0
                ? TimeSpan.FromTicks((long)mttrValues.Average(t => t.Ticks))
                : null,
            DowntimeToday = reportedToday.Count > 0
                ? TimeSpan.FromTicks(reportedToday.Sum(b => (b.Downtime ?? TimeSpan.Zero).Ticks))
                : TimeSpan.Zero,
            TopProblemMachines = topMachines
        };
    }

    private async Task<Breakdown> RequireBreakdownAsync(int id)
    {
        return await _db.Breakdowns.FindAsync(id)
            ?? throw new InvalidOperationException($"Breakdown {id} was not found.");
    }

    private async Task<string> GenerateTicketNumberAsync()
    {
        var year = DateTime.Now.Year;
        var countThisYear = await _db.Breakdowns.CountAsync(b => b.ReportedAt.Year == year);
        return $"BD-{year}-{(countThisYear + 1):D4}";
    }
}
