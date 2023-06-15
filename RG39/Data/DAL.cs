using RG39.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RG39.Data
{
    /// <summary>
    /// DAL = Data Access Layer
    /// </summary>
    internal static class DAL
    {
        internal static void ClearList_Legacy()
        {
            if (File.Exists($".{Path.DirectorySeparatorChar}list.json"))
                File.Delete($".{Path.DirectorySeparatorChar}list.json");
        }

        internal static void SaveList_Legacy(List<Game> games)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(games, options);
            File.WriteAllText($".{Path.DirectorySeparatorChar}list.json", json);
        }

        internal static List<Game> ReadList_Legacy()
        {
            List<Game> games = new();

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
