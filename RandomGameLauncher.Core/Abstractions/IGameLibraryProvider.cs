using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Core.Abstractions;

public interface IGameLibraryProvider
{
    GameSource Source { get; }
    Task<IReadOnlyList<GameEntry>> GetInstalledGamesAsync(CancellationToken ct = default);
}
