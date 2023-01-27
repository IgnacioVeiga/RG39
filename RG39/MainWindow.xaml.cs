using RG39.Lang;
using RG39.Properties;
using RG39.Util;
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
            InitializeComponent();

            List<GenericFile> gamesList = MyFunctions.ReadList();
            // ToDo: mostrar en una ventana aparte los juegos no existentes.
            if (gamesList is not null) this.gamesList.Items.AddRange(gamesList);

            Settings.Default.SteamPath = MyFunctions.LocateStoreExeFromReg(FromLibrary.Steam);
            //Settings.Default.EGSPath = MyFunctions.LocateStoreExeFromReg(FromLibrary.EpicGames);

            if (!string.IsNullOrEmpty(Settings.Default.SteamPath))
            {
                steamIcon.Source = System.Drawing.Icon.ExtractAssociatedIcon(Settings.Default.SteamPath).ToImageSource();
                this.gamesList.Items.AddRange(MyFunctions.GetGamesFromLib(FromLibrary.Steam));
            }
            else Settings.Default.SteamPath = $"Steam: {strings.NOT_FOUND_MSG}";

            //if (!string.IsNullOrEmpty(Settings.Default.EGSPath))
            //{
            //    egsIcon.Source = System.Drawing.Icon.ExtractAssociatedIcon(Settings.Default.EGSPath).ToImageSource();
            //    this.gamesList.Items.AddRange(MyFunctions.GetGamesFromLib(FromLibrary.EpicGames));
            //}
            //else Settings.Default.EGSPath = $"Epic Games Store: {strings.NOT_FOUND_MSG}";

            // En btn se activa si hay elementos en la lista
            start_BTN.IsEnabled = this.gamesList.Items.Count > 1;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<GenericFile> list = gamesList.Items.As<GenericFile>().Where(f => f.Active);
                int index = new Random().Next(list.Count());
                GenericFile game = list.ToArray()[index];

                MyFunctions.RunGame(game);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddGameToList_Click(object sender, RoutedEventArgs e)
        {
            string gameFileName = MyFunctions.SelectExecutable();
            if (gameFileName is null) return;

            if (gamesList.Items.As<GenericFile>().Any(g => g.FilePath == gameFileName))
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

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgResult = MessageBox.Show(strings.CLEAR_LIST_MSG, strings.CLEAR_LIST_TITLE, MessageBoxButton.YesNo);
            if (msgResult == MessageBoxResult.Yes)
            {
                if (File.Exists(@".\list.xml"))
                    File.Delete(@".\list.xml");
                gamesList.Items.Clear();
                MessageBox.Show("Ok");
            }

            start_BTN.IsEnabled = gamesList.Items.Count > 1;
        }

        private void RemoveItemFromList_Click(object sender, RoutedEventArgs e)
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

        private void ToggleLang_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.Lang == "en")
                Settings.Default.Lang = "es";
            else
                Settings.Default.Lang = "en";
        }

        private void ToggleVisibilityGeneral_Click(object sender, RoutedEventArgs e)
        {
            // ToDo: realizar esto de otra forma más simple
            if (general.Visibility == Visibility.Visible)
            {
                general.Visibility = Visibility.Collapsed;
                toggleVisibilityGeneralBTN.Content = "˅";
                theWindow.MinHeight = 150;
                theWindow.MaxHeight = 150;
                theWindow.Height = 150;
            }
            else if (general.Visibility == Visibility.Collapsed)
            {
                general.Visibility = Visibility.Visible;
                toggleVisibilityGeneralBTN.Content = "˄";
                theWindow.MinHeight = 400;
                theWindow.MaxHeight = double.PositiveInfinity;
                theWindow.Height = 400;
            }

        }

        private void StartItemFromList_Click(object sender, RoutedEventArgs e)
        {
            GenericFile game = ((Button)sender).DataContext as GenericFile;
            MyFunctions.RunGame(game);
        }
    }
}
