namespace RandomGameLauncher.Core.Models;

public sealed record StoredGame(
    string FilePath,
    bool Active = true,
    string LaunchArguments = "");
