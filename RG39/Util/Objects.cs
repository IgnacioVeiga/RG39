using System.Drawing;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace RG39
{
    public class GenericFile
    {
        public int SteamGameId { get; set; }
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
                    appIcon = IconUtilities.ToImageSource(Icon.ExtractAssociatedIcon(FilePath));
                    return appIcon;
                }
                //else if (From == FromLibrary.Steam)
                //{
                //    return AppIcon;
                //}
                else return null;
            }
            set => appIcon = value;
        }

        // example:    "C:/Folder/FileName.ext" (NOT for Steam/Epic games)
        // IMPORTANT: for Steam or EGS FilePath == Path
        public string FilePath { get; set; }

        // example:    "C:/Folder"
        private string path;
        public string Path
        {
            get
            {
                if (From != FromLibrary.Other)
                {
                    return FilePath;
                }
                path = FilePath[..(FilePath.LastIndexOf(@"\") + 1)];
                return path;
            }
            set => path = value;
        }

        // example: ".ext"
        public string Type
        {
            get
            {
                if (From == FromLibrary.Steam)
                {
                    return ".url";
                }
                else if (From == FromLibrary.EpicGames)
                {
                    return ".url";
                }
                else
                {
                    return FilePath.Remove(0, FilePath.Length - 4);
                }
            }
        }

        // name of file but without format
        // example:  "FileName", NOT "FileName.ext"
        private string fileName;
        public string FileName
        {
            get
            {
                if (From != FromLibrary.Other)
                {
                    return fileName;
                }
                return fileName = FilePath.Remove(0, Path.Length)[..^4];
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
}