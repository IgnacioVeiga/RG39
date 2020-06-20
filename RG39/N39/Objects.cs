namespace RG39.N39
{
    public class FileN39
    {
        // A primary key
        public int Id { get; set; }

        // indicates if must be filtered
        public bool Active { get; set; }

        // example:    "C:/Folder/FileName.ext"
        public string FilePath { get; set; }

        // example:    "C:/Folder"
        public string Path { get; set; }

        // example: ".ext"
        public string Format => FilePath.Remove(0, FilePath.Length - 4);

        // name of file but without format
        // example:  "FileName", NOT "FileName.ext"
        private string fileName;
        public string FileName
        {
            get
            {
                fileName = FilePath.Remove(0, Path.Length + 1);
                fileName = fileName.Remove(fileName.Length - 4);
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }
    }

    //public class ResultN39
    //{
    //    public ResultN39()
    //    {
    //        Errors = new List<string>();
    //    }

    //    public bool Ok => Errors.Count == 0;

    //    public List<string> Errors { get; private set; }

    //    public void AddError(string msg)
    //    {
    //        Errors.Add(msg);
    //    }

    //    public string Message => Errors != null ? string.Join(". ", Errors.ToArray()) : "";

    //}

}