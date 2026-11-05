using System.Collections.ObjectModel;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Services;
using BreakdownManager.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BreakdownManager.App.ViewModels;

/// <summary>
/// Lets a Maintenance Manager (or Admin) see every account and add new ones. The real permission
/// check lives in <see cref="IUserService"/>; this screen is also only reachable by those roles.
/// </summary>
public partial class UserManagementViewModel : ObservableObject
{
    private readonly IUserService _userService;

    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<UserRole> AvailableRoles { get; } = new();

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;
    [ObservableProperty] private UserRole selectedRole = UserRole.Technician;

    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? successMessage;
    [ObservableProperty] private bool isBusy;

    /// <summary>Raised after a successful add so the view can blank its PasswordBoxes (which can't be data-bound).</summary>
    public event EventHandler? FormCleared;

    public string PasswordRequirementText => $"At least {UserService.MinPasswordLength} characters.";

    public UserManagementViewModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task InitializeAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        AvailableRoles.Clear();
        foreach (var role in _userService.GetAssignableRoles())
            AvailableRoles.Add(role);

        if (AvailableRoles.Count > 0 && !AvailableRoles.Contains(SelectedRole))
            SelectedRole = AvailableRoles[0];

        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            Users.Clear();
            foreach (var user in users)
                Users.Add(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private bool CanAddUser() =>
        !IsBusy
        && !string.IsNullOrWhiteSpace(FullName)
        && !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrEmpty(Password)
        && !string.IsNullOrEmpty(ConfirmPassword);

    [RelayCommand(CanExecute = nameof(CanAddUser))]
    private async Task AddUserAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "The two passwords don't match.";
            return;
        }

        IsBusy = true;
        try
        {
            var created = await _userService.CreateUserAsync(FullName, Username, Password, SelectedRole);

            SuccessMessage = $"Added {created.FullName} ({created.Username}) as {FormatRole(created.Role)}.";
            ClearForm();
            await LoadUsersAsync();
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearForm()
    {
        FullName = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        FormCleared?.Invoke(this, EventArgs.Empty);
    }

    public static string FormatRole(UserRole role) => role switch
    {
        UserRole.MaintenanceManager => "Maintenance Manager",
        _ => role.ToString()
    };

    partial void OnFullNameChanged(string value) => AddUserCommand.NotifyCanExecuteChanged();
    partial void OnUsernameChanged(string value) => AddUserCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => AddUserCommand.NotifyCanExecuteChanged();
    partial void OnConfirmPasswordChanged(string value) => AddUserCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool value) => AddUserCommand.NotifyCanExecuteChanged();
}
