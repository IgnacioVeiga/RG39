using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
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

                MessageBoxResult result = MessageBox.Show("¿Cargar la libreria de Steam?", "Steam", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    List<GenericFile> steamGames = MyFunctions.GetSteamLib();
                    foreach (GenericFile game in steamGames)
                    {
                        gamesList.Items.Add(game);
                    }

                    // Obtener ubicación del ejecutable steam.exe
                    steamExe.Content = MyFunctions.LocateSteamExe();
                    // Ejecutar steam.exe con el parametro steam://rungameid/game_id
                    if (string.IsNullOrEmpty((string)steamExe.Content))
                    {
                        MessageBox.Show("No se puede ubicar steam.exe");
                        return;
                    }
                }

                // En btn se activa si hay elementos en la lista
                run_BTN.IsEnabled = gamesList.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Run_Click(object sender, RoutedEventArgs e)
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
                        MessageBox.Show("No se puede ubicar steam.exe");
                        return;
                    }
                    Process.Start($"\"{steamExe.Content}\"", $"steam://rungameid/{game.SteamGameId}");
                    Application.Current.Shutdown();
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
                // En btn se activa si hay elementos en la lista
                run_BTN.IsEnabled = gamesList.Items.Count > 0;
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
                if (File.Exists(@".\list.xml"))
                {
                    File.Delete(@".\list.xml");
                    gamesList.Items.Clear();
                    MessageBox.Show("Ok");
                }
                // En btn se activa si hay elementos en la lista
                run_BTN.IsEnabled = gamesList.Items.Count > 0;
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
                    bool result = MessageBox.Show($"Quitar de la lista a \n{((GenericFile)((Button)sender).DataContext).FileName.ToString()}?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    if (result)
                    {
                        MyFunctions.SaveList(gamesList.Items.As<GenericFile>()
                                                        .Where(i => i.From == FromLibrary.Other && i.FilePath != gameFilePath)
                                                        .ToList());
                    }
                    else return;
                }
                gamesList.Items.RemoveAt(gameIndex);

                // En btn se activa si hay elementos en la lista
                run_BTN.IsEnabled = gamesList.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LocateSteam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string steamFileName = MyFunctions.SelectExecutable();
                string str = steamFileName[(steamFileName.LastIndexOf(@"\") + 1)..];
                if (steamFileName is null || str != "steam.exe")
                {
                    MessageBox.Show("Ejecutable incorrecto");
                    return;
                }
                else
                {
                    List<GenericFile> steamGames = MyFunctions.GetSteamLib();
                    foreach (GenericFile game in steamGames)
                    {
                        gamesList.Items.Add(game);
                    }

                    steamExe.Content = steamFileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
