using BreakdownManager.Domain.Entities;
using BreakdownManager.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BreakdownManager.App.ViewModels;

/// <summary>
/// The sign-in screen. Everything after this assumes <see cref="ICurrentUserContext"/> is
/// populated — the Supervisor and Technician screens no longer ask "who are you working as"
/// themselves, they just read it from there.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly ICurrentUserContext _currentUserContext;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isBusy;

    /// <summary>Raised once a username/password pair checks out. MainViewModel listens for this to switch screens.</summary>
    public event EventHandler<User>? LoginSucceeded;

    public LoginViewModel(IUserService userService, ICurrentUserContext currentUserContext)
    {
        _userService = userService;
        _currentUserContext = currentUserContext;
    }

    private bool CanLogIn() =>
        !IsBusy && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanLogIn))]
    private async Task LogInAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var user = await _userService.AuthenticateAsync(Username, Password);
            if (user is null)
            {
                ErrorMessage = "Incorrect username or password.";
                return;
            }

            _currentUserContext.SignIn(user);
            var loggedInUser = user;
            Reset();
            LoginSucceeded?.Invoke(this, loggedInUser);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Blanks the form. Called after a successful login and again on logout so the next person sees a clean screen.</summary>
    public void Reset()
    {
        Username = string.Empty;
        Password = string.Empty;
        ErrorMessage = null;
    }

    partial void OnUsernameChanged(string value) => LogInCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => LogInCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool value) => LogInCommand.NotifyCanExecuteChanged();
}
