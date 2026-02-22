using Microsoft.Win32;
using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Services;

/// <summary>
/// Reads launcher paths from known registry keys to show installation diagnostics in UI.
/// </summary>
public sealed class StorePathService : IStorePathService
{
    public string GetStorePath(GameSource source)
    {
        try
        {
            return source switch
            {
                GameSource.Steam => GetRegistryValue("Software\\Valve\\Steam", "SteamExe"),
                GameSource.EpicGames => GetRegistryValue("Software\\Epic Games\\EOS", "ModSdkCommand"),
                _ => string.Empty
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetRegistryValue(string keyPath, string valueName)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath);
        return key?.GetValue(valueName) as string ?? string.Empty;
    }
}
