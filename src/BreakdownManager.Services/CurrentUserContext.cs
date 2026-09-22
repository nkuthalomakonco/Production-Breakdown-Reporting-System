using BreakdownManager.Domain.Entities;
using BreakdownManager.Services.Interfaces;

namespace BreakdownManager.Services;

public class CurrentUserContext : ICurrentUserContext
{
    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null;

    public event EventHandler? Changed;

    public void SignIn(User user)
    {
        CurrentUser = user;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SignOut()
    {
        CurrentUser = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
