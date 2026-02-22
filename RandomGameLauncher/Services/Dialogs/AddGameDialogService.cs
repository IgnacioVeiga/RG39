using RandomGameLauncher.Views;

namespace RandomGameLauncher.Services;

/// <summary>
/// Modal dialog adapter used by MainViewModel to gather manual game inputs.
/// </summary>
public sealed class AddGameDialogService : IAddGameDialogService
{
    private readonly ExecutableFilePicker _executablePicker;

    /// <summary>
    /// Constructor receiving the file picker dependency used by the dialog.
    /// </summary>
    public AddGameDialogService(ExecutableFilePicker executablePicker)
    {
        _executablePicker = executablePicker;
    }

    /// <summary>
    /// Creates the dialog, blocks until the user closes it, and maps the result to a DTO.
    /// </summary>
    public AddGameDialogResult ShowDialog()
    {
        AddGameWindow dialog = new(_executablePicker);
        bool? dialogResult = dialog.ShowDialog();

        if (dialogResult != true)
        {
            return new AddGameDialogResult(false, string.Empty, string.Empty);
        }

        return new AddGameDialogResult(
            true,
            dialog.SelectedFilePath,
            dialog.LaunchArguments);
    }
}
