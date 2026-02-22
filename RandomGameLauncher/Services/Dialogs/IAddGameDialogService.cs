namespace RandomGameLauncher.Services;

/// <summary>
/// Abstraction for showing the add-game dialog and returning user input.
/// </summary>
public interface IAddGameDialogService
{
    /// <summary>
    /// Shows the modal dialog and returns whether it was accepted plus entered values.
    /// </summary>
    AddGameDialogResult ShowDialog();
}

public readonly record struct AddGameDialogResult(bool Accepted, string FilePath, string LaunchArguments);
