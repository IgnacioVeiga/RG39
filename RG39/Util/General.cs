using Microsoft.WindowsAPICodePack.Dialogs;
using RG39.Lang;
using RG39.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace RG39.Util
{
    internal static class General
    {
        internal static void RunGame(Game game)
        {
            try
            {
                if (game is null) return;

                if (game.From == GameStores.FromLibrary.Other)
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = game.Name + game.Type,
                        WorkingDirectory = game.Folder
                    });

                if (game.From == GameStores.FromLibrary.Steam)
                    Process.Start($"\"{Settings.Default.SteamPath}\"", $"steam://rungameid/{game.GameId}");

                #region EpicGamesStore
                if (game.From == GameStores.FromLibrary.EpicGames)
                {
                    /*
                     Ejecutar EpicGamesLauncher.exe con el parametro com.epicgames.launcher://apps/{parametro}{EGSGameId}{parametro}?action=launch&silent=true
                     Ejemplo: com.epicgames.launcher://apps/0bd3e505924240adb702295fa08c1eff%3A283080ad58e64fd084d30413888a571c%3Aa64dcf9b711a4a60a3c0b6f052dfc7da?action=launch&silent=true
                     El EGSGameId es 283080ad58e64fd084d30413888a571c
                     ToDo: encontrar los otros 2 parametros que lo rodean
                     */
                    MessageBox.Show($"{strings.CANNOT_LOAD_GAME_MSG}\n\"{game.Name}\".");
                    return;
                    // Process.Start($"{Settings.Default.EGSPath} com.epicgames.launcher://apps/AAAAAAAAAAAAA{game.gameId}AAAAAAAAAAAAA?action=launch&silent=true");
                }
                #endregion

                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                string msg = ex.Message + "\n-> " + game.Name + "´<-";
                MessageBox.Show(msg);
            }
        }

        internal static string SelectExecutable()
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
    }
}
