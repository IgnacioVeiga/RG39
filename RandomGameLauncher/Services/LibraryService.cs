using GameFinder.Common;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using Microsoft.Win32;
using NexusMods.Paths;
using RandomGameLauncher.Models;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using System.IO;
using System.Text.Json;

namespace RandomGameLauncher.Services
{
    // TODO: refactor
    public static class LibraryService
    {
        public static void LocateStoreExeFromReg(LibraryEnum library)
        {
            if (LibraryEnum.Steam == library)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
                Settings.Default.SteamPath = (key is not null) ? key.GetValue("SteamExe") as string : string.Empty;
            }
            else if (LibraryEnum.EpicGames == library)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\EOS");
                Settings.Default.EpicGamesPath = (key is not null) ? key.GetValue("ModSdkCommand") as string : string.Empty;
            }
        }

        public record EGSGameEx(string CatalogNamespace, string CatalogItemId, string AppName, string DisplayName, string InstallLocation);
        public static List<Game> GetEGSGames()
        {
            List<Game> egs_list = [];

            using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\EOS");
            
            // TODO: use a default directory if the key is null
            string? registryMetadataDir = (key is not null) ? key.GetValue("ModSdkMetadataDir") as string : string.Empty;
            AbsolutePath manifestFolder = FileSystem.Shared.FromUnsanitizedFullPath(registryMetadataDir);
            AbsolutePath[] itemFiles = FileSystem.Shared.EnumerateFiles(manifestFolder, "*.item").ToArray();

            foreach (AbsolutePath itemFile in itemFiles)
            {
                using Stream stream = FileSystem.Shared.ReadFile(itemFile);
                EGSGameEx game = JsonSerializer.Deserialize<EGSGameEx>(stream);
                if (game is null) continue;

                /*
                 Example of .url shortcut file (Sonic Mania):
                 'com.epicgames.launcher://apps/
                    45e7cf3c49054f2fb20b673d9b0ae69e    // CatalogNamespace
                    %3A                                 // :
                    f08663635fd84c33bfc62ea3bac000e6    // CatalogItemId
                    %3A                                 // :
                    818447bb519b46d48d365d5753362796    // AppName
                    ?action=launch&silent=true'
                 */

                string game_id = game.CatalogNamespace + "%3A" + game.CatalogItemId + "%3A" + game.AppName;
                string path = $"{game.InstallLocation}{Path.DirectorySeparatorChar}{game.DisplayName}.url";
                egs_list.Add(new Game(LibraryEnum.EpicGames, game_id, path));
            }
            return egs_list;
        }

        public static List<Game> GetSteamGames()
        {
            List<Game> mygames = [];
            SteamHandler steam_handler = new(FileSystem.Shared, WindowsRegistry.Shared);
            var steam_games = steam_handler.FindAllGamesById(out _);

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
                mygames.Add(new Game(LibraryEnum.Steam, steam_game.Key.ToString(), path));
            }
            return mygames;
        }

        public static string? SelectExecutableFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = Strings.SEL_EXE_TITLE,
                Filter = "(*.exe)|*.exe",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }

            return null;
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
