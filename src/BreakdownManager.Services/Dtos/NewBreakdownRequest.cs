using BreakdownManager.Domain.Enums;

namespace BreakdownManager.Services.Dtos;

/// <summary>What the supervisor screen collects — deliberately minimal, so a report takes under a minute.</summary>
public class NewBreakdownRequest
{
    public int MachineId { get; set; }
    public int ReportedByUserId { get; set; }
    public FaultCategory Category { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string Description { get; set; } = string.Empty;
    public string? Shift { get; set; }
    public string? PhotoPath { get; set; }
}
