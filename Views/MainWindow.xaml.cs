using System.Windows;
using PersonalPropertyManager.ViewModels;

namespace PersonalPropertyManager.Views;

/// <summary>
/// Main window. The DataContext is injected via DI (see App.OnStartup) so
/// view-model unit tests can construct the window with a fake VM.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
