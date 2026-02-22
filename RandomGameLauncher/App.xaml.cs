using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Services;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Services;
using RandomGameLauncher.ViewModels;
using System.Diagnostics;
using System.Windows;

namespace RandomGameLauncher;

/// <summary>
/// WPF application bootstrapper. It enforces single instance and composes dependencies.
/// </summary>
public partial class App : Application
{
    private static Mutex? _mutex;

    public App()
    {
        AppLanguageService.ChangeLanguage(Settings.Default.Language);
    }

    /// <summary>
    /// Restarts the current executable to apply settings that require a full reload.
    /// </summary>
    internal static void RestartApp()
    {
        try
        {
            Process.Start(Environment.ProcessPath!);
            Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    /// <summary>
    /// Startup routine with single-instance guard and manual object composition.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        const bool initiallyOwned = true;
        const string name = "RGL";

        _mutex = new Mutex(initiallyOwned, name, out bool createdNew);

        if (!createdNew)
        {
            MessageBox.Show(Strings.MULTI_INSTANCE_MSG);
            Current.Shutdown();
            return;
        }

        Exit += CloseMutexHandler;
        MainWindow mainWindow = CreateMainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <summary>
    /// Releases the singleton mutex on application shutdown.
    /// </summary>
    protected virtual void CloseMutexHandler(object? sender, EventArgs e)
    {
        _mutex?.Close();
    }

    /// <summary>
    /// Composition root for the app. Dependencies are built explicitly to keep startup flow easy to read.
    /// </summary>
    private static MainWindow CreateMainWindow()
    {
        IPathNormalizer pathNormalizer = new WindowsPathNormalizer();
        IRandomGameSelector randomGameSelector = new BagRandomGameSelector(pathNormalizer);

        IGameRepository gameRepository = new JsonGameRepository(pathNormalizer);
        IGameLauncher gameLauncher = new WindowsGameLauncher();
        IStorePathService storePathService = new StorePathService();
        ExecutableFilePicker executableFilePicker = new();
        IAddGameDialogService addGameDialogService = new AddGameDialogService(executableFilePicker);

        IGameLibraryProvider[] gameLibraryProviders =
        [
            new SteamGameLibraryProvider(),
            new EpicGameLibraryProvider()
        ];

        IGameCatalogService gameCatalogService = new GameCatalogService(
            gameRepository,
            gameLibraryProviders,
            pathNormalizer);

        MainViewModel mainViewModel = new(
            gameCatalogService,
            gameLauncher,
            randomGameSelector,
            addGameDialogService,
            storePathService);

        return new MainWindow(mainViewModel);
    }
}
