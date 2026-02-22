using RandomGameLauncher.Collections;
using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Models;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Services;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RandomGameLauncher.ViewModels;

/// <summary>
/// Main application view model. This type coordinates user interactions and delegates
/// domain or infrastructure concerns to services so the UI state remains explicit.
/// </summary>
public partial class MainViewModel : INotifyPropertyChanged
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
    private readonly RelayCommand _toggleAllActiveCommand;

    private static readonly BitmapImage EpicGamesStatusIcon = Utils.ByteArrayToImage(Properties.Resources.EpicGames);
    private static readonly BitmapImage SteamStatusIcon = Utils.ByteArrayToImage(Properties.Resources.Steam);

    private bool? _isAllActiveChecked;
    private bool _isUpdatingActiveState;
    private bool _isLoading;
    private int _totalGamesCount;
    private int _activeGamesCount;

    public ObservableCollectionEx<Game> Games { get; }
    public IReadOnlyList<LanguageOptionItem> Languages { get; }

    public string GamesCount => $"{Games.Count} {Strings.GAMES.ToLower()}";
    public string StatusText => IsLoading ? $"{Strings.GAMES}..." : GamesCount;

    public static BitmapImage EpicGamesIcon => EpicGamesStatusIcon;
    public static BitmapImage SteamIcon => SteamStatusIcon;

    public bool? IsAllActiveChecked
    {
        get => _isAllActiveChecked;
        private set
        {
            if (_isAllActiveChecked == value)
            {
                return;
            }

            _isAllActiveChecked = value;
            OnPropertyChanged(nameof(IsAllActiveChecked));
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
    public ICommand ToggleAllActiveCommand => _toggleAllActiveCommand;
    public ICommand HelpCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand ChangeLanguageCommand { get; }

    /// <summary>
    /// Constructor wiring command handlers and starting background initialization.
    /// </summary>
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
        _runGameCommand = new AsyncRelayCommand<Game>(RunGameAsync, game => game is not null && !IsLoading, ShowUnhandledError);
        _removeGameCommand = new AsyncRelayCommand<Game>(RemoveGameAsync, game => game is not null && game.From == LibraryEnum.Other && !IsLoading, ShowUnhandledError);
        _clearListCommand = new AsyncRelayCommand(ClearListAsync, CanUseInteractiveCommands, ShowUnhandledError);
        _toggleAllActiveCommand = new RelayCommand(ToggleAllActive, CanToggleAllActive);

        Languages = BuildLanguages(Settings.Default.Language);
        ChangeLanguageCommand = new RelayCommand<string?>(ChangeLanguage, CanChangeLanguage);
        HelpCommand = new RelayCommand(HowToUse);
        AboutCommand = new RelayCommand(About);

        RunBackgroundTask(InitializeAsync);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Shared property notification helper used across partial files.
    /// </summary>
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
