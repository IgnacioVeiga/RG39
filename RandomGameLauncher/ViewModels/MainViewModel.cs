using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Core.Utilities;
using RandomGameLauncher.Models;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Services;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RandomGameLauncher.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IGameRepository _gameRepository;
    private readonly IReadOnlyList<IGameLibraryProvider> _gameLibraryProviders;
    private readonly IGameLauncher _gameLauncher;
    private readonly IRandomGameSelector _randomGameSelector;
    private readonly IPathNormalizer _pathNormalizer;
    private readonly IAddGameDialogService _addGameDialogService;
    private readonly IStorePathService _storePathService;

    private bool _isAllActiveChecked;
    private bool _isUpdatingActiveState;

    public ObservableCollectionEx<Game> Games { get; }

    public string GamesCount => $"{Games.Count} {Strings.GAMES.ToLower()}";

    public static BitmapImage EpicGamesIcon => Utils.ByteArrayToImage(Properties.Resources.EpicGames);
    public static BitmapImage SteamIcon => Utils.ByteArrayToImage(Properties.Resources.Steam);

    public bool IsAllActiveChecked
    {
        get => _isAllActiveChecked;
        set
        {
            if (_isAllActiveChecked == value)
            {
                return;
            }

            _isAllActiveChecked = value;
            OnPropertyChanged(nameof(IsAllActiveChecked));

            if (_isUpdatingActiveState)
            {
                return;
            }

            SetAllGamesActive(value);
        }
    }

    public ICommand PlayRandomGameCommand { get; }
    public ICommand AddGameCommand { get; }
    public ICommand RunGameCommand { get; }
    public ICommand RemoveGameCommand { get; }
    public ICommand ClearListCommand { get; }
    public ICommand HelpCommand { get; }
    public ICommand AboutCommand { get; }

    public MainViewModel(
        IGameRepository gameRepository,
        IEnumerable<IGameLibraryProvider> gameLibraryProviders,
        IGameLauncher gameLauncher,
        IRandomGameSelector randomGameSelector,
        IPathNormalizer pathNormalizer,
        IAddGameDialogService addGameDialogService,
        IStorePathService storePathService)
    {
        _gameRepository = gameRepository;
        _gameLibraryProviders = gameLibraryProviders.ToList();
        _gameLauncher = gameLauncher;
        _randomGameSelector = randomGameSelector;
        _pathNormalizer = pathNormalizer;
        _addGameDialogService = addGameDialogService;
        _storePathService = storePathService;

        Games = [];
        Games.CollectionChanged += Games_CollectionChanged;

        ConfigureStorePathStatus();
        LoadGames();

        PlayRandomGameCommand = new RelayCommand(PlayRandomGame);
        AddGameCommand = new RelayCommand(AddGame);
        RunGameCommand = new RelayCommand<Game>(RunGame);
        RemoveGameCommand = new RelayCommand<Game>(RemoveGame);
        ClearListCommand = new RelayCommand(ClearList);
        HelpCommand = new RelayCommand(HowToUse);
        AboutCommand = new RelayCommand(About);
    }

    private void PlayRandomGame()
    {
        List<Game> activeGames = Games.Where(g => g.Active).ToList();

        if (activeGames.Count == 0)
        {
            MessageBox.Show(Strings.CANNOT_LOAD_GAME_MSG, Strings.PLAY_GAME, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IReadOnlyList<GameEntry> activeEntries = activeGames.Select(ToCoreGameEntry).ToList();
        GameEntry? randomEntry = _randomGameSelector.PickNext(activeEntries);

        if (randomEntry is null)
        {
            MessageBox.Show(Strings.CANNOT_LOAD_GAME_MSG, Strings.PLAY_GAME, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string selectedId = GameIdentity.Build(randomEntry, _pathNormalizer);

        Game? gameToLaunch = activeGames.FirstOrDefault(g =>
            string.Equals(GameIdentity.Build(ToCoreGameEntry(g), _pathNormalizer), selectedId, StringComparison.OrdinalIgnoreCase));

        if (gameToLaunch is null)
        {
            return;
        }

        RunGame(gameToLaunch);
    }

    private void AddGame()
    {
        AddGameDialogResult dialogResult = _addGameDialogService.ShowDialog();
        if (!dialogResult.Accepted)
        {
            return;
        }

        string selectedFilePath = dialogResult.FilePath;
        string selectedLaunchArguments = dialogResult.LaunchArguments;

        if (!TryCreateManualGame(selectedFilePath, selectedLaunchArguments, out Game? newGame))
        {
            MessageBox.Show($"{Strings.CANNOT_LOAD_GAME_MSG}\n\"{selectedFilePath}\"");
            return;
        }

        bool alreadyExists = Games.Any(g =>
            g.From == LibraryEnum.Other &&
            _pathNormalizer.AreEquivalent(g.FilePath, newGame.FilePath));

        if (alreadyExists)
        {
            MessageBox.Show($"\"{newGame.FilePath}\"\n {Strings.REPEATED_GAME_MSG}", Strings.REPEATED_TITLE);
            return;
        }

        Games.Add(newGame);
        PersistManualGames();
        _randomGameSelector.ResetCycle();
    }

    private void RunGame(Game game)
    {
        if (game is null)
        {
            return;
        }

        try
        {
            _gameLauncher.LaunchAsync(ToCoreGameEntry(game)).GetAwaiter().GetResult();
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}\n-> '{game.Name}' <-");
        }
    }

    private void RemoveGame(Game game)
    {
        if (game is null || game.From != LibraryEnum.Other)
        {
            return;
        }

        string msg = $"{Strings.REMOVE_GAME_MSG}\n{game.Name}?";
        MessageBoxResult result = MessageBox.Show(msg, string.Empty, MessageBoxButton.YesNo);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        Games.Remove(game);
        PersistManualGames();
        _randomGameSelector.ResetCycle();
    }

    private void ClearList()
    {
        MessageBoxResult msgResult = MessageBox.Show(Strings.CLEAR_LIST_MSG, Strings.CLEAR_LIST, MessageBoxButton.YesNo);

        if (msgResult != MessageBoxResult.Yes)
        {
            return;
        }

        for (int i = Games.Count - 1; i >= 0; i--)
        {
            if (Games[i].From == LibraryEnum.Other)
            {
                Games.RemoveAt(i);
            }
        }

        _gameRepository.ClearAsync().GetAwaiter().GetResult();
        _randomGameSelector.ResetCycle();
    }

    private void SetAllGamesActive(bool isChecked)
    {
        _isUpdatingActiveState = true;

        try
        {
            foreach (Game game in Games)
            {
                game.Active = isChecked;
            }
        }
        finally
        {
            _isUpdatingActiveState = false;
        }

        PersistManualGames();
        _randomGameSelector.ResetCycle();
        RecalculateHeaderCheckbox();
    }

    private void About()
    {
        new AboutWindow().ShowDialog();
    }

    private void HowToUse()
    {
        string url = "https://github.com/IgnacioVeiga/RandomGameLauncher/blob/master/README";

        switch (Settings.Default.Language)
        {
            case "en":
                url += ".md#how-to-use";
                break;

            case "es":
                url += "_es.md#como-usar";
                break;

            default:
                return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = url
        });
    }

    private void LoadGames()
    {
        HashSet<string> knownIds = new(StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<StoredGame> storedGames = _gameRepository.LoadAsync().GetAwaiter().GetResult();
        foreach (StoredGame storedGame in storedGames)
        {
            if (!TryCreateManualGame(storedGame.FilePath, storedGame.LaunchArguments, out Game? manualGame))
            {
                continue;
            }

            manualGame.Active = storedGame.Active;
            TryAddGame(manualGame, knownIds);
        }

        foreach (IGameLibraryProvider provider in _gameLibraryProviders)
        {
            IReadOnlyList<GameEntry> discoveredGames;

            try
            {
                discoveredGames = provider.GetInstalledGamesAsync().GetAwaiter().GetResult();
            }
            catch
            {
                discoveredGames = [];
            }

            foreach (GameEntry discoveredGame in discoveredGames)
            {
                if (!TryCreateLibraryGame(discoveredGame, out Game? game))
                {
                    continue;
                }

                TryAddGame(game, knownIds);
            }
        }

        OnPropertyChanged(nameof(GamesCount));
        RecalculateHeaderCheckbox();
    }

    private void ConfigureStorePathStatus()
    {
        Settings.Default.SteamPath = BuildStorePathStatusLabel("Steam", _storePathService.GetStorePath(GameSource.Steam));
        Settings.Default.EpicGamesPath = BuildStorePathStatusLabel("Epic Games Store", _storePathService.GetStorePath(GameSource.EpicGames));
        Settings.Default.Save();
    }

    private static string BuildStorePathStatusLabel(string storeName, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return $"{storeName}: {Strings.NOT_FOUND_MSG}";
        }

        return path;
    }

    private bool TryCreateManualGame(string? rawPath, string? launchArguments, out Game? game)
    {
        game = null;

        string? normalizedPath = _pathNormalizer.NormalizeAbsolutePath(rawPath);

        if (string.IsNullOrWhiteSpace(normalizedPath) ||
            !string.Equals(Path.GetExtension(normalizedPath), ".exe", StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(normalizedPath))
        {
            return false;
        }

        game = new Game(LibraryEnum.Other, string.Empty, normalizedPath)
        {
            Active = true
        };
        game.LaunchArguments = launchArguments ?? string.Empty;

        return true;
    }

    private static bool TryCreateLibraryGame(GameEntry coreGame, out Game? game)
    {
        game = null;

        if (string.IsNullOrWhiteSpace(coreGame.FilePath))
        {
            return false;
        }

        LibraryEnum from = coreGame.Source switch
        {
            GameSource.Steam => LibraryEnum.Steam,
            GameSource.EpicGames => LibraryEnum.EpicGames,
            _ => LibraryEnum.Other
        };

        game = new Game(from, coreGame.GameId, coreGame.FilePath)
        {
            Active = coreGame.Active
        };
        game.LaunchArguments = coreGame.LaunchArguments ?? string.Empty;

        return true;
    }

    private void PersistManualGames()
    {
        List<StoredGame> gamesToPersist = Games
            .Where(g => g.From == LibraryEnum.Other)
            .Select(g => new StoredGame(g.FilePath, g.Active, g.LaunchArguments))
            .ToList();

        _gameRepository.SaveAsync(gamesToPersist).GetAwaiter().GetResult();
    }

    private void TryAddGame(Game game, ISet<string> knownIds)
    {
        string id = GameIdentity.Build(ToCoreGameEntry(game), _pathNormalizer);
        if (knownIds.Contains(id))
        {
            return;
        }

        knownIds.Add(id);
        Games.Add(game);
    }

    private static GameSource ToGameSource(LibraryEnum source) => source switch
    {
        LibraryEnum.Steam => GameSource.Steam,
        LibraryEnum.EpicGames => GameSource.EpicGames,
        _ => GameSource.Other
    };

    private static string GetGameId(Game game)
    {
        if (game.From == LibraryEnum.Other)
        {
            return string.Empty;
        }

        return game.GameId;
    }

    private static string GetComparablePath(Game game)
    {
        if (game.From == LibraryEnum.Other)
        {
            return game.FilePath;
        }

        return string.Empty;
    }

    private static GameEntry ToCoreGameEntry(Game game) =>
        new(ToGameSource(game.From), GetGameId(game), GetComparablePath(game), game.Active, game.LaunchArguments);

    private void Games_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (Game game in e.NewItems.OfType<Game>())
            {
                game.PropertyChanged += Game_PropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (Game game in e.OldItems.OfType<Game>())
            {
                game.PropertyChanged -= Game_PropertyChanged;
            }
        }

        OnPropertyChanged(nameof(GamesCount));
        RecalculateHeaderCheckbox();
    }

    private void Game_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        bool isActiveChanged = string.Equals(e.PropertyName, nameof(Game.Active), StringComparison.Ordinal);
        bool isLaunchArgsChanged = string.Equals(e.PropertyName, nameof(Game.LaunchArguments), StringComparison.Ordinal);

        if (!isActiveChanged && !isLaunchArgsChanged)
        {
            return;
        }

        if (isActiveChanged)
        {
            RecalculateHeaderCheckbox();
            _randomGameSelector.ResetCycle();

            if (_isUpdatingActiveState)
            {
                return;
            }
        }

        if (sender is Game game && game.From == LibraryEnum.Other)
        {
            PersistManualGames();
        }
    }

    private void RecalculateHeaderCheckbox()
    {
        bool areAllActive = Games.Count > 0 && Games.All(g => g.Active);

        _isUpdatingActiveState = true;

        try
        {
            if (_isAllActiveChecked != areAllActive)
            {
                _isAllActiveChecked = areAllActive;
                OnPropertyChanged(nameof(IsAllActiveChecked));
            }
        }
        finally
        {
            _isUpdatingActiveState = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
