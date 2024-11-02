using RandomGameLauncher.Models;
using RandomGameLauncher.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
                new(FromLibrary.Steam, "1", @"C:\Games\ExampleGame1.exe"),
                new(FromLibrary.Other, "2", @"C:\Games\ExampleGame2.exe")
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
            if (game != null && File.Exists(game.FilePath))
            {
                System.Diagnostics.Process.Start(game.FilePath);
            }
        }

        private void DeleteGame(Game game)
        {
            if (game != null)
            {
                Games.Remove(game);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
