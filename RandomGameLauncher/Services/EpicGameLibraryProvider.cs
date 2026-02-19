using Microsoft.Win32;
using NexusMods.Paths;
using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using System.IO;
using System.Text.Json;

namespace RandomGameLauncher.Services;

public sealed class EpicGameLibraryProvider : IGameLibraryProvider
{
    public GameSource Source => GameSource.EpicGames;

    public Task<IReadOnlyList<GameEntry>> GetInstalledGamesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            string? metadataDir = ReadMetadataDirectoryFromRegistry();
            if (string.IsNullOrWhiteSpace(metadataDir) || !Directory.Exists(metadataDir))
            {
                return Task.FromResult<IReadOnlyList<GameEntry>>([]);
            }

            AbsolutePath manifestFolder = FileSystem.Shared.FromUnsanitizedFullPath(metadataDir);

            AbsolutePath[] itemFiles;
            try
            {
                itemFiles = FileSystem.Shared.EnumerateFiles(manifestFolder, "*.item").ToArray();
            }
            catch
            {
                return Task.FromResult<IReadOnlyList<GameEntry>>([]);
            }

            List<GameEntry> games = [];

            foreach (AbsolutePath itemFile in itemFiles)
            {
                try
                {
                    using Stream stream = FileSystem.Shared.ReadFile(itemFile);
                    EgsGameManifest? game = JsonSerializer.Deserialize<EgsGameManifest>(stream);
                    if (game is null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(game.CatalogNamespace) ||
                        string.IsNullOrWhiteSpace(game.CatalogItemId) ||
                        string.IsNullOrWhiteSpace(game.AppName) ||
                        string.IsNullOrWhiteSpace(game.DisplayName) ||
                        string.IsNullOrWhiteSpace(game.InstallLocation))
                    {
                        continue;
                    }

                    string gameId = $"{game.CatalogNamespace}%3A{game.CatalogItemId}%3A{game.AppName}";
                    string fakePath = $"{game.InstallLocation}{Path.DirectorySeparatorChar}{game.DisplayName}.url";
                    games.Add(new GameEntry(Source, gameId, fakePath));
                }
                catch
                {
                    // Skip invalid manifest entries and keep loading the rest.
                }
            }

            return Task.FromResult<IReadOnlyList<GameEntry>>(games);
        }
        catch
        {
            return Task.FromResult<IReadOnlyList<GameEntry>>([]);
        }
    }

    private static string? ReadMetadataDirectoryFromRegistry()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\EOS");
        return key?.GetValue("ModSdkMetadataDir") as string;
    }

    private sealed record EgsGameManifest(
        string CatalogNamespace,
        string CatalogItemId,
        string AppName,
        string DisplayName,
        string InstallLocation);
}
