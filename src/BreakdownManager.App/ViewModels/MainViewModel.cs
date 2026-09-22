using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BreakdownManager.App.ViewModels;

/// <summary>
/// Top-level shell. Starts on the login screen; once <see cref="ICurrentUserContext"/> is
/// populated it switches to the Supervisor and/or Technician screens, restricted by the
/// logged-in user's role, with a logout that drops back to login.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly LoginViewModel _loginViewModel;
    private readonly ReportBreakdownViewModel _reportBreakdownViewModel;
    private readonly TechnicianDashboardViewModel _technicianDashboardViewModel;
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

    public MainViewModel(
        LoginViewModel loginViewModel,
        ReportBreakdownViewModel reportBreakdownViewModel,
        TechnicianDashboardViewModel technicianDashboardViewModel,
        ICurrentUserContext currentUserContext)
    {
        _loginViewModel = loginViewModel;
        _reportBreakdownViewModel = reportBreakdownViewModel;
        _technicianDashboardViewModel = technicianDashboardViewModel;
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
        ActiveTab = string.Empty;
        _loginViewModel.Reset();
        CurrentView = _loginViewModel;
    }

    private void OnLoginSucceeded(object? sender, User user)
    {
        IsLoggedIn = true;
        LoggedInUserDisplay = $"{user.FullName} ({FormatRole(user.Role)})";

        // Supervisors report faults, technicians work them; managers and admins can see both
        // screens rather than being locked out of either one.
        CanShowSupervisorTab = user.Role is UserRole.Supervisor or UserRole.MaintenanceManager or UserRole.Admin;
        CanShowTechnicianTab = user.Role is UserRole.Technician or UserRole.MaintenanceManager or UserRole.Admin;

        if (CanShowSupervisorTab)
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
    private void Logout()
    {
        _currentUserContext.SignOut();
        ShowLoginScreen();
    }
}
