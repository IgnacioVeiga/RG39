using RG39.Properties;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.Windows.Media;
using WinCopies.Linq;

namespace RG39.Util
{
    // ToDo:
    // Revisar que atributos son fundamentales para el archivo json.
    // Debería "auto-construirse" solo con el path completo y el campo de activo.
    // Crear otra clase diferente para Steam y EGS de ser necesario
    public class GenericFile
    {
        [JsonIgnore]
        public int SteamGameId { get; set; }
        [JsonIgnore]
        public string EGSGameId { get; set; }

        // indicates if must be filtered
        public bool Active { get; set; }

        private ImageSource appIcon;
        [JsonIgnore]
        public ImageSource AppIcon
        {
            get
            {
                if (From == GameStores.FromLibrary.Other)
                {
                    appIcon = Icon.ExtractAssociatedIcon(FilePath).ToImageSource();
                }
                else if (From == GameStores.FromLibrary.Steam)
                {
                    appIcon = Icon.ExtractAssociatedIcon(Settings.Default.SteamPath).ToImageSource();
                }
                else if (From == GameStores.FromLibrary.EpicGames)
                {
                    appIcon = Icon.ExtractAssociatedIcon(Settings.Default.EGSPath).ToImageSource();
                }
                return appIcon;
            }
            set => appIcon = value;
        }

        // example:    "C:/Folder/FileName.ext" (NOT for Steam/Epic games)
        // IMPORTANT: for Steam or EGS FilePath == Path
        // ToDo: change this
        public string FilePath { get; set; }

        // example:    "C:/Folder"
        private string path;
        [JsonIgnore]
        public string Path
        {
            get
            {
                if (From == GameStores.FromLibrary.Other)
                {
                    path = FilePath[..(FilePath.LastIndexOf(@"\") + 1)];
                    return path;
                }
                return FilePath;
            }
            set => path = value;
        }

        // example: ".ext"
        [JsonIgnore]
        public string Type
        {
            get
            {
                if (From == GameStores.FromLibrary.Steam || From == GameStores.FromLibrary.EpicGames)
                {
                    return ".url";
                }
                else
                {
                    return FilePath.Remove(0, FilePath.Length - 4);
                }
            }
        }

        // file name without extension
        // example:  "FileName", NOT "FileName.ext"
        private string fileName;
        [JsonIgnore]
        public string FileName
        {
            get
            {
                if (From == GameStores.FromLibrary.Other)
                    return fileName = FilePath.Remove(0, Path.Length)[..^4];
                else
                    return fileName;
            }
            set => fileName = value;
        }

        public GameStores.FromLibrary From { get; set; }
    }

    // ToDo: reemplazar la clase de arriba por esta
    public class Game
    {
        public Game(GameStores.FromLibrary from, string gameId, string path)
        {
            Active = true;
            From = from;
            GameId = gameId;

            Folder = path[..(path.LastIndexOf(@"\") + 1)];
            Name = path.Remove(0, Folder.Length)[..^4];
            Type = path.Remove(0, path.Length - 4);

            switch (From)
            {
                case GameStores.FromLibrary.Other:
                    AppIcon = Icon.ExtractAssociatedIcon(path).ToImageSource();
                    break;
                case GameStores.FromLibrary.Steam:
                    AppIcon = Icon.ExtractAssociatedIcon(Settings.Default.SteamPath).ToImageSource();
                    break;
                case GameStores.FromLibrary.EpicGames:
                    AppIcon = Icon.ExtractAssociatedIcon(Settings.Default.EGSPath).ToImageSource();
                    break;
            }
        }

        public bool Active { get; set; }
        public readonly string GameId;
        public readonly GameStores.FromLibrary From;

        public readonly string Folder;
        public readonly string Name;
        public readonly string Type;

        [JsonIgnore]
        public readonly ImageSource AppIcon;
    }
}