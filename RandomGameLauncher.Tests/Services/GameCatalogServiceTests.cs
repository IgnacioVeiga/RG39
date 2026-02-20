using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Core.Services;
using RandomGameLauncher.Models;
using RandomGameLauncher.Services;

namespace RandomGameLauncher.Tests.Services;

public sealed class GameCatalogServiceTests
{
    [Fact]
    public async Task LoadGamesAsync_MergesSources_AndDeduplicatesEntries()
    {
        using TemporaryExecutable manualExe = TemporaryExecutable.Create();

        FakeGameRepository repository = new()
        {
            LoadResult =
            [
                new StoredGame(manualExe.ExecutablePath),
                new StoredGame(manualExe.ExecutablePath.ToUpperInvariant())
            ]
        };

        FakeGameLibraryProvider steamProvider = new(
            GameSource.Steam,
            [
                new GameEntry(GameSource.Steam, "10", @"C:\Steam\A.url"),
                new GameEntry(GameSource.Steam, "10", @"C:\Steam\B.url")
            ]);

        FakeGameLibraryProvider epicProvider = new(
            GameSource.EpicGames,
            [new GameEntry(GameSource.EpicGames, "epic-1", @"C:\Epic\A.url")]);

        GameCatalogService sut = new(repository, [steamProvider, epicProvider], new WindowsPathNormalizer());

        IReadOnlyList<Game> games = await sut.LoadGamesAsync();

        Assert.Equal(3, games.Count);
        Assert.Equal(1, games.Count(g => g.From == LibraryEnum.Other));
        Assert.Equal(1, games.Count(g => g.From == LibraryEnum.Steam));
        Assert.Equal(1, games.Count(g => g.From == LibraryEnum.EpicGames));
    }

    [Fact]
    public async Task SaveManualGamesAsync_PersistsOnlyManualEntries()
    {
        using TemporaryExecutable manualExe = TemporaryExecutable.Create();

        FakeGameRepository repository = new();
        GameCatalogService sut = new(repository, [], new WindowsPathNormalizer());

        List<Game> games =
        [
            new Game(LibraryEnum.Other, string.Empty, manualExe.ExecutablePath)
            {
                LaunchArguments = "--windowed"
            },
            new Game(LibraryEnum.Steam, "10", @"C:\Steam\A.url")
        ];

        await sut.SaveManualGamesAsync(games);

        Assert.Single(repository.LastSavedGames);
        Assert.Equal(manualExe.ExecutablePath, repository.LastSavedGames[0].FilePath, ignoreCase: true);
        Assert.Equal("--windowed", repository.LastSavedGames[0].LaunchArguments);
    }

    [Fact]
    public void TryCreateManualGame_ReturnsFalse_WhenPathIsInvalid()
    {
        FakeGameRepository repository = new();
        GameCatalogService sut = new(repository, [], new WindowsPathNormalizer());

        bool created = sut.TryCreateManualGame(@"C:\does-not-exist\game.exe", string.Empty, out Game? game);

        Assert.False(created);
        Assert.Null(game);
    }

    private sealed class FakeGameRepository : IGameRepository
    {
        public IReadOnlyList<StoredGame> LoadResult { get; set; } = [];
        public List<StoredGame> LastSavedGames { get; } = [];

        public Task<IReadOnlyList<StoredGame>> LoadAsync(CancellationToken ct = default) =>
            Task.FromResult(LoadResult);

        public Task SaveAsync(IReadOnlyList<StoredGame> games, CancellationToken ct = default)
        {
            LastSavedGames.Clear();
            LastSavedGames.AddRange(games);
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeGameLibraryProvider : IGameLibraryProvider
    {
        public GameSource Source { get; }
        private readonly IReadOnlyList<GameEntry> _games;

        public FakeGameLibraryProvider(GameSource source, IReadOnlyList<GameEntry> games)
        {
            Source = source;
            _games = games;
        }

        public Task<IReadOnlyList<GameEntry>> GetInstalledGamesAsync(CancellationToken ct = default) =>
            Task.FromResult(_games);
    }

    private sealed class TemporaryExecutable : IDisposable
    {
        public string ExecutablePath { get; }
        private readonly string _directoryPath;

        private TemporaryExecutable(string directoryPath, string executablePath)
        {
            _directoryPath = directoryPath;
            ExecutablePath = executablePath;
        }

        public static TemporaryExecutable Create()
        {
            string directoryPath = Path.Combine(Path.GetTempPath(), "RandomGameLauncherTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directoryPath);

            string executablePath = Path.Combine(directoryPath, "Game.exe");
            File.WriteAllText(executablePath, "test executable");

            return new TemporaryExecutable(directoryPath, executablePath);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_directoryPath))
                {
                    Directory.Delete(_directoryPath, true);
                }
            }
            catch
            {
                // Cleanup failure should not fail the test run.
            }
        }
    }
}
