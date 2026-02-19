using Microsoft.Extensions.DependencyInjection;
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
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static Mutex? _mutex;
    private ServiceProvider? _serviceProvider;

    public App()
    {
        AppLanguageService.ChangeLanguage(Settings.Default.Language);
    }

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

        _serviceProvider = ConfigureServices();

        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected virtual void CloseMutexHandler(object? sender, EventArgs e)
    {
        _serviceProvider?.Dispose();
        _mutex?.Close();
    }

    private static ServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddSingleton<IPathNormalizer, WindowsPathNormalizer>();
        services.AddSingleton<IRandomGameSelector, BagRandomGameSelector>();

        services.AddSingleton<IGameRepository, JsonGameRepository>();
        services.AddSingleton<IGameLauncher, WindowsGameLauncher>();
        services.AddSingleton<IStorePathService, StorePathService>();
        services.AddSingleton<IExecutablePicker, ExecutableFilePicker>();
        services.AddTransient<IAddGameDialogService, AddGameDialogService>();

        services.AddSingleton<IGameLibraryProvider, SteamGameLibraryProvider>();
        services.AddSingleton<IGameLibraryProvider, EpicGameLibraryProvider>();

        services.AddSingleton<MainViewModel>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }
}
