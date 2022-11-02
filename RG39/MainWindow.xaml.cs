using RG39.Lang;
using RG39.Properties;
using System;
using System.Collections.Generic;
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

                Settings.Default.SteamPath = MyFunctions.LocateStoreExeFromReg(FromLibrary.Steam);
                Settings.Default.EGSPath = MyFunctions.LocateStoreExeFromReg(FromLibrary.EpicGames);

                if (!string.IsNullOrEmpty(Settings.Default.SteamPath))
                {
                    steamIcon.Source = IconUtilities.ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(Settings.Default.SteamPath));
                    foreach (GenericFile game in MyFunctions.GetGamesFromLib(FromLibrary.Steam))
                    {
                        game.AppIcon = steamIcon.Source;
                        gamesList.Items.Add(game);
                    }
                }
                else
                {
                    Settings.Default.SteamPath = $"Steam: {strings.NOT_FOUND_MSG}";
                }

                if (!string.IsNullOrEmpty(Settings.Default.EGSPath))
                {
                    egsIcon.Source = IconUtilities.ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(Settings.Default.EGSPath));
                    foreach (GenericFile game in MyFunctions.GetGamesFromLib(FromLibrary.EpicGames))
                    {
                        game.AppIcon = egsIcon.Source;
                        gamesList.Items.Add(game);
                    }
                }
                else
                {
                    Settings.Default.EGSPath = $"Epic Games Store: {strings.NOT_FOUND_MSG}";
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
                int index = new Random().Next(gamesList.Items.As<GenericFile>()
                                                           .Where(f => f.Active)
                                                           .ToList().Count);

                GenericFile game = gamesList.Items.As<GenericFile>().Where(f => f.Active).ToArray()[index];
                MyFunctions.RunGame(game);
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
                if (gameFileName is null) return;

                if (gamesList.Items.As<GenericFile>().FirstOrDefault(g => g.FilePath == gameFileName) is not null)
                {
                    MessageBox.Show($"\"{gameFileName}\"\n {strings.REPEATED_GAME_MSG}", strings.REPEATED_TITLE);
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
                    MessageBoxResult res = MessageBox.Show($"{strings.REMOVE_GAME_MSG}\n{((GenericFile)((Button)sender).DataContext).FileName}?", "", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
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

        private void ShowOrHideTabs_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (generalTabControl.Visibility == Visibility.Visible)
                {
                    generalTabControl.Visibility = Visibility.Collapsed;
                    showOrHideTabs_BTN.Content = "˅";
                    theWindow.MinHeight = 150;
                    theWindow.MaxHeight = 150;
                    theWindow.Height = 150;
                }
                else if (generalTabControl.Visibility == Visibility.Collapsed)
                {
                    generalTabControl.Visibility = Visibility.Visible;
                    showOrHideTabs_BTN.Content = "˄";
                    theWindow.MinHeight = 400;
                    theWindow.MaxHeight = double.PositiveInfinity;
                    theWindow.Height = 400;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StartItemFromList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GenericFile game = ((Button)sender).DataContext as GenericFile;
                MyFunctions.RunGame(game);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
