using RG39.Properties;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Windows.Media;

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

        [JsonIgnore]
        private ImageSource appIcon;
        [JsonIgnore]
        public ImageSource AppIcon
        {
            get
            {
                if (From == FromLibrary.Other)
                {
                    appIcon = Icon.ExtractAssociatedIcon(FilePath).ToImageSource();
                }
                else if (From == FromLibrary.Steam)
                {
                    appIcon = Icon.ExtractAssociatedIcon(Settings.Default.SteamPath).ToImageSource();
                }
                else if (From == FromLibrary.EpicGames)
                {
                    appIcon = Icon.ExtractAssociatedIcon(Settings.Default.EGSPath).ToImageSource();
                }
                return appIcon;
            }
            set => appIcon = value;
        }

        // example:    "C:/Folder/FileName.ext" (NOT for Steam/Epic games)
        // IMPORTANT: for Steam or EGS FilePath == Path
        public string FilePath { get; set; }

        // example:    "C:/Folder"
        [JsonIgnore]
        private string path;
        [JsonIgnore]
        public string Path
        {
            get
            {
                if (From == FromLibrary.Other)
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
                if (From == FromLibrary.Steam || From == FromLibrary.EpicGames)
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
        [JsonIgnore]
        private string fileName;
        [JsonIgnore]
        public string FileName
        {
            get
            {
                if (From == FromLibrary.Other)
                    return fileName = FilePath.Remove(0, Path.Length)[..^4];
                else
                    return fileName;
            }
            set => fileName = value;
        }

        public FromLibrary From { get; set; }
    }

    public enum FromLibrary
    {
        Other = 0,
        Steam = 1,
        EpicGames = 2
    }

    // ToDo: Intentar generar los ComboBoxItems a partir de esta enum
    public enum Languages
    {
        English = 0,
        Español = 1
    }
}