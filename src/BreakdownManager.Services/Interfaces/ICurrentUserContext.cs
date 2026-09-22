using BreakdownManager.Domain.Entities;

namespace BreakdownManager.Services.Interfaces;

/// <summary>
/// Holds "who's using the app right now" for the lifetime of a login session.
/// Registered as a singleton so every screen sees the same signed-in user without
/// having to pass it around — this is what replaces the old per-screen "which
/// user am I working as" dropdown pickers.
/// </summary>
public interface ICurrentUserContext
{
    User? CurrentUser { get; }
    bool IsLoggedIn { get; }

    void SignIn(User user);
    void SignOut();

    /// <summary>Raised after SignIn or SignOut, so view models can react (e.g. re-run their InitializeAsync).</summary>
    event EventHandler? Changed;
}
