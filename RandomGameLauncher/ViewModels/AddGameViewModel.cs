using RandomGameLauncher.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace RandomGameLauncher.ViewModels;

public class AddGameViewModel : INotifyPropertyChanged
{
    private readonly IExecutablePicker _executablePicker;
    private readonly RelayCommand _confirmCommand;
    private string _selectedFilePath = string.Empty;
    private string _launchArguments = string.Empty;
    private bool? _dialogResult;

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
            _confirmCommand.RaiseCanExecuteChanged();
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

    /// <summary>
    /// Bound to the dialog through an attached property so the ViewModel can close
    /// the window without code-behind click handlers.
    /// </summary>
    public bool? DialogResult
    {
        get => _dialogResult;
        private set
        {
            if (_dialogResult == value)
            {
                return;
            }

            _dialogResult = value;
            OnPropertyChanged(nameof(DialogResult));
        }
    }

    public ICommand OpenDialogCommand { get; }
    public ICommand ConfirmCommand => _confirmCommand;
    public ICommand CancelCommand { get; }

    public AddGameViewModel(IExecutablePicker executablePicker)
    {
        _executablePicker = executablePicker;
        _confirmCommand = new RelayCommand(Confirm, CanConfirm);
        OpenDialogCommand = new RelayCommand(OpenDialog);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void OpenDialog()
    {
        string? filePath = _executablePicker.SelectExecutableFile();

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            SelectedFilePath = filePath;
        }
    }

    private bool CanConfirm() =>
        !string.IsNullOrWhiteSpace(SelectedFilePath);

    private void Confirm()
    {
        DialogResult = true;
    }

    private void Cancel()
    {
        DialogResult = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
