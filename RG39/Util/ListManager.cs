using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace RG39.Util
{
    internal static class ListManager
    {
        internal static void ClearList()
        {
            if (File.Exists(@".\list.xml"))
                File.Delete(@".\list.xml");

            if (File.Exists(@".\list.json"))
                File.Delete(@".\list.json");
        }

        internal static void SaveList(List<GenericFile> games)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(games, options);
            File.WriteAllText(@".\list.json", json);
        }

        internal static List<GenericFile> ReadList()
        {
            List<GenericFile> games = new();

            if (File.Exists(@".\list.json"))
            {
                string json = File.ReadAllText(@".\list.json");
                JsonSerializerOptions options = new() { WriteIndented = true };
                games.AddRange(JsonSerializer.Deserialize<List<GenericFile>>(json, options));
            }

            if (File.Exists(@".\list.xml"))
            {
                games.AddRange(ReadListLegacy(games));
                File.Delete(@".\list.xml");
                SaveList(games.Where(g => File.Exists(g.FilePath)).ToList());
            }

            return games.Where(g => File.Exists(g.FilePath)).ToList();
        }

        #region Legacy
        internal static List<GenericFile> ReadListLegacy(List<GenericFile> games)
        {
            XmlReader listXML = XmlReader.Create("list.xml");
            listXML.ReadToFollowing("Other");
            string json = listXML.ReadElementContentAsString();
            games.AddRange(JsonSerializer.Deserialize<List<GenericFile>>(json));
            listXML.Close();
            return games;
        }
        #endregion
    }
}
