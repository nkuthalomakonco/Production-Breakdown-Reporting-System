using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Services.Dtos;

namespace BreakdownManager.Services.Interfaces;

public interface IBreakdownService
{
    Task<Breakdown> ReportBreakdownAsync(NewBreakdownRequest request);

    Task<List<Breakdown>> GetOpenBreakdownsAsync();
    Task<List<Breakdown>> GetUnassignedBreakdownsAsync();
    Task<List<Breakdown>> GetAssignedToTechnicianAsync(int technicianUserId);
    Task<Breakdown?> GetByIdAsync(int id);

    Task AssignTechnicianAsync(int breakdownId, int technicianUserId);
    Task UpdateStatusAsync(int breakdownId, BreakdownStatus newStatus);
    Task CompleteRepairAsync(int breakdownId, string rootCause, string repairActions);
    Task CloseBreakdownAsync(int breakdownId);

    Task<DashboardStats> GetDashboardStatsAsync();
}
