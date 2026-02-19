using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Core.Services;

namespace RandomGameLauncher.Core.Tests.Services;

public class BagRandomGameSelectorTests
{
    [Fact]
    public void PickNext_ReturnsOnlyActiveGames()
    {
        var selector = new BagRandomGameSelector(new WindowsPathNormalizer(), new Random(42));

        var games = new List<GameEntry>
        {
            new(GameSource.Other, string.Empty, @"C:\\Games\\A.exe", true),
            new(GameSource.Other, string.Empty, @"C:\\Games\\B.exe", false)
        };

        GameEntry? selected = selector.PickNext(games);

        Assert.NotNull(selected);
        Assert.True(selected!.Active);
        Assert.Equal(@"C:\\Games\\A.exe", selected.FilePath);
    }

    [Fact]
    public void PickNext_DoesNotRepeatUntilCycleEnds()
    {
        var selector = new BagRandomGameSelector(new WindowsPathNormalizer(), new Random(123));

        var games = new List<GameEntry>
        {
            new(GameSource.Other, string.Empty, @"C:\\Games\\A.exe", true),
            new(GameSource.Other, string.Empty, @"C:\\Games\\B.exe", true),
            new(GameSource.Other, string.Empty, @"C:\\Games\\C.exe", true)
        };

        var picks = new List<GameEntry?>
        {
            selector.PickNext(games),
            selector.PickNext(games),
            selector.PickNext(games)
        };

        Assert.All(picks, p => Assert.NotNull(p));

        int uniqueCount = picks
            .Select(p => p!.FilePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        Assert.Equal(3, uniqueCount);

        GameEntry? nextCyclePick = selector.PickNext(games);
        Assert.NotNull(nextCyclePick);
    }

    [Fact]
    public void PickNext_ReturnsNull_WhenNoActiveGames()
    {
        var selector = new BagRandomGameSelector(new WindowsPathNormalizer(), new Random(7));

        var games = new List<GameEntry>
        {
            new(GameSource.Other, string.Empty, @"C:\\Games\\A.exe", false)
        };

        GameEntry? selected = selector.PickNext(games);

        Assert.Null(selected);
    }
}
