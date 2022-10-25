using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using WinCopies.Linq;

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
                MessageBoxResult result = MessageBox.Show("¿Cargar la libreria de Steam?", "Steam", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    List<GenericFile> steamGames = MyFunctions.GetSteamLib();
                    foreach (GenericFile game in steamGames)
                    {
                        game.Id = gamesList.Items.Count + 1;
                        gamesList.Items.Add(game);
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
                    int num = new Random().Next(1, gamesList.Items.Count + 1);

                    // Busca el archivo con el id aleatorio generado y en estado "activo"
                    GenericFile game = gamesList.Items.As<GenericFile>()
                                                         .FirstOrDefault(f => f.Id == num && f.Active);
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
                        // Obtener ubicación del ejecutable steam.exe
                        string steamExe = MyFunctions.LocateSteamExe();
                        // Ejecutar steam.exe con el parametro steam://rungameid/game_id
                        if (string.IsNullOrEmpty(steamExe))
                        {
                            MessageBox.Show("No se puede ubicar steam.exe");
                            return;
                        }
                        Process.Start($"\"{steamExe}\"",$"steam://rungameid/{game.SteamGameId}");
                        Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string gameFileName = MyFunctions.SelectExecutable();
                if (gameFileName is null)
                {
                    return;
                }
                gamesList.Items.Add(new GenericFile()
                {
                    Id = gamesList.Items.Count + 1,
                    FilePath = gameFileName,
                    Active = true,
                    From = FromLibrary.Other
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
