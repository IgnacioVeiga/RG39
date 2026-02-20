using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using System.Diagnostics;
using System.IO;

namespace RandomGameLauncher.Services;

/// <summary>
/// Windows-specific launcher that starts either local executables or store URIs.
/// </summary>
public sealed class WindowsGameLauncher : IGameLauncher
{
    public Task LaunchAsync(GameEntry game, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        switch (game.Source)
        {
            case GameSource.Other:
                LaunchExecutable(game.FilePath, game.LaunchArguments);
                break;

            case GameSource.Steam:
                LaunchUri($"steam://rungameid/{game.GameId}");
                break;

            case GameSource.EpicGames:
                LaunchUri($"com.epicgames.launcher://apps/{game.GameId}?action=launch&silent=true");
                break;
        }

        return Task.CompletedTask;
    }

    private static void LaunchExecutable(string executablePath, string launchArguments)
    {
        // UseShellExecute keeps launch behavior compatible with .url handlers and shell resolution.
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = executablePath,
            Arguments = launchArguments ?? string.Empty,
            WorkingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty
        });
    }

    private static void LaunchUri(string uri)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = uri
        });
    }
}
