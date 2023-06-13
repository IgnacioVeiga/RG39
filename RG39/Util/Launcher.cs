using RG39.Entities;
using RG39.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace RG39.Util
{
    internal static class Launcher
    {
        internal static void RunGame(Game game)
        {
            try
            {
                if (game is null) return;

                switch (game.From)
                {
                    case GameStores.FromLibrary.Other:
                        Process.Start(new ProcessStartInfo()
                        {
                            UseShellExecute = true,
                            FileName = game.Name + game.Type,
                            WorkingDirectory = game.Folder
                        });
                        break;

                    case GameStores.FromLibrary.Steam:
                        Process.Start($"\"{Settings.Default.SteamPath}\"", $"steam://rungameid/{game.GameId}");
                        break;

                    #region EpicGamesStore
                    case GameStores.FromLibrary.EpicGames:
                        /*
                         Try to run 'EpicGamesLauncher.exe' together with the following parameter:
                            com.epicgames.launcher://apps/{variable}{GameId}{variable}?action=launch&silent=true

                         Example:
                         'com.epicgames.launcher://apps/
                            0bd3e505924240adb702295fa08c1eff
                            %3A
                            283080ad58e64fd084d30413888a571c
                            %3A
                            a64dcf9b711a4a60a3c0b6f052dfc7da
                            ?action=launch&silent=true'

                         Where in this case the variable 'GameId' is equal to '283080ad58e64fd084d30413888a571c'

                         ToDo: Find those others 2 variables that surround the 'GameId'
                         */

                        //Process.Start($"{Settings.Default.EGSPath} com.epicgames.launcher://apps/{variable}{game.GameId}{variable}?action=launch&silent=true");
                        break;
                    #endregion EpicGamesStore
                }

                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n-> '{game.Name}' <-");
            }
        }
    }
}
