using Microsoft.WindowsAPICodePack.Dialogs;
using RG39.Lang;
using RG39.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RG39.Util
{
    public static class MyFunctions
    {
        public static void RunGame(GenericFile game)
        {
            try
            {
                if (game is null) return;

                if (game.From == GameStores.FromLibrary.Other)
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = game.FileName + game.Type,
                        WorkingDirectory = game.Path
                    });

                if (game.From == GameStores.FromLibrary.Steam)
                    Process.Start($"\"{Settings.Default.SteamPath}\"", $"steam://rungameid/{game.SteamGameId}");

                #region EpicGamesStore
                if (game.From == GameStores.FromLibrary.EpicGames)
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
                #endregion

                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                string msg = ex.Message + "\n" + JsonSerializer.Serialize(game, new JsonSerializerOptions() { WriteIndented = true });
                MessageBox.Show(msg);
            }
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

        public static void RestartApp()
        {
            try
            {
                Process.Start(Environment.ProcessPath);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                BitmapSizeOptions.FromEmptyOptions()
            );

            if (!DeleteObject(hBitmap)) throw new Win32Exception();

            return wpfBitmap;
        }
    }
}
