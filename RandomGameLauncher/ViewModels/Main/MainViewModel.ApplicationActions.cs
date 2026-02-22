using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Services;
using RandomGameLauncher.Views;
using System.Diagnostics;
using System.Windows;

namespace RandomGameLauncher.ViewModels;

/// <summary>
/// Application-level actions unrelated to list mutation (help, about, language, diagnostics).
/// </summary>
public partial class MainViewModel
{
    /// <summary>
    /// Shows the modal about dialog.
    /// </summary>
    private void About() =>
        new AboutWindow().ShowDialog();

    /// <summary>
    /// Opens the language-specific README section in the user's default browser.
    /// </summary>
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

        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = url
        });
    }

    /// <summary>
    /// Reads known launcher installation paths and stores them in application settings.
    /// </summary>
    private void ConfigureStorePathStatus()
    {
        Settings.Default.SteamPath = BuildStorePathStatusLabel("Steam", _storePathService.GetStorePath(GameSource.Steam));
        Settings.Default.EpicGamesPath = BuildStorePathStatusLabel("Epic Games Store", _storePathService.GetStorePath(GameSource.EpicGames));
        Settings.Default.Save();
    }

    /// <summary>
    /// Builds the status-bar text for launcher paths when discovery fails.
    /// </summary>
    private static string BuildStorePathStatusLabel(string storeName, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return $"{storeName}: {Strings.NOT_FOUND_MSG}";
        }

        return path;
    }

    /// <summary>
    /// Validates if a language switch command should execute.
    /// </summary>
    private bool CanChangeLanguage(string? language) =>
        !string.IsNullOrWhiteSpace(language) &&
        !string.Equals(language, Settings.Default.Language, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Applies a new language and restarts the application so resources reload consistently.
    /// </summary>
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

    /// <summary>
    /// Projects available language options to UI-friendly items.
    /// </summary>
    private static IReadOnlyList<LanguageOptionItem> BuildLanguages(string? selectedLanguage) =>
        AppLanguageService.Languages
            .Select(language => new LanguageOptionItem(
                language.Key,
                language.Value,
                string.Equals(selectedLanguage, language.Key, StringComparison.OrdinalIgnoreCase)))
            .ToList();
}
