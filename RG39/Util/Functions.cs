using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using GameFinder.StoreHandlers.Steam;
using GameFinder.StoreHandlers.EGS;
using System.Runtime.InteropServices;
using GameFinder.RegistryUtils;
using Microsoft.Win32;
using System.Xml;
using System.IO;
using System.Text.Json;
using RG39.Lang;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;

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

        public static string LocateStoreExeFromReg(FromLibrary from)
        {
            try
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

                foreach ((SteamGame game, string error) in handler.FindAllGames())
                {
                    if (game is not null && game.AppId != 228980)
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
                foreach ((EGSGame game, string error) in handler.FindAllGames())
                {
                    if (game is not null)
                    {
                        games.Add(new GenericFile()
                        {
                            Active = true,
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
                // TODO: reemplazar este dialogo por el propio en creación
                Title = strings.SEL_EXE_TITLE,
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

        public static void SaveList(List<GenericFile> games)
        {
            try
            {
                if (File.Exists(@".\list.xml"))
                    File.Delete(@".\list.xml");
                XmlWriter list = XmlWriter.Create("list.xml");
                list.WriteStartElement("MyGames");
                list.WriteElementString("Other", JsonSerializer.Serialize(games));
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
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }
    }
}
