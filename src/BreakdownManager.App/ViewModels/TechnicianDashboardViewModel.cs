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
    private readonly IUserService _userService;

    public ObservableCollection<User> Technicians { get; } = new();
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

    [ObservableProperty]
    private User? currentTechnician;

    [ObservableProperty]
    private string? statusMessage;

    public TechnicianDashboardViewModel(IBreakdownService breakdownService, IUserService userService)
    {
        _breakdownService = breakdownService;
        _userService = userService;
    }

    public async Task InitializeAsync()
    {
        if (Technicians.Count == 0)
        {
            foreach (var tech in await _userService.GetByRoleAsync(UserRole.Technician))
                Technicians.Add(tech);

            CurrentTechnician ??= Technicians.FirstOrDefault();
        }

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
            StatusMessage = "Select which technician you're working as first.";
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

    partial void OnCurrentTechnicianChanged(User? value) => _ = RefreshAsync();
}
