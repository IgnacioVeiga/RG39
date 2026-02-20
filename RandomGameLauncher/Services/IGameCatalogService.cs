using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Models;
using System.Diagnostics.CodeAnalysis;

namespace RandomGameLauncher.Services;

public interface IGameCatalogService
{
    Task<IReadOnlyList<Game>> LoadGamesAsync(CancellationToken ct = default);
    Task SaveManualGamesAsync(IEnumerable<Game> games, CancellationToken ct = default);
    Task ClearManualGamesAsync(CancellationToken ct = default);
    bool TryCreateManualGame(string? rawPath, string? launchArguments, [NotNullWhen(true)] out Game? game);
    string BuildIdentity(Game game);
    string BuildIdentity(GameEntry gameEntry);
    GameEntry ToCoreGameEntry(Game game);
}
