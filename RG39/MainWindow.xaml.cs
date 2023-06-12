using RG39.Lang;
using RG39.Properties;
using RG39.Util;
using System;
using System.Collections.Generic;
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

            #region Languages
            foreach (var language in AppLanguage.Languages)
            {
                bool isLangSelected = Settings.Default.Lang == language.Key;
                MenuItem menuItem = new()
                {
                    Tag = language.Key,
                    Header = language.Value,
                    IsCheckable = true,
                    IsChecked = isLangSelected,
                    IsEnabled = !isLangSelected
                };
                menuItem.Click += LanguageSelected_Click;
                LanguagesMenu.Items.Add(menuItem);
            }
            #endregion Languages

            gamesList.Items.AddRange(ListManager.ReadList());
            #region Steam
            GameStores.LocateStoreExeFromReg(GameStores.FromLibrary.Steam);

            if (!string.IsNullOrEmpty(Settings.Default.SteamPath))
            {
                steamIcon.Source = System.Drawing.Icon.ExtractAssociatedIcon(Settings.Default.SteamPath).ToImageSource();
                gamesList.Items.AddRange(GameStores.GetGamesFromLib(GameStores.FromLibrary.Steam));
            }
            else Settings.Default.SteamPath = $"Steam: {Strings.NOT_FOUND_MSG}";
            #endregion
            #region EpicGamesStore
            GameStores.LocateStoreExeFromReg(GameStores.FromLibrary.EpicGames);

            if (!string.IsNullOrEmpty(Settings.Default.EGSPath))
            {
                //egsIcon.Source = System.Drawing.Icon.ExtractAssociatedIcon(Settings.Default.EGSPath).ToImageSource();
                //this.gamesList.Items.AddRange(GameStores.GetGamesFromLib(GameStores.FromLibrary.EpicGames));
            }
            else Settings.Default.EGSPath = $"Epic Games Store: {Strings.NOT_FOUND_MSG}";
            #endregion

            // There has to be more than 1 game in the list for the program to make sense
            start_BTN.IsEnabled = gamesList.Items.Count > 1;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<Game> list = gamesList.Items.As<Game>().Where(f => f.Active);
                int index = new Random().Next(list.Count());
                Game game = list.ToArray()[index];

                General.RunGame(game);
            }
            catch (Exception ex)
            {
                // For these cases it is necessary to show a friendlier message to the end user.
                // ToDo: Hide the original message and make it visible with a "See more" button or similar.
                MessageBox.Show(ex.Message);
            }
        }

        private void AddGameToList_Click(object sender, RoutedEventArgs e)
        {
            string gamePath = General.SelectExecutable();
            if (gamePath is null) return;

            if (gamesList.Items.As<Game>().Any(g => g.FilePath == gamePath))
            {
                MessageBox.Show($"\"{gamePath}\"\n {Strings.REPEATED_GAME_MSG}", Strings.REPEATED_TITLE);
                return;
            }

            Game game = new(GameStores.FromLibrary.Other, string.Empty, gamePath);
            gamesList.Items.Add(game);

            ListManager.SaveList(gamesList.Items.As<Game>()
                                                .Where(i => i.From == GameStores.FromLibrary.Other)
                                                .ToList());

            start_BTN.IsEnabled = gamesList.Items.Count > 1;
        }

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgResult = MessageBox.Show(Strings.CLEAR_LIST_MSG, Strings.CLEAR_LIST, MessageBoxButton.YesNo);
            if (msgResult == MessageBoxResult.Yes)
            {
                ListManager.ClearList();
                gamesList.Items.Clear();
                start_BTN.IsEnabled = false;
            }
        }

        private void RemoveItemFromList_Click(object sender, RoutedEventArgs e)
        {
            // ToDo: Change everything below to something better
            string gamePath = ((Game)((Button)sender).DataContext).FilePath;
            if (gamePath is null) return;

            int gameIndex = (int)gamesList.Items.As<Game>().FindIndexOf(g => g.FilePath == gamePath);
            if (gameIndex < 0) return;

            if (((Game)((Button)sender).DataContext).From == GameStores.FromLibrary.Other)
            {
                string msg = $"{Strings.REMOVE_GAME_MSG}\n{((Game)((Button)sender).DataContext).Name}?";
                MessageBoxResult result = MessageBox.Show(msg, "", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    List<Game> list = gamesList.Items.As<Game>().ToList();
                    list.RemoveAt(gameIndex);
                    ListManager.SaveList(list.Where(i => i.From == GameStores.FromLibrary.Other).ToList());
                }
                else return;
            }

            gamesList.Items.RemoveAt(gameIndex);
            start_BTN.IsEnabled = gamesList.Items.Count > 1;
        }

        private void ToggleVisibilityGeneral_Click(object sender, RoutedEventArgs e)
        {
            //// ToDo: Do this in another more universal way and without those "magic numbers"
            //if (general.Visibility == Visibility.Visible)
            //{
            //    general.Visibility = Visibility.Collapsed;
            //    toggleVisibilityGeneralBTN.Content = "˅";
            //    theWindow.MinHeight = 150;
            //    theWindow.MaxHeight = 150;
            //    theWindow.Height = 150;
            //}
            //else if (general.Visibility == Visibility.Collapsed)
            //{
            //    general.Visibility = Visibility.Visible;
            //    toggleVisibilityGeneralBTN.Content = "˄";
            //    theWindow.MinHeight = 400;
            //    theWindow.MaxHeight = double.PositiveInfinity;
            //    theWindow.Height = 400;
            //}
        }

        private void StartItemFromList_Click(object sender, RoutedEventArgs e)
        {
            Game game = ((Button)sender).DataContext as Game;
            General.RunGame(game);
        }

        private void LanguageSelected_Click(object sender, RoutedEventArgs e)
        {
            string language = (sender as MenuItem)?.Tag.ToString();
            AppLanguage.ChangeLanguage(language);
            MessageBox.Show(Strings.TOGGLE_LANG_MSG, "Reiniciando", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            App.RestartApp();
        }
    }
}
