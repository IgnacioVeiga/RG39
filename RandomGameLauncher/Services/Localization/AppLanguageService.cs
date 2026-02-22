using RandomGameLauncher.Properties;
using System.Globalization;

namespace RandomGameLauncher.Services
{
    /// <summary>
    /// Centralizes language selection and persists it for next startup.
    /// </summary>
    public static class AppLanguageService
    {
        /// <summary>
        /// Supported UI language options mapped by culture key.
        /// </summary>
        public static readonly Dictionary<string, string> Languages = new()
        {
            { "en", "English" },
            { "es", "Español" }
        };

        /// <summary>
        /// Applies culture settings to the current thread and persists the preference.
        /// </summary>
        public static void ChangeLanguage(string? lang)
        {
            lang = (lang is null) ? "en" : lang;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            Settings.Default.Language = lang;
            Settings.Default.Save();
        }
    }
}
