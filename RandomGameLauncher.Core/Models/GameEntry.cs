namespace RandomGameLauncher.Core.Models;

public sealed record GameEntry(
    GameSource Source,
    string GameId,
    string FilePath,
    bool Active = true,
    string LaunchArguments = "");
