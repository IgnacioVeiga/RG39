using RandomGameLauncher.Models;
using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Services;
using RandomGameLauncher.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RandomGameLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollectionEx<Game> Games { get; set; }
        public string GamesCount { get => Games.Count + " " + Strings.GAMES.ToLower(); }
        public BitmapImage EpicGamesIcon => Utils.ByteArrayToImage(Properties.Resources.EpicGames);
        public BitmapImage SteamIcon => Utils.ByteArrayToImage(Properties.Resources.Steam);

        private bool _isAllActiveChecked;
        public bool IsAllActiveChecked
        {
            get => _isAllActiveChecked;
            set
            {
                if (_isAllActiveChecked != value)
                {
                    _isAllActiveChecked = value;
                    ToggleActive(value);
                    OnPropertyChanged(nameof(IsAllActiveChecked));
                }
            }
        }

        public ICommand PlayRandomGameCommand { get; }
        public ICommand AddGameCommand { get; }
        public ICommand RunGameCommand { get; }
        public ICommand RemoveGameCommand { get; }
        public ICommand ClearListCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand AboutCommand { get; }

        public MainViewModel()
        {
            Games = [];
            Games.AddRange(LibraryService.ReadList());

            #region Steam
            LibraryService.LocateStoreExeFromReg(LibraryEnum.Steam);

            if (!string.IsNullOrEmpty(Settings.Default.SteamPath))
            {
                Games.AddRange(LibraryService.GetSteamGames());
            }
            else Settings.Default.SteamPath = $"Steam: {Strings.NOT_FOUND_MSG}";
            #endregion Steam

            #region EpicGames
            LibraryService.LocateStoreExeFromReg(LibraryEnum.EpicGames);

            if (!string.IsNullOrEmpty(Settings.Default.EpicGamesPath))
            {
                Games.AddRange(LibraryService.GetEGSGames());
            }
            else Settings.Default.EpicGamesPath = $"Epic Games Store: {Strings.NOT_FOUND_MSG}";
            #endregion EpicsGames

            PlayRandomGameCommand = new RelayCommand(PlayRandomGame);
            AddGameCommand = new RelayCommand(AddGame);
            RunGameCommand = new RelayCommand<Game>(RunGame);
            RemoveGameCommand = new RelayCommand<Game>(RemoveGame);
            ClearListCommand = new RelayCommand(ClearList);
            HelpCommand = new RelayCommand(HowToUse);
            AboutCommand = new RelayCommand(About);
        }

        private void PlayRandomGame()
        {
            if (Games.Count > 0)
            {
                // FIXME: The checkboxes for each game do not work correctly, they are not updated in the list when checked individually.
                // TODO: Use only active games
                int index = new Random().Next(Games.Count());
                Game random_game = Games[index];
                RunGame(random_game);
            }
        }

        private void AddGame()
        {
            AddGameWindow addGameWindow = new();
            bool? dialog_result = addGameWindow.ShowDialog();
            if (dialog_result != true) return;

            Game new_game = addGameWindow.NewGame;
            if (new_game == null) return;
            if (Games.Contains(new_game))
                MessageBox.Show($"\"{new_game.FilePath}\"\n {Strings.REPEATED_GAME_MSG}", Strings.REPEATED_TITLE);

            Games.Add(new_game);
            LibraryService.SaveList(Games.ToList());
        }

        private void RunGame(Game game)
        {
            if (game == null) return;

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
                        Process.Start($"\"{Settings.Default.EpicGamesPath}\"", $"com.epicgames.launcher://apps/{game.GameId}?action=launch&silent=true");
                        break;
                }

                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n-> '{game.Name}' <-");
            }
        }

        private void RemoveGame(Game game)
        {

            if (game == null) return;

            if (game.From == LibraryEnum.Other)
            {
                string msg = $"{Strings.REMOVE_GAME_MSG}\n{game.Name}?";
                MessageBoxResult result = MessageBox.Show(msg, "", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Games.Remove(game);
                    LibraryService.SaveList(Games.Where(i => i.From == LibraryEnum.Other).ToList());
                }
                else return;
            }
        }

        private void ClearList()
        {
            MessageBoxResult msgResult = MessageBox.Show(Strings.CLEAR_LIST_MSG, Strings.CLEAR_LIST, MessageBoxButton.YesNo);
            if (msgResult == MessageBoxResult.Yes)
            {
                LibraryService.ClearList();
                Games.Clear();
                // TODO: remove only with LibraryEnum = Other
            }
        }

        private void ToggleActive(bool isChecked)
        {
            ObservableCollectionEx<Game> templist = [.. Games];
            foreach (Game game in templist)
            {
                game.Active = isChecked;
            }
            Games.Clear();
            Games.AddRange(templist);
        }

        private void About()
        {
            new AboutWindow().ShowDialog();
        }

        private void HowToUse()
        {
            string url = "https://github.com/IgnacioVeiga/RG39/blob/master/README";
            switch (Settings.Default.Language)
            {
                case "en":
                    url += ".md#how-to-use";
                    break;
                case "es":
                    url += "_es.md#como-usar";
                    break;
                default:
                    return;
            }
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = url
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
