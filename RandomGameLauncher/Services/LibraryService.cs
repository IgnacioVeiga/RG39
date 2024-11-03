using GameFinder.Common;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.EGS;
using GameFinder.StoreHandlers.Steam;
using Microsoft.Win32;
using NexusMods.Paths;
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
            List<Game> mygames = [];

            if (LibraryEnum.Steam == from)
            {
                SteamHandler steam_handler = new(FileSystem.Shared, WindowsRegistry.Shared);
                var steam_games = steam_handler.FindAllGamesById(out ErrorMessage[]? errors);

                foreach (var error in errors)
                {
                    // TODO: log and show errors
                }
                foreach (var steam_game in steam_games)
                {
                    // If app id is 0
                    if (steam_game.Key == 0) continue;

                    // Skip "Steamworks Common Redistributables"
                    if (steam_game.Key == 228980) continue;

                    // Try to filter any soundtrack
                    if (steam_game.Value.Name.Contains("Soundtrack")) continue;
                    if (steam_game.Value.Name.EndsWith(" OST")) continue;
                    if (steam_game.Value.Name.EndsWith("-OST")) continue;

                    // This is a fake filepath
                    string path = $"{steam_game.Value.Path}{Path.DirectorySeparatorChar}{steam_game.Value.Name}.url";
                    mygames.Add(new Game(from, steam_game.Key.ToString(), path));
                }
            }
            else if (LibraryEnum.EpicGames == from)
            {
                EGSHandler epicgames_handler = new(WindowsRegistry.Shared, FileSystem.Shared);
                var egs_games = epicgames_handler.FindAllGamesById(out ErrorMessage[]? errors);

                foreach (var error in errors)
                {
                    // TODO: log and show errors
                }
                foreach (var egs_game in egs_games)
                {
                    // This is a fake filepath
                    string path = $"{egs_game.Value.InstallLocation}{Path.DirectorySeparatorChar}{egs_game.Value.DisplayName}.url";
                    mygames.Add(new Game(from, egs_game.Value.CatalogItemId.ToString(), path));
                }
            }
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
