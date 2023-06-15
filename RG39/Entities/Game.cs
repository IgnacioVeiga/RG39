using RG39.Properties;
using RG39.Util;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace RG39.Entities
{
    public class Game
    {
        /// <summary>
        /// Game constructor
        /// </summary>
        /// <param name="from">From library ...</param>
        /// <param name="gameId">String ID used by the library. For manually added games it can be empty.</param>
        /// <param name="path">A full path such as 'C:\Mydir\GameTitle.exe'. This will be a fake path in case the game is coming from Steam or Epic Games.</param>
        public Game(GameStores.FromLibrary from, string gameId, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Active = true;
                From = from;
                GameId = gameId;
                FilePath = path;
            }
        }

        [JsonConstructor] public Game() { }

        [JsonPropertyName("Active")]
        public bool Active { get; set; }

        [JsonIgnore]
        public string GameId { get; set; }

        [JsonPropertyName("From")]
        public GameStores.FromLibrary From { get; set; }

        [JsonIgnore]
        public string Folder { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public string Type { get; set; }

        [JsonPropertyName("FilePath")]
        public string FilePath
        {
            get => Folder + Name + Type;
            set
            {
                // GetDirectoryName('C:\MyDir\MySubDir\myfile.ext') returns 'C:\MyDir\MySubDir'
                Folder = Path.GetDirectoryName(value) + Path.DirectorySeparatorChar;

                // GetFileNameWithoutExtension('C:\mydir\myfile.ext') returns 'myfile'
                Name = Path.GetFileNameWithoutExtension(value);

                // GetExtension('C:\mydir.old\myfile.ext') returns '.ext'
                Type = Path.GetExtension(value);
            }
        }

        [JsonIgnore]
        public ImageSource AppIcon => From switch
        {
            GameStores.FromLibrary.Other => Icon.ExtractAssociatedIcon(FilePath).ToImageSource(),

            // TODO: Try to get the original game icons even if they are from one of these libraries
            GameStores.FromLibrary.Steam => Resources.Steam.ToImageSource(),
            GameStores.FromLibrary.EpicGames => Resources.EpicGames.ToImageSource(),
            _ => null,
        };
    }
}