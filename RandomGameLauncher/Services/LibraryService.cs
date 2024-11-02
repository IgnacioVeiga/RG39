using Microsoft.Win32;
using RandomGameLauncher.Models;
using RandomGameLauncher.Properties;
using System.IO;
using System.Text.Json;

namespace RandomGameLauncher.Services
{
    public static class LibraryService
    {
        public static void LocateStoreExeFromReg(LibraryEnum library)
        {
            if (LibraryEnum.Steam == library)
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
            else if (LibraryEnum.EpicGames == library)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\EOS");
                if (key is not null)
                {
                    Settings.Default.EpicGamesPath = key.GetValue("ModSdkCommand").ToString();
                }
                else
                {
                    Settings.Default.EpicGamesPath = string.Empty;
                }
            }
        }

        public static List<Game> GetGamesFromLib(LibraryEnum from)
        {
            List<Game> mygames = new();

            //if (LibraryEnum.Steam == from)
            //{
            //    SteamHandler steamHandler = new(new WindowsRegistry());
            //    foreach ((SteamGame game, _) in steamHandler.FindAllGames())
            //    {
            //        if (game is null || game.AppId == 0) continue;

            //        // Skip "Steamworks Common Redistributables"
            //        if (game.AppId == 228980) continue;

            //        // Try to filter any soundtrack
            //        if (game.Name.Contains("Soundtrack")) continue;
            //        if (game.Name.EndsWith(" OST")) continue;
            //        if (game.Name.EndsWith("-OST")) continue;

            //        // This is a fake filepath
            //        string path = $"{game.Path}{Path.DirectorySeparatorChar}{game.Name}.url";
            //        mygames.Add(new Game(from, game.AppId.ToString(), path));
            //    }
            //}
            //else if (LibraryEnum.EpicGames == from)
            //{
            //    EGSHandler handler = new();
            //    foreach ((EGSGame game, _) in handler.FindAllGames())
            //    {
            //        if (game is null) continue;

            //        // This is a fake filepath
            //        string path = $"{game.InstallLocation}{Path.DirectorySeparatorChar}{game.DisplayName}.url";
            //        mygames.Add(new Game(from, game.CatalogItemId, path));
            //    }
            //}
            return mygames;
        }

        public static void ClearList()
        {
            if (File.Exists($".{Path.DirectorySeparatorChar}list.json"))
                File.Delete($".{Path.DirectorySeparatorChar}list.json");
        }

        public static void SaveList(List<Game> games)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(games, options);
            File.WriteAllText($".{Path.DirectorySeparatorChar}list.json", json);
        }

        public static List<Game> ReadList()
        {
            List<Game> games = [];

            if (File.Exists($".{Path.DirectorySeparatorChar}list.json"))
            {
                string json = File.ReadAllText($".{Path.DirectorySeparatorChar}list.json");
                JsonSerializerOptions options = new() { WriteIndented = true };
                List<Game> list = JsonSerializer.Deserialize<List<Game>>(json, options);
                games.AddRange(list);
            }

            return games.Where(g => File.Exists(g.FilePath)).ToList();
        }
    }
}
