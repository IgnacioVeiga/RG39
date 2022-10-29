using RG39.Lang;
using RG39.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WinCopies.Linq;
using WinCopies.Util;

namespace RG39
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();

                List<GenericFile> gamesListSaved = MyFunctions.ReadList();
                if (gamesListSaved is not null)
                {
                    foreach (GenericFile item in gamesListSaved)
                    {
                        gamesList.Items.Add(item);
                    }
                }

                // Obtener ubicación del ejecutable steam.exe
                steamExe.Content = MyFunctions.LocateStoreExeFromReg(FromLibrary.Steam);

                // Obtener ubicación del ejecutable epic.exe
                epicGamesExe.Content = MyFunctions.LocateStoreExeFromReg(FromLibrary.EpicGames);

                MessageBoxResult resultSteam = MessageBox.Show(strings.LOAD_STEAM_LIB_MSG, "Steam", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultSteam == MessageBoxResult.Yes)
                {
                    List<GenericFile> steamGames = MyFunctions.GetSteamLib();
                    foreach (GenericFile game in steamGames)
                    {
                        gamesList.Items.Add(game);
                    }
                    if (string.IsNullOrEmpty((string)steamExe.Content))
                    {
                        MessageBox.Show(strings.CANNOT_LOCATE_STEAM);
                        epicGamesExe.Content = "[No encontado]";
                    }
                }

                MessageBoxResult resultEpic = MessageBox.Show(strings.LOAD_EPIC_LIB_MSG, "Epic Games", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultEpic == MessageBoxResult.Yes)
                {
                    List<GenericFile> epicGames = MyFunctions.GetEpicGamesStoreLib();
                    foreach (GenericFile game in epicGames)
                    {
                        gamesList.Items.Add(game);
                    }
                    if (string.IsNullOrEmpty((string)epicGamesExe.Content))
                    {
                        MessageBox.Show(strings.CANNOT_LOCATE_EPIC);
                        epicGamesExe.Content = "[No encontado]";
                    }
                }

                // En btn se activa si hay elementos en la lista
                start_BTN.IsEnabled = gamesList.Items.Count > 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo info = new()
                {
                    UseShellExecute = true
                };
                int max = gamesList.Items.As<GenericFile>().Where(f => f.Active).ToList().Count;
                int num = new Random().Next(max);

                // Busca el archivo con el id aleatorio generado y en estado "activo"
                GenericFile game = gamesList.Items.As<GenericFile>().Where(f => f.Active).ToArray()[num];
                if (game is null)
                {
                    return;
                }
                if (game.From == FromLibrary.Other)
                {
                    info.FileName = game.FileName + game.Type;
                    info.WorkingDirectory = game.Path;
                    MyFunctions.RunGame(info);
                }
                if (game.From == FromLibrary.Steam)
                {
                    if (string.IsNullOrEmpty((string)steamExe.Content))
                    {
                        MessageBox.Show(strings.CANNOT_LOCATE_STEAM);
                        return;
                    }
                    // Ejecutar steam.exe con el parametro steam://rungameid/game_id
                    Process.Start($"\"{steamExe.Content}\"", $"steam://rungameid/{game.SteamGameId}");
                    Application.Current.Shutdown();
                }
                if (game.From == FromLibrary.EpicGames)
                {
                    if (string.IsNullOrEmpty((string)epicGamesExe.Content))
                    {
                        MessageBox.Show(strings.CANNOT_LOCATE_EPIC);
                        return;
                    }
                    // Ejecutar EpicGamesLauncher.exe con el parametro com.epicgames.launcher://apps/{parametro}{game_id}{parametro}?action=launch&silent=true
                    MessageBox.Show($"Por ahora no puedo ejecutar \"{game.FileName}\".");
                    // Process.Start($"{epicGamesExe.Content} com.epicgames.launcher://apps/AAAAAAAAAAAAA{game.EGSGameId}AAAAAAAAAAAAA?action=launch&silent=true");
                    // Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddGameToList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string gameFileName = MyFunctions.SelectExecutable();
                if (gameFileName is null)
                {
                    return;
                }
                GenericFile game = new()
                {
                    FilePath = gameFileName,
                    Active = true,
                    From = FromLibrary.Other
                };
                gamesList.Items.Add(game);
                MyFunctions.SaveList(gamesList.Items.As<GenericFile>()
                                                    .Where(i => i.From == FromLibrary.Other)
                                                    .ToList());

                start_BTN.IsEnabled = gamesList.Items.Count > 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show(strings.CLEAR_LIST_MSG, strings.CLEAR_LIST_TITLE, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (File.Exists(@".\list.xml"))
                        File.Delete(@".\list.xml");
                    gamesList.Items.Clear();
                    MessageBox.Show("Ok");
                }

                start_BTN.IsEnabled = gamesList.Items.Count > 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RemoveItemFromList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string gameFilePath = ((GenericFile)((Button)sender).DataContext).FilePath;
                if (gameFilePath is null) return;
                int gameIndex = (int)gamesList.Items.As<GenericFile>().FindIndexOf(g => g.FilePath == gameFilePath);
                if (gameIndex < 0) return;


                if (((GenericFile)((Button)sender).DataContext).From == FromLibrary.Other)
                {
                    bool result = MessageBox.Show($"Quitar de la lista a \n{((GenericFile)((Button)sender).DataContext).FileName}?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    if (result)
                    {
                        MyFunctions.SaveList(gamesList.Items.As<GenericFile>()
                                                        .Where(i => i.From == FromLibrary.Other && i.FilePath != gameFilePath)
                                                        .ToList());
                    }
                    else return;
                }
                gamesList.Items.RemoveAt(gameIndex);

                start_BTN.IsEnabled = gamesList.Items.Count > 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LocateStoreExe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = MyFunctions.SelectExecutable();
                if (filename is null)
                {
                    return;
                }
                else if (filename.EndsWith("steam.exe"))
                {
                    List<GenericFile> steamGames = MyFunctions.GetSteamLib();
                    foreach (GenericFile game in steamGames)
                    {
                        gamesList.Items.Add(game);
                    }

                    steamExe.Content = filename;
                }
                else if (filename.EndsWith("EpicGamesLauncher.exe"))
                {
                    List<GenericFile> egsGames = MyFunctions.GetEpicGamesStoreLib();
                    foreach (GenericFile game in egsGames)
                    {
                        gamesList.Items.Add(game);
                    }

                    epicGamesExe.Content = filename;
                }
                else
                {
                    MessageBox.Show("Incorrect executable");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ToggleLang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Settings.Default.Lang == "en")
                    Settings.Default.Lang = "es";
                else
                    Settings.Default.Lang = "en";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void showOrHideTabs_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (generalTabControl.Visibility == Visibility.Visible)
                {
                    generalTabControl.Visibility = Visibility.Collapsed;
                    showOrHideTabs_BTN.Content = "˅";
                }
                else if (generalTabControl.Visibility == Visibility.Collapsed)
                {
                    generalTabControl.Visibility = Visibility.Visible;
                    showOrHideTabs_BTN.Content = "˄";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
