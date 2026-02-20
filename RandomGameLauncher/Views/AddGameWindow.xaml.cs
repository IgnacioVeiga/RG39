using RandomGameLauncher.ViewModels;
using System.Windows;

namespace RandomGameLauncher.Views;

/// <summary>
/// Interaction logic for AddGameWindow.xaml
/// </summary>
public partial class AddGameWindow : Window
{
    public AddGameWindow(AddGameViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
