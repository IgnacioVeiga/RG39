using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Models;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Services;
using RandomGameLauncher.Views;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RandomGameLauncher.ViewModels;

/// <summary>
/// Coordinates UI interactions while delegating catalog operations to application services.
/// Keeping orchestration explicit here makes behavior easier to evolve without leaking
/// persistence or discovery details into the view layer.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly IGameCatalogService _gameCatalogService;
    private readonly IGameLauncher _gameLauncher;
    private readonly IRandomGameSelector _randomGameSelector;
    private readonly IAddGameDialogService _addGameDialogService;
    private readonly IStorePathService _storePathService;
    private readonly AsyncRelayCommand _playRandomGameCommand;
    private readonly AsyncRelayCommand _addGameCommand;
    private readonly AsyncRelayCommand<Game> _runGameCommand;
    private readonly AsyncRelayCommand<Game> _removeGameCommand;
    private readonly AsyncRelayCommand _clearListCommand;

    private bool _isAllActiveChecked;
    private bool _isUpdatingActiveState;
    private bool _isLoading;

    public ObservableCollectionEx<Game> Games { get; }
    public IReadOnlyList<LanguageOptionItem> Languages { get; }

    public string GamesCount => $"{Games.Count} {Strings.GAMES.ToLower()}";
    public string StatusText => IsLoading ? $"{Strings.GAMES}..." : GamesCount;

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

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value)
            {
                return;
            }

            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(StatusText));
            RaiseCommandsCanExecuteChanged();
        }
    }

    public ICommand PlayRandomGameCommand => _playRandomGameCommand;
    public ICommand AddGameCommand => _addGameCommand;
    public ICommand RunGameCommand => _runGameCommand;
    public ICommand RemoveGameCommand => _removeGameCommand;
    public ICommand ClearListCommand => _clearListCommand;
    public ICommand HelpCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand ChangeLanguageCommand { get; }

    public MainViewModel(
        IGameCatalogService gameCatalogService,
        IGameLauncher gameLauncher,
        IRandomGameSelector randomGameSelector,
        IAddGameDialogService addGameDialogService,
        IStorePathService storePathService)
    {
        _gameCatalogService = gameCatalogService;
        _gameLauncher = gameLauncher;
        _randomGameSelector = randomGameSelector;
        _addGameDialogService = addGameDialogService;
        _storePathService = storePathService;

        Games = [];
        Games.CollectionChanged += Games_CollectionChanged;

        _playRandomGameCommand = new AsyncRelayCommand(PlayRandomGameAsync, CanUseInteractiveCommands, ShowUnhandledError);
        _addGameCommand = new AsyncRelayCommand(AddGameAsync, CanUseInteractiveCommands, ShowUnhandledError);
        _runGameCommand = new AsyncRelayCommand<Game>(RunGameAsync, g => g is not null && !IsLoading, ShowUnhandledError);
        _removeGameCommand = new AsyncRelayCommand<Game>(RemoveGameAsync, g => g is not null && g.From == LibraryEnum.Other && !IsLoading, ShowUnhandledError);
        _clearListCommand = new AsyncRelayCommand(ClearListAsync, CanUseInteractiveCommands, ShowUnhandledError);

        Languages = BuildLanguages(Settings.Default.Language);
        ChangeLanguageCommand = new RelayCommand<string?>(ChangeLanguage, CanChangeLanguage);
        HelpCommand = new RelayCommand(HowToUse);
        AboutCommand = new RelayCommand(About);

        RunBackgroundTask(InitializeAsync);
    }

    /// <summary>
    /// Initializes the view model without blocking the UI thread.
    /// Any failure is surfaced through a user-visible message while keeping the app responsive.
    /// </summary>
    private async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            ConfigureStorePathStatus();

            IReadOnlyList<Game> loadedGames = await _gameCatalogService.LoadGamesAsync();
            foreach (Game game in loadedGames)
            {
                Games.Add(game);
            }

            OnPropertyChanged(nameof(GamesCount));
            RecalculateHeaderCheckbox();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task PlayRandomGameAsync()
    {
        List<Game> activeGames = Games.Where(g => g.Active).ToList();

        if (activeGames.Count == 0)
        {
            MessageBox.Show(Strings.CANNOT_LOAD_GAME_MSG, Strings.PLAY_GAME, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IReadOnlyList<GameEntry> activeEntries = activeGames.Select(_gameCatalogService.ToCoreGameEntry).ToList();
        GameEntry? randomEntry = _randomGameSelector.PickNext(activeEntries);

        if (randomEntry is null)
        {
            MessageBox.Show(Strings.CANNOT_LOAD_GAME_MSG, Strings.PLAY_GAME, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string selectedId = _gameCatalogService.BuildIdentity(randomEntry);

        Game? gameToLaunch = activeGames.FirstOrDefault(g =>
            string.Equals(_gameCatalogService.BuildIdentity(g), selectedId, StringComparison.OrdinalIgnoreCase));

        if (gameToLaunch is null)
        {
            return;
        }

        await RunGameAsync(gameToLaunch);
    }

    private async Task AddGameAsync()
    {
        AddGameDialogResult dialogResult = _addGameDialogService.ShowDialog();
        if (!dialogResult.Accepted)
        {
            return;
        }

        string selectedFilePath = dialogResult.FilePath;
        string selectedLaunchArguments = dialogResult.LaunchArguments;

        if (!_gameCatalogService.TryCreateManualGame(selectedFilePath, selectedLaunchArguments, out Game? newGame))
        {
            MessageBox.Show($"{Strings.CANNOT_LOAD_GAME_MSG}\n\"{selectedFilePath}\"");
            return;
        }

        string newGameId = _gameCatalogService.BuildIdentity(newGame);
        bool alreadyExists = Games.Any(g =>
            g.From == LibraryEnum.Other &&
            string.Equals(_gameCatalogService.BuildIdentity(g), newGameId, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            MessageBox.Show($"\"{newGame.FilePath}\"\n {Strings.REPEATED_GAME_MSG}", Strings.REPEATED_TITLE);
            return;
        }

        Games.Add(newGame);
        await PersistManualGamesAsync();
        _randomGameSelector.ResetCycle();
    }

    private async Task RunGameAsync(Game? game)
    {
        if (game is null)
        {
            return;
        }

        try
        {
            await _gameLauncher.LaunchAsync(_gameCatalogService.ToCoreGameEntry(game));
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}\n-> '{game.Name}' <-");
        }
    }

    private async Task RemoveGameAsync(Game? game)
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
        await PersistManualGamesAsync();
        _randomGameSelector.ResetCycle();
    }

    private async Task ClearListAsync()
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

        await _gameCatalogService.ClearManualGamesAsync();
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

        RunBackgroundTask(PersistManualGamesAsync);
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

    private Task PersistManualGamesAsync()
    {
        // Save takes the full current snapshot to avoid incremental merge logic in the UI layer.
        return _gameCatalogService.SaveManualGamesAsync(Games);
    }

    private void RunBackgroundTask(Func<Task> taskFactory)
    {
        _ = RunBackgroundTaskCoreAsync(taskFactory);
    }

    private async Task RunBackgroundTaskCoreAsync(Func<Task> taskFactory)
    {
        try
        {
            await taskFactory();
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellations from stale background operations.
        }
        catch (Exception ex)
        {
            ShowUnhandledError(ex);
        }
    }

    private bool CanUseInteractiveCommands() => !IsLoading;

    private static IReadOnlyList<LanguageOptionItem> BuildLanguages(string? selectedLanguage) =>
        AppLanguageService.Languages
            .Select(language => new LanguageOptionItem(
                language.Key,
                language.Value,
                string.Equals(selectedLanguage, language.Key, StringComparison.OrdinalIgnoreCase)))
            .ToList();

    private bool CanChangeLanguage(string? language) =>
        !string.IsNullOrWhiteSpace(language) &&
        !string.Equals(language, Settings.Default.Language, StringComparison.OrdinalIgnoreCase);

    private void ChangeLanguage(string? language)
    {
        if (!CanChangeLanguage(language))
        {
            return;
        }

        AppLanguageService.ChangeLanguage(language);
        MessageBox.Show(Strings.TOGGLE_LANG_MSG, Strings.RESTARTING, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        App.RestartApp();
    }

    private void RaiseCommandsCanExecuteChanged()
    {
        _playRandomGameCommand.RaiseCanExecuteChanged();
        _addGameCommand.RaiseCanExecuteChanged();
        _runGameCommand.RaiseCanExecuteChanged();
        _removeGameCommand.RaiseCanExecuteChanged();
        _clearListCommand.RaiseCanExecuteChanged();
    }

    private static void ShowUnhandledError(Exception ex) =>
        MessageBox.Show(ex.Message);

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
        OnPropertyChanged(nameof(StatusText));
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
            RunBackgroundTask(PersistManualGamesAsync);
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
