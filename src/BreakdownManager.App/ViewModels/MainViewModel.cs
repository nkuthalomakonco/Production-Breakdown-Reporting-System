using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BreakdownManager.App.ViewModels;

/// <summary>
/// Top-level shell. Starts on the login screen; once <see cref="ICurrentUserContext"/> is
/// populated it switches to the Supervisor, Technician, and/or Manager screens, restricted by
/// the logged-in user's role, with a logout that drops back to login.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly LoginViewModel _loginViewModel;
    private readonly ReportBreakdownViewModel _reportBreakdownViewModel;
    private readonly TechnicianDashboardViewModel _technicianDashboardViewModel;
    private readonly ManagerDashboardViewModel _managerDashboardViewModel;
    private readonly ICurrentUserContext _currentUserContext;

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private string activeTab = string.Empty;

    [ObservableProperty]
    private bool isLoggedIn;

    [ObservableProperty]
    private string? loggedInUserDisplay;

    [ObservableProperty]
    private bool canShowSupervisorTab;

    [ObservableProperty]
    private bool canShowTechnicianTab;

    [ObservableProperty]
    private bool canShowManagerTab;

    public MainViewModel(
        LoginViewModel loginViewModel,
        ReportBreakdownViewModel reportBreakdownViewModel,
        TechnicianDashboardViewModel technicianDashboardViewModel,
        ManagerDashboardViewModel managerDashboardViewModel,
        ICurrentUserContext currentUserContext)
    {
        _loginViewModel = loginViewModel;
        _reportBreakdownViewModel = reportBreakdownViewModel;
        _technicianDashboardViewModel = technicianDashboardViewModel;
        _managerDashboardViewModel = managerDashboardViewModel;
        _currentUserContext = currentUserContext;

        _loginViewModel.LoginSucceeded += OnLoginSucceeded;

        ShowLoginScreen();
    }

    private void ShowLoginScreen()
    {
        IsLoggedIn = false;
        LoggedInUserDisplay = null;
        CanShowSupervisorTab = false;
        CanShowTechnicianTab = false;
        CanShowManagerTab = false;
        ActiveTab = string.Empty;
        _loginViewModel.Reset();
        CurrentView = _loginViewModel;
    }

    private void OnLoginSucceeded(object? sender, User user)
    {
        IsLoggedIn = true;
        LoggedInUserDisplay = $"{user.FullName} ({FormatRole(user.Role)})";

        // Supervisors report faults, technicians work them, managers (and admins) watch the KPIs
        // and can see both operational screens too rather than being locked out of either one.
        CanShowSupervisorTab = user.Role is UserRole.Supervisor or UserRole.MaintenanceManager or UserRole.Admin;
        CanShowTechnicianTab = user.Role is UserRole.Technician or UserRole.MaintenanceManager or UserRole.Admin;
        CanShowManagerTab = user.Role is UserRole.MaintenanceManager or UserRole.Admin;

        if (CanShowManagerTab)
            _ = ShowManagerViewAsync();
        else if (CanShowSupervisorTab)
            _ = ShowSupervisorViewAsync();
        else
            _ = ShowTechnicianViewAsync();
    }

    private static string FormatRole(UserRole role) => role switch
    {
        UserRole.MaintenanceManager => "Maintenance Manager",
        _ => role.ToString()
    };

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

    [RelayCommand]
    private async Task ShowManagerViewAsync()
    {
        ActiveTab = "Manager";
        CurrentView = _managerDashboardViewModel;
        await _managerDashboardViewModel.InitializeAsync();
    }

    [RelayCommand]
    private void Logout()
    {
        _currentUserContext.SignOut();
        ShowLoginScreen();
    }
}
