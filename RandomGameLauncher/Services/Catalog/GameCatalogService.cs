using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Core.Utilities;
using RandomGameLauncher.Models;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RandomGameLauncher.Services;

/// <summary>
/// Centralizes game catalog orchestration so the ViewModel stays focused on UI behavior.
/// This service owns loading, deduplication, mapping and manual persistence.
/// </summary>
public sealed class GameCatalogService : IGameCatalogService
{
    private readonly IGameRepository _gameRepository;
    private readonly IReadOnlyList<IGameLibraryProvider> _gameLibraryProviders;
    private readonly IPathNormalizer _pathNormalizer;
    private readonly SemaphoreSlim _manualPersistenceGate = new(1, 1);

    public GameCatalogService(
        IGameRepository gameRepository,
        IEnumerable<IGameLibraryProvider> gameLibraryProviders,
        IPathNormalizer pathNormalizer)
    {
        _gameRepository = gameRepository;
        _gameLibraryProviders = gameLibraryProviders.ToList();
        _pathNormalizer = pathNormalizer;
    }

    /// <summary>
    /// Loads persisted manual games and discovered store games into one deduplicated list.
    /// Discovery is best-effort: provider failures do not block the rest of the catalog.
    /// </summary>
    public async Task<IReadOnlyList<Game>> LoadGamesAsync(CancellationToken ct = default)
    {
        HashSet<string> knownIds = new(StringComparer.OrdinalIgnoreCase);
        List<Game> games = [];

        IReadOnlyList<StoredGame> storedGames = await _gameRepository.LoadAsync(ct);
        foreach (StoredGame storedGame in storedGames)
        {
            if (!TryCreateManualGame(storedGame.FilePath, storedGame.LaunchArguments, out Game? manualGame))
            {
                continue;
            }

            manualGame.Active = storedGame.Active;
            TryAddGame(manualGame, games, knownIds);
        }

        Task<IReadOnlyList<GameEntry>>[] providerTasks = _gameLibraryProviders
            .Select(provider => LoadProviderGamesSafelyAsync(provider, ct))
            .ToArray();

        IReadOnlyList<GameEntry>[] providerGameBatches = await Task.WhenAll(providerTasks);

        foreach (IReadOnlyList<GameEntry> providerGames in providerGameBatches)
        {
            foreach (GameEntry discoveredGame in providerGames)
            {
                if (!TryCreateLibraryGame(discoveredGame, out Game? game))
                {
                    continue;
                }

                TryAddGame(game, games, knownIds);
            }
        }

        return games;
    }

    /// <summary>
    /// Serializes manual list writes to prevent overlapping file operations when UI events
    /// trigger multiple saves in quick succession.
    /// </summary>
    public async Task SaveManualGamesAsync(IEnumerable<Game> games, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(games);
        ct.ThrowIfCancellationRequested();

        await _manualPersistenceGate.WaitAsync(ct);

        try
        {
            List<StoredGame> gamesToPersist = games
                .Where(g => g.From == LibraryEnum.Other)
                .Select(g => new StoredGame(g.FilePath, g.Active, g.LaunchArguments))
                .ToList();

            await _gameRepository.SaveAsync(gamesToPersist, ct);
        }
        finally
        {
            _manualPersistenceGate.Release();
        }
    }

    public async Task ClearManualGamesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await _manualPersistenceGate.WaitAsync(ct);

        try
        {
            await _gameRepository.ClearAsync(ct);
        }
        finally
        {
            _manualPersistenceGate.Release();
        }
    }

    public bool TryCreateManualGame(string? rawPath, string? launchArguments, [NotNullWhen(true)] out Game? game)
    {
        game = null;

        string? normalizedPath = _pathNormalizer.NormalizeAbsolutePath(rawPath);

        if (string.IsNullOrWhiteSpace(normalizedPath) ||
            !string.Equals(Path.GetExtension(normalizedPath), ".exe", StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(normalizedPath))
        {
            return false;
        }

        game = new Game(LibraryEnum.Other, string.Empty, normalizedPath)
        {
            Active = true,
            LaunchArguments = launchArguments ?? string.Empty
        };

        return true;
    }

    public string BuildIdentity(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        return BuildIdentity(ToCoreGameEntry(game));
    }

    public string BuildIdentity(GameEntry gameEntry) =>
        GameIdentity.Build(gameEntry, _pathNormalizer);

    public GameEntry ToCoreGameEntry(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        return new GameEntry(ToGameSource(game.From), GetGameId(game), GetComparablePath(game), game.Active, game.LaunchArguments);
    }

    private static async Task<IReadOnlyList<GameEntry>> LoadProviderGamesSafelyAsync(
        IGameLibraryProvider provider,
        CancellationToken ct)
    {
        try
        {
            return await provider.GetInstalledGamesAsync(ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return [];
        }
    }

    private void TryAddGame(Game game, ICollection<Game> games, ISet<string> knownIds)
    {
        string id = BuildIdentity(game);
        if (!knownIds.Add(id))
        {
            return;
        }

        games.Add(game);
    }

    private static bool TryCreateLibraryGame(GameEntry coreGame, [NotNullWhen(true)] out Game? game)
    {
        game = null;

        if (string.IsNullOrWhiteSpace(coreGame.FilePath))
        {
            return false;
        }

        LibraryEnum from = coreGame.Source switch
        {
            GameSource.Steam => LibraryEnum.Steam,
            GameSource.EpicGames => LibraryEnum.EpicGames,
            _ => LibraryEnum.Other
        };

        game = new Game(from, coreGame.GameId, coreGame.FilePath)
        {
            Active = coreGame.Active,
            LaunchArguments = coreGame.LaunchArguments ?? string.Empty
        };

        return true;
    }

    private static GameSource ToGameSource(LibraryEnum source) => source switch
    {
        LibraryEnum.Steam => GameSource.Steam,
        LibraryEnum.EpicGames => GameSource.EpicGames,
        _ => GameSource.Other
    };

    private static string GetGameId(Game game) =>
        game.From == LibraryEnum.Other ? string.Empty : game.GameId;

    private static string GetComparablePath(Game game) =>
        game.From == LibraryEnum.Other ? game.FilePath : string.Empty;
}
