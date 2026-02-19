using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Core.Abstractions;

public interface IGameRepository
{
    Task<IReadOnlyList<StoredGame>> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(IReadOnlyList<StoredGame> games, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
}
