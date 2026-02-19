using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Core.Abstractions;

public interface IRandomGameSelector
{
    GameEntry? PickNext(IReadOnlyList<GameEntry> activeGames);
    void ResetCycle();
}
