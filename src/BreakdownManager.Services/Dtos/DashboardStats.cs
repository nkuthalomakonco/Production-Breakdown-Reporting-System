using BreakdownManager.Domain.Enums;

namespace BreakdownManager.Services.Dtos;

/// <summary>Everything the supervisor / manager dashboard needs, computed in one shot.</summary>
public class DashboardStats
{
    public int OpenBreakdowns { get; set; }
    public int InProgress { get; set; }
    public int WaitingForParts { get; set; }
    public int CompletedToday { get; set; }
    public int MachinesDown { get; set; }
    public TimeSpan? AverageResponseTime { get; set; }
    public TimeSpan? AverageMttr { get; set; }
    public TimeSpan? DowntimeToday { get; set; }
    public List<(string MachineName, int BreakdownCount)> TopProblemMachines { get; set; } = new();

    /// <summary>Breakdown count per fault category, for the category pie/donut chart.</summary>
    public List<(FaultCategory Category, int Count)> BreakdownsByCategory { get; set; } = new();

    /// <summary>Open ticket count per workflow status, for the status bar chart.</summary>
    public List<(BreakdownStatus Status, int Count)> BreakdownsByStatus { get; set; } = new();

    /// <summary>One entry per day for the trailing window (oldest first), for the daily-volume line chart.</summary>
    public List<(DateTime Date, int Count)> DailyTrend { get; set; } = new();
}
