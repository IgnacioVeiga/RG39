using RandomGameLauncher.Views;
using RandomGameLauncher.ViewModels;

namespace RandomGameLauncher.Services;

/// <summary>
/// Isolates modal dialog wiring so ViewModels do not directly depend on window types.
/// </summary>
public sealed class AddGameDialogService : IAddGameDialogService
{
    private readonly IExecutablePicker _executablePicker;

    public AddGameDialogService(IExecutablePicker executablePicker)
    {
        _executablePicker = executablePicker;
    }

    public AddGameDialogResult ShowDialog()
    {
        AddGameViewModel viewModel = new(_executablePicker);
        AddGameWindow addGameWindow = new(viewModel);
        bool? dialogResult = addGameWindow.ShowDialog();

        if (dialogResult != true)
        {
            return new AddGameDialogResult(false, string.Empty, string.Empty);
        }

        return new AddGameDialogResult(
            true,
            viewModel.SelectedFilePath,
            viewModel.LaunchArguments);
    }
}
