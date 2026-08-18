using System.Windows;
using BreakdownManager.App.ViewModels;

namespace BreakdownManager.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
