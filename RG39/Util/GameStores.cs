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

        internal static List<GenericFile> GetGamesFromLib(FromLibrary from)
        {
            List<GenericFile> games = new();

            if (FromLibrary.Steam == from)
            {
                // use the Windows registry on Windows
                // Linux doesn't have a registry
                SteamHandler handler = new(new WindowsRegistry());
                foreach ((SteamGame game, _) in handler.FindAllGames())
                {
                    // ToDo: filter soundtracks
                    if (game is not null && game.AppId != 228980 && !game.Name.Contains("Soundtrack"))
                    {
                        games.Add(new GenericFile()
                        {
                            Active = true,
                            FileName = game.Name,
                            FilePath = game.Path,
                            From = FromLibrary.Steam,
                            SteamGameId = game.AppId
                        });
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
                        games.Add(new GenericFile()
                        {
                            Active = false,
                            FileName = game.DisplayName,
                            FilePath = game.InstallLocation,
                            From = FromLibrary.EpicGames,
                            EGSGameId = game.CatalogItemId
                        });
                    }
                }
            }
            return games;
        }
    }
}
