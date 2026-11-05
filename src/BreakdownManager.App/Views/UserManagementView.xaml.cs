using System.Windows.Controls;
using BreakdownManager.App.ViewModels;

namespace BreakdownManager.App.Views;

public partial class UserManagementView : UserControl
{
    public UserManagementView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is UserManagementViewModel oldVm) oldVm.FormCleared -= OnFormCleared;
            if (e.NewValue is UserManagementViewModel newVm) newVm.FormCleared += OnFormCleared;
        };
    }

    // PasswordBox.Password isn't a bindable DependencyProperty, so both boxes are pushed into the
    // view model by hand (same approach as the login screen) and cleared by hand after a save.
    private void PasswordInput_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is UserManagementViewModel vm)
            vm.Password = PasswordInput.Password;
    }

    private void ConfirmPasswordInput_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is UserManagementViewModel vm)
            vm.ConfirmPassword = ConfirmPasswordInput.Password;
    }

    private void OnFormCleared(object? sender, EventArgs e)
    {
        PasswordInput.Clear();
        ConfirmPasswordInput.Clear();
    }
}
