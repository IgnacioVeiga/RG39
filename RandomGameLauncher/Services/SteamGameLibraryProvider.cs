using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using NexusMods.Paths;
using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Services;

public sealed class SteamGameLibraryProvider : IGameLibraryProvider
{
    public GameSource Source => GameSource.Steam;

    public Task<IReadOnlyList<GameEntry>> GetInstalledGamesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            SteamHandler steamHandler = new(FileSystem.Shared, WindowsRegistry.Shared);
            var steamGames = steamHandler.FindAllGamesById(out _);

            List<GameEntry> games = [];

            foreach (var steamGame in steamGames)
            {
                if (steamGame.Key == 0)
                {
                    continue;
                }

                if (steamGame.Key == 228980)
                {
                    continue;
                }

                string gameName = steamGame.Value.Name;
                if (gameName.Contains("Soundtrack", StringComparison.OrdinalIgnoreCase) ||
                    gameName.EndsWith(" OST", StringComparison.OrdinalIgnoreCase) ||
                    gameName.EndsWith("-OST", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string path = $"{steamGame.Value.Path}{Path.DirectorySeparatorChar}{gameName}.url";
                games.Add(new GameEntry(Source, steamGame.Key.ToString(), path));
            }

            return Task.FromResult<IReadOnlyList<GameEntry>>(games);
        }
        catch
        {
            return Task.FromResult<IReadOnlyList<GameEntry>>([]);
        }
    }
}
