namespace RandomGameLauncher.Services;

public interface IAddGameDialogService
{
    AddGameDialogResult ShowDialog();
}

public readonly record struct AddGameDialogResult(bool Accepted, string FilePath, string LaunchArguments);
