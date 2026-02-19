using RandomGameLauncher.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace RandomGameLauncher.ViewModels;

public class AddGameViewModel : INotifyPropertyChanged
{
    private readonly IExecutablePicker _executablePicker;
    private string _selectedFilePath = string.Empty;
    private string _launchArguments = string.Empty;

    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set
        {
            if (string.Equals(_selectedFilePath, value, StringComparison.Ordinal))
            {
                return;
            }

            _selectedFilePath = value;
            OnPropertyChanged(nameof(SelectedFilePath));
        }
    }

    public string LaunchArguments
    {
        get => _launchArguments;
        set
        {
            if (string.Equals(_launchArguments, value, StringComparison.Ordinal))
            {
                return;
            }

            _launchArguments = value ?? string.Empty;
            OnPropertyChanged(nameof(LaunchArguments));
        }
    }

    public ICommand OpenDialogCommand { get; }

    public AddGameViewModel(IExecutablePicker executablePicker)
    {
        _executablePicker = executablePicker;
        OpenDialogCommand = new RelayCommand(OpenDialog);
    }

    private void OpenDialog()
    {
        string? filePath = _executablePicker.SelectExecutableFile();

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            SelectedFilePath = filePath;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
