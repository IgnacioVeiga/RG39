using RandomGameLauncher.Views;

namespace RandomGameLauncher.Services;

public sealed class AddGameDialogService : IAddGameDialogService
{
    private readonly IExecutablePicker _executablePicker;

    public AddGameDialogService(IExecutablePicker executablePicker)
    {
        _executablePicker = executablePicker;
    }

    public AddGameDialogResult ShowDialog()
    {
        AddGameWindow addGameWindow = new(_executablePicker);
        bool? dialogResult = addGameWindow.ShowDialog();

        if (dialogResult != true)
        {
            return new AddGameDialogResult(false, string.Empty, string.Empty);
        }

        return new AddGameDialogResult(
            true,
            addGameWindow.SelectedFilePath,
            addGameWindow.SelectedLaunchArguments);
    }
}
