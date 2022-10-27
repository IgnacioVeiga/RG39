using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using GameFinder.StoreHandlers.Steam;
using System.Runtime.InteropServices;
using GameFinder.RegistryUtils;
using Microsoft.Win32;
using System.Xml;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WinCopies.Util;

namespace RG39
{
    public static class MyFunctions
    {
        public static void RunGame(ProcessStartInfo info)
        {
            try
            {
                Process.Start(info);
                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static List<GenericFile> GetSteamLib()
        {
            List<GenericFile> steamLib = new();

            // use the Windows registry on Windows
            // Linux doesn't have a registry
            SteamHandler handler = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new SteamHandler(new WindowsRegistry())
                : new SteamHandler(null);

            // method 1: iterate over the game-error result
            foreach ((SteamGame game, string error) in handler.FindAllGames())
            {
                if (game is not null && game.AppId != 228980)
                {
                    //MessageBox.Show($"Found {game}");

                    // Mapear SteamGame a GenericFile
                    steamLib.Add(new GenericFile()
                    {
                        Active = true,
                        FileName = game.Name,
                        FilePath = game.Path,
                        From = FromLibrary.Steam,
                        SteamGameId = game.AppId
                    });
                }
                //else
                //{
                //    MessageBox.Show($"Error: {error}");
                //}
            }

            // method 2: use the dictionary if you need to find games by id
            //Dictionary<SteamGame, int> games = handler.FindAllGamesById(out string[] errors);

            // method 3: find a single game by id
            //SteamGame? game = handler.FindOneGameById(570940, out string[] errors);

            return steamLib;
        }

        public static string LocateSteamExe()
        {
            try
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
                if (key is not null)
                {
                    object steamExeDir = key.GetValue("SteamExe");
                    if (steamExeDir is not null) return steamExeDir as string;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return string.Empty;
        }

        public static void EpicGamesStoreLib()
        {
            //var handler = new EGSHandler();

            //// method 1: iterate over the game-error result
            //foreach (var (game, error) in handler.FindAllGames())
            //{
            //    if (game is not null)
            //    {
            //        Console.WriteLine($"Found {game}");
            //    }
            //    else
            //    {
            //        Console.WriteLine($"Error: {error}");
            //    }
            //}

            //// method 2: use the dictionary if you need to find games by id
            //Dictionary<EGSGame, string> games = handler.FindAllGamesById(out string[] errors);

            //// method 3: find a single game by id
            //EGSGame? game = handler.FindOneGameById("3257e06c28764231acd93049f3774ed6", out string[] errors);
        }

        public static string SelectExecutable()
        {
            // Sirve para mostrar el dialogo selector de carpetas
            CommonOpenFileDialog exe = new()
            {
                // TODO: reemplazar este dialogo por el propio en creación
                Title = "Select executable",
                Multiselect = false,
                EnsurePathExists = true,

                // Carpeta de escritorio por defecto
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };

            // Muestro la ventana para seleccionar carpeta y cargamos datos si es ok
            if (exe.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return exe.FileName;
            }
            return null;
        }

        public static void SaveList(List<GenericFile> game)
        {
            try
            {
                if (File.Exists(@".\list.xml"))
                    File.Delete(@".\list.xml");
                XmlWriter list = XmlWriter.Create("list.xml");
                list.WriteStartElement("MyGames");
                list.WriteElementString("Other", JsonSerializer.Serialize(game));
                list.WriteEndElement();
                list.Close();
                MessageBox.Show("Ok");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static List<GenericFile> ReadList()
        {
            List<GenericFile> gamesList = new();
            try
            {
                if (File.Exists(@".\list.xml"))
                {
                    XmlReader listXML = XmlReader.Create("list.xml");
                    listXML.ReadToFollowing("Other");
                    string json = listXML.ReadElementContentAsString();
                    List<GenericFile> list = JsonSerializer.Deserialize<List<GenericFile>>(json);
                    foreach (GenericFile item in list)
                    {
                        gamesList.Add(item);
                    }
                    listXML.Close();
                }
                return gamesList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}
