using RandomGameLauncher.Services;
using RandomGameLauncher.ViewModels;
using System.Windows;

namespace RandomGameLauncher.Views;

/// <summary>
/// Interaction logic for AddGameWindow.xaml
/// </summary>
public partial class AddGameWindow : Window
{
    public AddGameWindow(IExecutablePicker executablePicker)
    {
        InitializeComponent();
        DataContext = new AddGameViewModel(executablePicker);
    }

    public string SelectedFilePath => ((AddGameViewModel)DataContext).SelectedFilePath;
    public string SelectedLaunchArguments => ((AddGameViewModel)DataContext).LaunchArguments;

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
