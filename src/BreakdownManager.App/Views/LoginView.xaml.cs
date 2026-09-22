using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using BreakdownManager.App.ViewModels;

namespace BreakdownManager.App.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is INotifyPropertyChanged oldVm) oldVm.PropertyChanged -= ViewModel_PropertyChanged;
            if (e.NewValue is INotifyPropertyChanged newVm) newVm.PropertyChanged += ViewModel_PropertyChanged;
        };
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // A failed attempt sets ErrorMessage; clear the box rather than leaving a wrong password sitting in it.
        if (e.PropertyName == nameof(LoginViewModel.ErrorMessage) &&
            DataContext is LoginViewModel { ErrorMessage: not null })
        {
            PasswordInput.Clear();
        }
    }

    // PasswordBox.Password is deliberately not a bindable DependencyProperty (so a password
    // can't end up sitting in memory via the binding system longer than necessary), so it's
    // pushed into the view model by hand here instead.
    private void PasswordInput_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = PasswordInput.Password;
        }
    }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LoginViewModel viewModel && viewModel.LogInCommand.CanExecute(null))
        {
            viewModel.LogInCommand.Execute(null);
        }
    }
}
