using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Models;
using System.Diagnostics.CodeAnalysis;

namespace RandomGameLauncher.Services;

/// <summary>
/// Application-layer catalog orchestration contract used by MainViewModel.
/// </summary>
public interface IGameCatalogService
{
    /// <summary>
    /// Loads a merged and deduplicated catalog from manual storage and store providers.
    /// </summary>
    Task<IReadOnlyList<Game>> LoadGamesAsync(CancellationToken ct = default);

    /// <summary>
    /// Persists only manual games from the provided UI snapshot.
    /// </summary>
    Task SaveManualGamesAsync(IEnumerable<Game> games, CancellationToken ct = default);

    /// <summary>
    /// Clears persisted manual games.
    /// </summary>
    Task ClearManualGamesAsync(CancellationToken ct = default);

    /// <summary>
    /// Validates a manual executable path and creates a UI game model when valid.
    /// </summary>
    bool TryCreateManualGame(string? rawPath, string? launchArguments, [NotNullWhen(true)] out Game? game);

    /// <summary>
    /// Builds a stable identity string from a UI game model.
    /// </summary>
    string BuildIdentity(Game game);

    /// <summary>
    /// Builds a stable identity string from a core game entry.
    /// </summary>
    string BuildIdentity(GameEntry gameEntry);

    /// <summary>
    /// Maps a UI game model to the core transport model.
    /// </summary>
    GameEntry ToCoreGameEntry(Game game);
}
