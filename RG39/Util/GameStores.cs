using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.EGS;
using GameFinder.StoreHandlers.Steam;
using Microsoft.Win32;
using RG39.Entities;
using RG39.Properties;
using System.Collections.Generic;
using System.IO;

namespace RG39.Util
{
    public static class GameStores
    {
        public enum FromLibrary
        {
            Other = 0,
            Steam = 1,
            EpicGames = 2
        }

        public static void LocateStoreExeFromReg(FromLibrary store)
        {
            if (FromLibrary.Steam == store)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
                if (key is not null)
                {
                    Settings.Default.SteamPath = key.GetValue("SteamExe").ToString();
                }
                else
                {
                    Settings.Default.SteamPath = string.Empty;
                }
            }
            else if (FromLibrary.EpicGames == store)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\EOS");
                if (key is not null)
                {
                    Settings.Default.EGSPath = key.GetValue("ModSdkCommand").ToString();
                }
                else
                {
                    Settings.Default.EGSPath = string.Empty;
                }
            }
        }

        internal static List<Game> GetGamesFromLib(FromLibrary from)
        {
            List<Game> mygames = new();

            if (FromLibrary.Steam == from)
            {
                SteamHandler steamHandler = new(new WindowsRegistry());
                foreach ((SteamGame game, _) in steamHandler.FindAllGames())
                {
                    if (game.AppId == 0) continue;

                    // Skip "Steamworks Common Redistributables"
                    if (game.AppId == 228980) continue;

                    // ToDo: Try to filter any soundtrack
                    if (game.Name.Contains("Soundtrack")) continue;
                    if (game.Name.EndsWith(" OST")) continue;
                    if (game.Name.EndsWith("-OST")) continue;

                    // This is a fake filepath
                    string path = $"{game.Path}{Path.DirectorySeparatorChar}{game.Name}.url";
                    mygames.Add(new Game(from, game.AppId.ToString(), path));
                }
            }
            else if (FromLibrary.EpicGames == from)
            {
                EGSHandler handler = new();
                foreach ((EGSGame game, _) in handler.FindAllGames())
                {
                    if (game is null) continue;

                    // This is a fake filepath
                    string path = $"{game.InstallLocation}{Path.DirectorySeparatorChar}{game.DisplayName}.url";
                    mygames.Add(new Game(from, game.CatalogItemId, path));
                }
            }
            return mygames;
        }
    }
}
