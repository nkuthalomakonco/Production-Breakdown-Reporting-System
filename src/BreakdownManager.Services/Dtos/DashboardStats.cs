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
}
