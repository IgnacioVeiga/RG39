using RandomGameLauncher.Services;
using System.Windows;

namespace RandomGameLauncher.Views;

/// <summary>
/// Simple modal dialog for collecting manual game path and optional launch arguments.
/// This dialog intentionally uses code-behind to keep the feature easy to follow for beginners.
/// </summary>
public partial class AddGameWindow : Window
{
    private readonly ExecutableFilePicker _executablePicker;

    /// <summary>
    /// Constructor receiving the platform file picker used by the browse button.
    /// </summary>
    public AddGameWindow(ExecutableFilePicker executablePicker)
    {
        _executablePicker = executablePicker;
        InitializeComponent();
    }

    /// <summary>
    /// Selected executable path entered or chosen by the user.
    /// </summary>
    public string SelectedFilePath => SelectedFilePathTextBox.Text?.Trim() ?? string.Empty;

    /// <summary>
    /// Optional launch arguments entered by the user.
    /// </summary>
    public string LaunchArguments => LaunchArgumentsTextBox.Text ?? string.Empty;

    /// <summary>
    /// Opens the platform file picker and fills the path textbox when a file is selected.
    /// </summary>
    private void BrowseExecutable_Click(object sender, RoutedEventArgs e)
    {
        string? filePath = _executablePicker.SelectExecutableFile();

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            SelectedFilePathTextBox.Text = filePath;
        }
    }

    /// <summary>
    /// Closes the dialog only when a non-empty path is present.
    /// </summary>
    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SelectedFilePath))
        {
            return;
        }

        DialogResult = true;
    }
}
