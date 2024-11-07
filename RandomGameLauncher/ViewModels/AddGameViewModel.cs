using RandomGameLauncher.Models;
using RandomGameLauncher.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace RandomGameLauncher.ViewModels
{
    public class AddGameViewModel : INotifyPropertyChanged
    {
        public Game NewGame { get; private set; }

        public ICommand OpenDialogCommand { get; }

        public AddGameViewModel()
        {
            NewGame = new Game(LibraryEnum.Other, "", "");
            OpenDialogCommand = new RelayCommand(OpenDialog);
        }

        private void OpenDialog()
        {
            string? filePath = LibraryService.SelectExecutableFile();

            if (!string.IsNullOrEmpty(filePath))
            {
                NewGame.Active = true;
                NewGame.FilePath = filePath;
                OnPropertyChanged(nameof(NewGame));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
