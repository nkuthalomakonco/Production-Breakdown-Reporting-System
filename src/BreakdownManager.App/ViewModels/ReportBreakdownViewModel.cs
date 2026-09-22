using System.Collections.ObjectModel;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Services.Dtos;
using BreakdownManager.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace BreakdownManager.App.ViewModels;

/// <summary>The supervisor's "Report Breakdown" screen. Goal: submit a ticket in under a minute.</summary>
public partial class ReportBreakdownViewModel : ObservableObject
{
    private readonly IBreakdownService _breakdownService;
    private readonly IMachineService _machineService;
    private readonly ICurrentUserContext _currentUserContext;

    public ObservableCollection<Machine> Machines { get; } = new();
    public List<FaultCategory> Categories { get; } = Enum.GetValues<FaultCategory>().ToList();
    public List<Priority> Priorities { get; } = Enum.GetValues<Priority>().ToList();

    /// <summary>Who's reporting — the logged-in user, no longer a dropdown pick.</summary>
    public User? Reporter => _currentUserContext.CurrentUser;

    [ObservableProperty]
    private Machine? selectedMachine;

    [ObservableProperty]
    private FaultCategory selectedCategory = FaultCategory.Mechanical;

    [ObservableProperty]
    private Priority selectedPriority = Priority.High;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string? photoPath;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private string? lastTicketNumber;

    public ReportBreakdownViewModel(IBreakdownService breakdownService, IMachineService machineService, ICurrentUserContext currentUserContext)
    {
        _breakdownService = breakdownService;
        _machineService = machineService;
        _currentUserContext = currentUserContext;
    }

    public async Task InitializeAsync()
    {
        if (Machines.Count == 0)
        {
            foreach (var machine in await _machineService.GetActiveMachinesAsync())
                Machines.Add(machine);
        }

        SelectedMachine ??= Machines.FirstOrDefault();
        OnPropertyChanged(nameof(Reporter));
        ReportBreakdownCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void BrowsePhoto()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Attach a photo of the fault",
            Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            PhotoPath = dialog.FileName;
        }
    }

    private bool CanReportBreakdown() =>
        SelectedMachine is not null && Reporter is not null && !string.IsNullOrWhiteSpace(Description);

    [RelayCommand(CanExecute = nameof(CanReportBreakdown))]
    private async Task ReportBreakdownAsync()
    {
        if (SelectedMachine is null || Reporter is null)
            return;

        var request = new NewBreakdownRequest
        {
            MachineId = SelectedMachine.Id,
            ReportedByUserId = Reporter.Id,
            Category = SelectedCategory,
            Priority = SelectedPriority,
            Description = Description,
            PhotoPath = PhotoPath
        };

        var created = await _breakdownService.ReportBreakdownAsync(request);

        LastTicketNumber = created.TicketNumber;
        StatusMessage = $"Ticket {created.TicketNumber} reported. Maintenance has been notified.";

        // Reset for the next report, but keep the same reporter and machine selected
        // since a supervisor often logs several faults on the same line in a row.
        Description = string.Empty;
        PhotoPath = null;
        SelectedPriority = Priority.High;
    }

    partial void OnSelectedMachineChanged(Machine? value) => ReportBreakdownCommand.NotifyCanExecuteChanged();
    partial void OnDescriptionChanged(string value) => ReportBreakdownCommand.NotifyCanExecuteChanged();
}
