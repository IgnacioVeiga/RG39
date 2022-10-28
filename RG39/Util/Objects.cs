namespace RG39
{
    public class GenericFile
    {
        public int SteamGameId { get; set; }

        // indicates if must be filtered
        public bool Active { get; set; }

        // example:    "C:/Folder/FileName.ext" (NOT for Steam games)
        public string FilePath { get; set; }

        // example:    "C:/Folder"
        private string path;
        public string Path
        {
            get
            {
                if (From == FromLibrary.Steam)
                {
                    return FilePath;
                }
                return path = FilePath[..(FilePath.LastIndexOf(@"\") + 1)];
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
                if (From == FromLibrary.Steam)
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