using System.Collections.ObjectModel;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BreakdownManager.App.ViewModels;

/// <summary>The technician's dashboard: unassigned jobs to accept, plus their own jobs to work through.</summary>
public partial class TechnicianDashboardViewModel : ObservableObject
{
    private readonly IBreakdownService _breakdownService;
    private readonly ICurrentUserContext _currentUserContext;

    public ObservableCollection<Breakdown> UnassignedJobs { get; } = new();
    public ObservableCollection<Breakdown> MyJobs { get; } = new();

    // The stages a technician clicks through once a job is accepted.
    public List<BreakdownStatus> WorkflowStages { get; } = new()
    {
        BreakdownStatus.Travelling,
        BreakdownStatus.Diagnosing,
        BreakdownStatus.WaitingForParts,
        BreakdownStatus.Repairing,
        BreakdownStatus.Testing,
        BreakdownStatus.Completed
    };

    /// <summary>Who's working — the logged-in user, no longer a dropdown pick.</summary>
    public User? CurrentTechnician => _currentUserContext.CurrentUser;

    [ObservableProperty]
    private string? statusMessage;

    public TechnicianDashboardViewModel(IBreakdownService breakdownService, ICurrentUserContext currentUserContext)
    {
        _breakdownService = breakdownService;
        _currentUserContext = currentUserContext;
    }

    public async Task InitializeAsync()
    {
        OnPropertyChanged(nameof(CurrentTechnician));
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        UnassignedJobs.Clear();
        foreach (var job in await _breakdownService.GetUnassignedBreakdownsAsync())
            UnassignedJobs.Add(job);

        MyJobs.Clear();
        if (CurrentTechnician is not null)
        {
            foreach (var job in await _breakdownService.GetAssignedToTechnicianAsync(CurrentTechnician.Id))
                MyJobs.Add(job);
        }
    }

    [RelayCommand]
    private async Task AcceptJobAsync(Breakdown job)
    {
        if (CurrentTechnician is null)
        {
            StatusMessage = "You need to be logged in to accept a job.";
            return;
        }

        await _breakdownService.AssignTechnicianAsync(job.Id, CurrentTechnician.Id);
        StatusMessage = $"Accepted {job.TicketNumber}.";
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task AdvanceStatusAsync(Breakdown job)
    {
        var currentIndex = WorkflowStages.IndexOf(job.Status);
        var nextStage = currentIndex >= 0 && currentIndex < WorkflowStages.Count - 1
            ? WorkflowStages[currentIndex + 1]
            : WorkflowStages.First();

        await _breakdownService.UpdateStatusAsync(job.Id, nextStage);
        StatusMessage = $"{job.TicketNumber} moved to {nextStage}.";
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshCommandAsync() => await RefreshAsync();
}
