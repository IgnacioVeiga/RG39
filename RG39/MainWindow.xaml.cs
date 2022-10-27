using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
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
                if (gamesList.Items.Count > 0)
                {
                    ProcessStartInfo info = new()
                    {
                        UseShellExecute = true
                    };
                    int num = new Random().Next(1, gamesList.Items.As<GenericFile>()
                                                                  .Where(f => f.Active)
                                                                  .ToList().Count + 1);

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
                        Process.Start($"\"{steamExe.Content}\"",$"steam://rungameid/{game.SteamGameId}");
                        Application.Current.Shutdown();
                    }
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(@".\list.xml"))
                {
                    File.Delete(@".\list.xml");
                    gamesList.Items.Clear();
                    MessageBox.Show("Ok");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
