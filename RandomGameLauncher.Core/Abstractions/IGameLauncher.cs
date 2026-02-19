using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Core.Abstractions;

public interface IGameLauncher
{
    Task LaunchAsync(GameEntry game, CancellationToken ct = default);
}
