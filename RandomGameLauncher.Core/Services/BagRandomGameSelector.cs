using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Core.Utilities;

namespace RandomGameLauncher.Core.Services;

public sealed class BagRandomGameSelector : IRandomGameSelector
{
    private readonly IPathNormalizer _pathNormalizer;
    private readonly Random _random;
    private readonly List<string> _remainingIds = [];

    public BagRandomGameSelector(IPathNormalizer pathNormalizer)
        : this(pathNormalizer, null)
    {
    }

    public BagRandomGameSelector(IPathNormalizer pathNormalizer, Random? random)
    {
        _pathNormalizer = pathNormalizer;
        _random = random ?? new Random();
    }

    public GameEntry? PickNext(IReadOnlyList<GameEntry> activeGames)
    {
        if (activeGames is null || activeGames.Count == 0)
        {
            return null;
        }

        var normalizedActiveGames = activeGames
            .Where(g => g.Active)
            .Select(g => new
            {
                Game = g,
                Id = GameIdentity.Build(g, _pathNormalizer)
            })
            .ToList();

        if (normalizedActiveGames.Count == 0)
        {
            return null;
        }

        var activeIds = normalizedActiveGames.Select(g => g.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Keep bag aligned with currently active games.
        _remainingIds.RemoveAll(id => !activeIds.Contains(id));

        if (_remainingIds.Count == 0)
        {
            FillBag(activeIds);
        }

        if (_remainingIds.Count == 0)
        {
            return null;
        }

        string selectedId = _remainingIds[0];
        _remainingIds.RemoveAt(0);

        return normalizedActiveGames.FirstOrDefault(g => string.Equals(g.Id, selectedId, StringComparison.OrdinalIgnoreCase))?.Game;
    }

    public void ResetCycle()
    {
        _remainingIds.Clear();
    }

    private void FillBag(HashSet<string> activeIds)
    {
        string[] orderedIds = activeIds.ToArray();

        // Fisher-Yates shuffle.
        for (int i = orderedIds.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (orderedIds[i], orderedIds[j]) = (orderedIds[j], orderedIds[i]);
        }

        foreach (string id in orderedIds)
        {
            _remainingIds.Add(id);
        }
    }
}
