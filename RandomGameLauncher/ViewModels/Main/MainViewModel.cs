using RandomGameLauncher.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Models;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Services;
using System.Windows.Media.Imaging;

namespace RandomGameLauncher.ViewModels;

/// <summary>
/// Main application view model. This type coordinates user interactions and delegates
/// domain or infrastructure concerns to services so the UI state remains explicit.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IGameCatalogService _gameCatalogService;
    private readonly IGameLauncher _gameLauncher;
    private readonly IRandomGameSelector _randomGameSelector;
    private readonly IAddGameDialogService _addGameDialogService;
    private readonly IStorePathService _storePathService;

    private static readonly BitmapImage EpicGamesStatusIcon = Utils.ByteArrayToImage(Properties.Resources.EpicGames);
    private static readonly BitmapImage SteamStatusIcon = Utils.ByteArrayToImage(Properties.Resources.Steam);

    [ObservableProperty]
    private bool? _isAllActiveChecked;

    private bool _isUpdatingActiveState;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool _isLoading;

    private int _totalGamesCount;
    private int _activeGamesCount;

    public ObservableCollectionEx<Game> Games { get; }
    public IReadOnlyList<LanguageOptionItem> Languages { get; }

    public string GamesCount => $"{Games.Count} {Strings.GAMES.ToLower()}";
    public string StatusText => IsLoading ? $"{Strings.GAMES}..." : GamesCount;

    public static BitmapImage EpicGamesIcon => EpicGamesStatusIcon;
    public static BitmapImage SteamIcon => SteamStatusIcon;

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

        Languages = BuildLanguages(Settings.Default.Language);

        RunBackgroundTask(InitializeAsync);
    }
}
