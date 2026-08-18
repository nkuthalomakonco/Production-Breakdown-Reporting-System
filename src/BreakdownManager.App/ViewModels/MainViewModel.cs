using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BreakdownManager.App.ViewModels;

/// <summary>
/// Top-level shell: lets you flip between the Supervisor and Technician screens.
/// In v1 there's no login yet, so this stands in for "which screen am I using right now" —
/// a proper per-user login can slot in here later without changing the child view models.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ReportBreakdownViewModel _reportBreakdownViewModel;
    private readonly TechnicianDashboardViewModel _technicianDashboardViewModel;

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private string activeTab = "Supervisor";

    public MainViewModel(
        ReportBreakdownViewModel reportBreakdownViewModel,
        TechnicianDashboardViewModel technicianDashboardViewModel)
    {
        _reportBreakdownViewModel = reportBreakdownViewModel;
        _technicianDashboardViewModel = technicianDashboardViewModel;

        CurrentView = _reportBreakdownViewModel;
        _ = _reportBreakdownViewModel.InitializeAsync();
    }

    [RelayCommand]
    private async Task ShowSupervisorViewAsync()
    {
        ActiveTab = "Supervisor";
        CurrentView = _reportBreakdownViewModel;
        await _reportBreakdownViewModel.InitializeAsync();
    }

    [RelayCommand]
    private async Task ShowTechnicianViewAsync()
    {
        ActiveTab = "Technician";
        CurrentView = _technicianDashboardViewModel;
        await _technicianDashboardViewModel.InitializeAsync();
    }
}
