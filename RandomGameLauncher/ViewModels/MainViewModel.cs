using RandomGameLauncher.Models;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace RandomGameLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Game> Games { get; set; }

        public ICommand PlayRandomGameCommand { get; }
        public ICommand AddGameCommand { get; }
        public ICommand RunGameCommand { get; }
        public ICommand DeleteGameCommand { get; }

        public MainViewModel()
        {
            Games = [
                new(LibraryEnum.Steam, "1", @"C:\Games\ExampleGame1.exe"),
                new(LibraryEnum.Other, "2", @"C:\Games\ExampleGame2.exe")
            ];

            PlayRandomGameCommand = new RelayCommand(PlayRandomGame);
            AddGameCommand = new RelayCommand(AddGame);
            RunGameCommand = new RelayCommand<Game>(RunGame);
            DeleteGameCommand = new RelayCommand<Game>(DeleteGame);
        }

        private void PlayRandomGame()
        {
            if (Games.Count > 0)
            {
                var random = new Random();
                int index = random.Next(Games.Count);
                var randomGame = Games[index];
                RunGame(randomGame);
            }
        }

        private void AddGame()
        {
            var addGameWindow = new AddGameWindow();
            if (addGameWindow.ShowDialog() == true)
            {
                var newGame = addGameWindow.NewGame;
                Games.Add(newGame);
            }
        }
        private void RunGame(Game game)
        {
            if (game == null || !File.Exists(game.FilePath)) return;

            try
            {
                switch (game.From)
                {
                    case LibraryEnum.Other:
                        Process.Start(new ProcessStartInfo()
                        {
                            UseShellExecute = true,
                            FileName = game.Name + game.Type,
                            WorkingDirectory = game.Folder
                        });
                        break;

                    case LibraryEnum.Steam:
                        Process.Start($"\"{Settings.Default.SteamPath}\"", $"steam://rungameid/{game.GameId}");
                        break;

                    case LibraryEnum.EpicGames:
                        //Process.Start($"{Settings.Default.EGSPath} com.epicgames.launcher://apps/{variable}{game.GameId}{variable}?action=launch&silent=true");
                        break;
                }

                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n-> '{game.Name}' <-");
            }
        }

        private void DeleteGame(Game game)
        {
            if (game != null) Games.Remove(game);
        }

        private void DeleteGames(Game game)
        {
            if (game == null) return;

            MessageBoxResult msgResult = MessageBox.Show(Strings.CLEAR_LIST_MSG, Strings.CLEAR_LIST, MessageBoxButton.YesNo);
            if (msgResult == MessageBoxResult.Yes)
            {
                //DAL.ClearList_Legacy();
                //gamesList.Items.Clear();
                //start_BTN.IsEnabled = false;
                //GamesCount.Content = $"{gamesList.Items.Count} {Strings.GAMES}";
                Games.Remove(game);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
