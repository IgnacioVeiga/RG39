using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.EGS;
using GameFinder.StoreHandlers.Steam;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using RG39.Lang;
using RG39.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using WinCopies.Util;

namespace RG39.Util
{
    public static class MyFunctions
    {
        public static void RunGame(GenericFile game)
        {
            try
            {
                if (game is null) return;

                if (game.From == FromLibrary.Other)
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = game.FileName + game.Type,
                        WorkingDirectory = game.Path
                    });

                if (game.From == FromLibrary.Steam)
                    Process.Start($"\"{Settings.Default.SteamPath}\"", $"steam://rungameid/{game.SteamGameId}");

                if (game.From == FromLibrary.EpicGames)
                {
                    /*
                     Ejecutar EpicGamesLauncher.exe con el parametro com.epicgames.launcher://apps/{parametro}{EGSGameId}{parametro}?action=launch&silent=true
                     Ejemplo: com.epicgames.launcher://apps/0bd3e505924240adb702295fa08c1eff%3A283080ad58e64fd084d30413888a571c%3Aa64dcf9b711a4a60a3c0b6f052dfc7da?action=launch&silent=true
                     El EGSGameId es 283080ad58e64fd084d30413888a571c
                     ToDo: encontrar los otros 2 parametros que lo rodean
                     */
                    MessageBox.Show($"{strings.CANNOT_LOAD_GAME_MSG}\n\"{game.FileName}\".");
                    return;
                    // Process.Start($"{Settings.Default.EGSPath} com.epicgames.launcher://apps/AAAAAAAAAAAAA{game.EGSGameId}AAAAAAAAAAAAA?action=launch&silent=true");
                }
                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                string msg = ex.Message + "\n" + JsonSerializer.Serialize(game, new JsonSerializerOptions() { WriteIndented = true });
                MessageBox.Show(msg);
            }
        }

        public static string LocateStoreExeFromReg(FromLibrary from)
        {
            if (FromLibrary.Steam == from)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
                if (key is not null)
                {
                    object steamExeDir = key.GetValue("SteamExe");
                    if (steamExeDir is not null) return steamExeDir as string;
                }
            }
            else if (FromLibrary.EpicGames == from)
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\EOS");
                if (key is not null)
                {
                    object egsExeDir = key.GetValue("ModSdkCommand");
                    if (egsExeDir is not null) return egsExeDir as string;
                }
            }
            return string.Empty;
        }

        public static List<GenericFile> GetGamesFromLib(FromLibrary from)
        {
            List<GenericFile> games = new();

            if (FromLibrary.Steam == from)
            {
                // use the Windows registry on Windows
                // Linux doesn't have a registry
                SteamHandler handler = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new SteamHandler(new WindowsRegistry())
                    : new SteamHandler(null);
                foreach ((SteamGame game, _) in handler.FindAllGames())
                {
                    // ToDo: filter soundtracks
                    if (game is not null && game.AppId != 228980 && !game.Name.Contains("Soundtrack"))
                    {
                        games.Add(new GenericFile()
                        {
                            Active = true,
                            FileName = game.Name,
                            FilePath = game.Path,
                            From = FromLibrary.Steam,
                            SteamGameId = game.AppId
                        });
                    }
                }
            }
            else if (FromLibrary.EpicGames == from)
            {
                EGSHandler handler = new();
                foreach ((EGSGame game, _) in handler.FindAllGames())
                {
                    if (game is not null)
                    {
                        games.Add(new GenericFile()
                        {
                            Active = false,
                            FileName = game.DisplayName,
                            FilePath = game.InstallLocation,
                            From = FromLibrary.EpicGames,
                            EGSGameId = game.CatalogItemId
                        });
                    }
                }
            }
            return games;
        }

        public static string SelectExecutable()
        {
            // Sirve para mostrar el dialogo selector de carpetas
            CommonOpenFileDialog exe = new()
            {
                // ToDo: reemplazar este dialogo por el propio en creación
                Title = strings.SEL_EXE_TITLE,
                Multiselect = false,
                EnsurePathExists = true,
                EnsureFileExists = true,

                // Carpeta de escritorio por defecto
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };

            // Muestro la ventana para seleccionar carpeta y cargamos datos si es ok
            if (exe.ShowDialog() == CommonFileDialogResult.Ok) return exe.FileName;
            else return null;
        }

        public static void SaveList(List<GenericFile> games)
        {
            if (File.Exists(@".\list.xml"))
                File.Delete(@".\list.xml");
            XmlWriter list = XmlWriter.Create("list.xml");
            list.WriteStartElement("MyGames");
            list.WriteElementString("Other", JsonSerializer.Serialize(games));
            list.WriteEndElement();
            list.Close();
        }

        public static List<GenericFile> ReadList()
        {
            List<GenericFile> gamesList = new();
            if (File.Exists(@".\list.xml"))
            {
                XmlReader listXML = XmlReader.Create("list.xml");
                listXML.ReadToFollowing("Other");
                string json = listXML.ReadElementContentAsString();
                gamesList.AddRange(JsonSerializer.Deserialize<List<GenericFile>>(json));
                listXML.Close();
                gamesList = gamesList.Where(g => File.Exists(g.FilePath)).ToList();
            }
            return gamesList;
        }

        /// <summary>
        /// 0 = English
        /// 1 = Español
        /// </summary>
        /// <param name="langIndex"></param>
        public static void ChangeLang(int langIndex)
        {
            switch (langIndex)
            {
                case 0:
                    Settings.Default.LangString = "en";
                    break;
                case 1:
                    Settings.Default.LangString = "es";
                    break;
            }

            // ToDo: buscar otra manera de recordar el item seleccionado del ComboBox "langSelected"
            Settings.Default.LangIndex = langIndex;
            Settings.Default.Save();
        }
    }

    // source: https://stackoverflow.com/questions/1127647/convert-system-drawing-icon-to-system-media-imagesource
    internal static class IconUtilities
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ToImageSource(this Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            if (!DeleteObject(hBitmap)) throw new Win32Exception();

            return wpfBitmap;
        }
    }
}
