using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.EGS;
using GameFinder.StoreHandlers.Steam;
using Microsoft.Win32;
using RG39.Properties;
using System.Collections.Generic;

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
            }
            else if (FromLibrary.EpicGames == store)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\EOS");
                if (key is not null)
                {
                    Settings.Default.EGSPath = key.GetValue("ModSdkCommand").ToString();
                }
            }
        }

        internal static List<Game> GetGamesFromLib(FromLibrary from)
        {
            List<Game> mygames = new();

            if (FromLibrary.Steam == from)
            {
                SteamHandler handler = new(new WindowsRegistry());
                foreach ((SteamGame game, _) in handler.FindAllGames())
                {
                    // ToDo: Try to filter any soundtrack
                    if (game is not null && game.AppId != 228980 && !game.Name.Contains("Soundtrack"))
                    {
                        string path = game.Path + System.IO.Path.DirectorySeparatorChar + game.Name + ".url";
                        mygames.Add(new Game(from, game.AppId.ToString(), path));
                    }
                }
            }
            else if (FromLibrary.EpicGames == from)
            {
                EGSHandler handler = new();
                foreach ((EGSGame game, _) in handler.FindAllGames())
                {
                    if (game is not null)
                    {
                        string path = game.InstallLocation + System.IO.Path.DirectorySeparatorChar + game.DisplayName + ".url";
                        mygames.Add(new Game(from, game.CatalogItemId, path));
                    }
                }
            }
            return mygames;
        }
    }
}
