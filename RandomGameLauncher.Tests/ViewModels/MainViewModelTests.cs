using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Models;
using RandomGameLauncher.Services;
using RandomGameLauncher.ViewModels;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RandomGameLauncher.Tests.ViewModels;

public sealed class MainViewModelTests
{
    [Fact]
    public async Task Constructor_LoadsCatalogInBackground_AndPopulatesGames()
    {
        using TemporaryExecutable executable = TemporaryExecutable.Create();

        FakeGameCatalogService catalogService = new()
        {
            LoadResult =
            [
                new Game(LibraryEnum.Other, string.Empty, executable.ExecutablePath)
            ]
        };

        MainViewModel sut = CreateViewModel(catalogService, new FakeAddGameDialogService());

        await WaitUntilAsync(() => sut.Games.Count == 1 && !sut.IsLoading);

        Assert.Single(sut.Games);
        Assert.Equal(sut.GamesCount, sut.StatusText);
    }

    [Fact]
    public async Task AddGameCommand_AddsGame_AndPersistsManualSnapshot()
    {
        using TemporaryExecutable executable = TemporaryExecutable.Create();

        FakeGameCatalogService catalogService = new();
        FakeAddGameDialogService addGameDialogService = new()
        {
            Result = new AddGameDialogResult(true, executable.ExecutablePath, "--safe")
        };

        MainViewModel sut = CreateViewModel(catalogService, addGameDialogService);
        await WaitUntilAsync(() => !sut.IsLoading);

        AsyncRelayCommand command = Assert.IsType<AsyncRelayCommand>(sut.AddGameCommand);
        await command.ExecuteAsync();

        Assert.Single(sut.Games);
        Assert.Equal("--safe", sut.Games[0].LaunchArguments);
        Assert.Equal(1, catalogService.SaveCalls);
        Assert.Single(catalogService.LastSavedSnapshot);
    }

    [Fact]
    public async Task HeaderCheckbox_UsesTriState_AndAppliesBulkToggle()
    {
        FakeGameCatalogService catalogService = new()
        {
            LoadResult =
            [
                new Game(LibraryEnum.Steam, "1", @"C:\Steam\A.url") { Active = true },
                new Game(LibraryEnum.Steam, "2", @"C:\Steam\B.url") { Active = true },
                new Game(LibraryEnum.Steam, "3", @"C:\Steam\C.url") { Active = false }
            ]
        };

        MainViewModel sut = CreateViewModel(catalogService, new FakeAddGameDialogService());
        await WaitUntilAsync(() => sut.Games.Count == 3 && !sut.IsLoading);

        Assert.Null(sut.IsAllActiveChecked);

        sut.IsAllActiveChecked = true;
        Assert.All(sut.Games, game => Assert.True(game.Active));
        Assert.True(sut.IsAllActiveChecked);

        sut.Games[0].Active = false;
        Assert.Null(sut.IsAllActiveChecked);

        sut.IsAllActiveChecked = false;
        Assert.All(sut.Games, game => Assert.False(game.Active));
        Assert.False(sut.IsAllActiveChecked);
    }

    private static MainViewModel CreateViewModel(
        IGameCatalogService catalogService,
        IAddGameDialogService addGameDialogService)
    {
        return new MainViewModel(
            catalogService,
            new FakeGameLauncher(),
            new FakeRandomGameSelector(),
            addGameDialogService,
            new FakeStorePathService());
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        Stopwatch timeout = Stopwatch.StartNew();

        while (timeout.ElapsedMilliseconds < timeoutMs)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        Assert.True(predicate(), "Condition was not reached within timeout.");
    }

    private sealed class FakeGameCatalogService : IGameCatalogService
    {
        public IReadOnlyList<Game> LoadResult { get; set; } = [];
        public int SaveCalls { get; private set; }
        public List<Game> LastSavedSnapshot { get; } = [];

        public Task<IReadOnlyList<Game>> LoadGamesAsync(CancellationToken ct = default) =>
            Task.FromResult(LoadResult);

        public Task SaveManualGamesAsync(IEnumerable<Game> games, CancellationToken ct = default)
        {
            SaveCalls++;
            LastSavedSnapshot.Clear();
            LastSavedSnapshot.AddRange(games.Where(g => g.From == LibraryEnum.Other));
            return Task.CompletedTask;
        }

        public Task ClearManualGamesAsync(CancellationToken ct = default) =>
            Task.CompletedTask;

        public bool TryCreateManualGame(string? rawPath, string? launchArguments, [NotNullWhen(true)] out Game? game)
        {
            game = null;

            if (string.IsNullOrWhiteSpace(rawPath) ||
                !string.Equals(Path.GetExtension(rawPath), ".exe", StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(rawPath))
            {
                return false;
            }

            string executablePath = rawPath;

            game = new Game(LibraryEnum.Other, string.Empty, executablePath)
            {
                LaunchArguments = launchArguments ?? string.Empty
            };
            return true;
        }

        public string BuildIdentity(Game game)
        {
            string key = game.From == LibraryEnum.Other ? game.FilePath : game.GameId;
            return $"{(int)ToSource(game.From)}|{key}";
        }

        public string BuildIdentity(GameEntry gameEntry)
        {
            string key = gameEntry.Source == GameSource.Other ? gameEntry.FilePath : gameEntry.GameId;
            return $"{(int)gameEntry.Source}|{key}";
        }

        public GameEntry ToCoreGameEntry(Game game) =>
            new(ToSource(game.From), game.From == LibraryEnum.Other ? string.Empty : game.GameId, game.From == LibraryEnum.Other ? game.FilePath : string.Empty, game.Active, game.LaunchArguments);

        private static GameSource ToSource(LibraryEnum source) => source switch
        {
            LibraryEnum.Steam => GameSource.Steam,
            LibraryEnum.EpicGames => GameSource.EpicGames,
            _ => GameSource.Other
        };
    }

    private sealed class FakeAddGameDialogService : IAddGameDialogService
    {
        public AddGameDialogResult Result { get; set; } = new(false, string.Empty, string.Empty);

        public AddGameDialogResult ShowDialog() => Result;
    }

    private sealed class FakeRandomGameSelector : IRandomGameSelector
    {
        public GameEntry? PickNext(IReadOnlyList<GameEntry> activeGames) =>
            activeGames.FirstOrDefault(g => g.Active);

        public void ResetCycle()
        {
        }
    }

    private sealed class FakeGameLauncher : IGameLauncher
    {
        public Task LaunchAsync(GameEntry game, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeStorePathService : IStorePathService
    {
        public string GetStorePath(GameSource source) =>
            source switch
            {
                GameSource.Steam => @"C:\Steam\steam.exe",
                GameSource.EpicGames => @"C:\Epic\launcher.exe",
                _ => string.Empty
            };
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
