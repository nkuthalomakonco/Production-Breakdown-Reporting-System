using BreakdownManager.Domain.Enums;
using BreakdownManager.Services.Dtos;
using BreakdownManager.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace BreakdownManager.App.ViewModels;

/// <summary>
/// The maintenance manager's dashboard: the KPI numbers <see cref="IBreakdownService.GetDashboardStatsAsync"/>
/// already computed, plus three charts (top problem machines, category split, daily volume trend)
/// built on top of the same call — no separate "just for charts" queries.
/// </summary>
public partial class ManagerDashboardViewModel : ObservableObject
{
    private readonly IBreakdownService _breakdownService;

    // --- KPI tiles ---
    [ObservableProperty] private int openBreakdowns;
    [ObservableProperty] private int inProgress;
    [ObservableProperty] private int waitingForParts;
    [ObservableProperty] private int completedToday;
    [ObservableProperty] private int machinesDown;
    [ObservableProperty] private string averageResponseTimeDisplay = "—";
    [ObservableProperty] private string averageMttrDisplay = "—";
    [ObservableProperty] private string downtimeTodayDisplay = "—";

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? statusMessage;

    // --- Charts (LiveCharts2) ---
    public ISeries[] TopMachinesSeries { get; private set; } = Array.Empty<ISeries>();
    public Axis[] TopMachinesXAxes { get; private set; } = Array.Empty<Axis>();

    public ISeries[] CategorySeries { get; private set; } = Array.Empty<ISeries>();

    public ISeries[] StatusSeries { get; private set; } = Array.Empty<ISeries>();
    public Axis[] StatusXAxes { get; private set; } = Array.Empty<Axis>();

    public ISeries[] TrendSeries { get; private set; } = Array.Empty<ISeries>();
    public Axis[] TrendXAxes { get; private set; } = Array.Empty<Axis>();

    public ManagerDashboardViewModel(IBreakdownService breakdownService)
    {
        _breakdownService = breakdownService;
    }

    public async Task InitializeAsync() => await RefreshAsync();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        StatusMessage = null;

        try
        {
            var stats = await _breakdownService.GetDashboardStatsAsync();
            ApplyKpis(stats);
            BuildTopMachinesChart(stats);
            BuildCategoryChart(stats);
            BuildStatusChart(stats);
            BuildTrendChart(stats);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't load dashboard data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyKpis(DashboardStats stats)
    {
        OpenBreakdowns = stats.OpenBreakdowns;
        InProgress = stats.InProgress;
        WaitingForParts = stats.WaitingForParts;
        CompletedToday = stats.CompletedToday;
        MachinesDown = stats.MachinesDown;
        AverageResponseTimeDisplay = FormatDuration(stats.AverageResponseTime);
        AverageMttrDisplay = FormatDuration(stats.AverageMttr);
        DowntimeTodayDisplay = FormatDuration(stats.DowntimeToday);
    }

    private void BuildTopMachinesChart(DashboardStats stats)
    {
        var machines = stats.TopProblemMachines;

        TopMachinesSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Breakdowns",
                Values = machines.Select(m => m.BreakdownCount).ToArray(),
                Fill = new SolidColorPaint(new SKColor(0x1E, 0x3A, 0x5F))
            }
        };

        TopMachinesXAxes = new[]
        {
            new Axis
            {
                Labels = machines.Select(m => m.MachineName).ToArray(),
                LabelsRotation = machines.Count > 4 ? 20 : 0
            }
        };

        OnPropertyChanged(nameof(TopMachinesSeries));
        OnPropertyChanged(nameof(TopMachinesXAxes));
    }

    private void BuildCategoryChart(DashboardStats stats)
    {
        // Empty categories are skipped here (unlike the KPI-safe list the service returns) so the
        // donut doesn't fill up with zero-width slices for fault types that have never occurred.
        CategorySeries = stats.BreakdownsByCategory
            .Where(c => c.Count > 0)
            .Select(c => (ISeries)new PieSeries<int>
            {
                Name = FormatCategoryName(c.Category),
                Values = new[] { c.Count },
                InnerRadius = 40
            })
            .ToArray();

        OnPropertyChanged(nameof(CategorySeries));
    }

    private void BuildStatusChart(DashboardStats stats)
    {
        var statuses = stats.BreakdownsByStatus.Where(s => s.Count > 0).ToList();

        // Fall back to a friendly empty state rather than an axis with nothing on it.
        if (statuses.Count == 0)
            statuses = new() { (BreakdownStatus.New, 0) };

        StatusSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Open tickets",
                Values = statuses.Select(s => s.Count).ToArray(),
                Fill = new SolidColorPaint(new SKColor(0xE6, 0x7E, 0x22))
            }
        };

        StatusXAxes = new[]
        {
            new Axis { Labels = statuses.Select(s => FormatStatusName(s.Status)).ToArray(), LabelsRotation = 20 }
        };

        OnPropertyChanged(nameof(StatusSeries));
        OnPropertyChanged(nameof(StatusXAxes));
    }

    private void BuildTrendChart(DashboardStats stats)
    {
        TrendSeries = new ISeries[]
        {
            new LineSeries<int>
            {
                Name = "Breakdowns reported",
                Values = stats.DailyTrend.Select(d => d.Count).ToArray(),
                Fill = null,
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(0x1E, 0x3A, 0x5F), 2)
            }
        };

        TrendXAxes = new[]
        {
            new Axis
            {
                Labels = stats.DailyTrend.Select(d => d.Date.ToString("MMM d")).ToArray(),
                LabelsRotation = 45
            }
        };

        OnPropertyChanged(nameof(TrendSeries));
        OnPropertyChanged(nameof(TrendXAxes));
    }

    private static string FormatDuration(TimeSpan? span)
    {
        if (span is null)
            return "—";

        var t = span.Value;
        if (t.TotalDays >= 1) return $"{(int)t.TotalDays}d {t.Hours}h";
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes}m";
        return $"{Math.Max(0, (int)t.TotalMinutes)}m";
    }

    private static string FormatCategoryName(FaultCategory category) => category switch
    {
        FaultCategory.Plc => "PLC",
        _ => category.ToString()
    };

    private static string FormatStatusName(BreakdownStatus status) => status switch
    {
        BreakdownStatus.WaitingForParts => "Waiting for Parts",
        _ => status.ToString()
    };
}
