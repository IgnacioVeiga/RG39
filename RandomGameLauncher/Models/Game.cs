using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace RandomGameLauncher.Models;

public class Game : INotifyPropertyChanged
{
    private bool _active;
    private string _gameId = string.Empty;
    private LibraryEnum _from;
    private string _launchArguments = string.Empty;
    private string _folder = string.Empty;
    private string _name = string.Empty;
    private string _type = string.Empty;

    public Game(LibraryEnum from, string gameId, string path)
    {
        From = from;
        GameId = gameId;

        if (!string.IsNullOrWhiteSpace(path))
        {
            FilePath = path;
            Active = true;
        }
    }

    [JsonConstructor]
    public Game()
    {
    }

    [JsonPropertyName("Active")]
    public bool Active
    {
        get => _active;
        set
        {
            if (_active == value)
            {
                return;
            }

            _active = value;
            OnPropertyChanged(nameof(Active));
        }
    }

    [JsonIgnore]
    public string GameId
    {
        get => _gameId;
        set
        {
            string normalizedValue = value ?? string.Empty;

            if (string.Equals(_gameId, normalizedValue, StringComparison.Ordinal))
            {
                return;
            }

            _gameId = normalizedValue;
            OnPropertyChanged(nameof(GameId));
        }
    }

    [JsonPropertyName("From")]
    public LibraryEnum From
    {
        get => _from;
        set
        {
            if (_from == value)
            {
                return;
            }

            _from = value;
            OnPropertyChanged(nameof(From));
            OnPropertyChanged(nameof(AppIcon));
        }
    }

    [JsonIgnore]
    public string LaunchArguments
    {
        get => _launchArguments;
        set
        {
            string normalizedValue = value ?? string.Empty;

            if (string.Equals(_launchArguments, normalizedValue, StringComparison.Ordinal))
            {
                return;
            }

            _launchArguments = normalizedValue;
            OnPropertyChanged(nameof(LaunchArguments));
        }
    }

    [JsonIgnore]
    public string Folder
    {
        get => _folder;
        set
        {
            string normalizedValue = value ?? string.Empty;

            if (string.Equals(_folder, normalizedValue, StringComparison.Ordinal))
            {
                return;
            }

            _folder = normalizedValue;
            OnPropertyChanged(nameof(Folder));
            OnPropertyChanged(nameof(FilePath));
        }
    }

    [JsonIgnore]
    public string Name
    {
        get => _name;
        set
        {
            string normalizedValue = value ?? string.Empty;

            if (string.Equals(_name, normalizedValue, StringComparison.Ordinal))
            {
                return;
            }

            _name = normalizedValue;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(FilePath));
        }
    }

    [JsonIgnore]
    public string Type
    {
        get => _type;
        set
        {
            string normalizedValue = value ?? string.Empty;

            if (string.Equals(_type, normalizedValue, StringComparison.Ordinal))
            {
                return;
            }

            _type = normalizedValue;
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(FilePath));
        }
    }

    [JsonPropertyName("FilePath")]
    public string FilePath
    {
        get
        {
            if (string.IsNullOrEmpty(Folder) && string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Type))
            {
                return string.Empty;
            }

            return string.Concat(Folder, Name, Type);
        }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _folder = string.Empty;
                _name = string.Empty;
                _type = string.Empty;
                OnPropertyChanged(nameof(Folder));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(FilePath));
                OnPropertyChanged(nameof(AppIcon));
                return;
            }

            string? directory = Path.GetDirectoryName(value);
            _folder = string.IsNullOrEmpty(directory)
                ? string.Empty
                : directory + Path.DirectorySeparatorChar;

            _name = Path.GetFileNameWithoutExtension(value);
            _type = Path.GetExtension(value);

            OnPropertyChanged(nameof(Folder));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(FilePath));
            OnPropertyChanged(nameof(AppIcon));
        }
    }

    private static readonly Dictionary<string, BitmapImage> IconCache = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    public BitmapImage? AppIcon => From switch
    {
        LibraryEnum.Other => GetIconWithCache(FilePath),
        LibraryEnum.Steam => Utils.ByteArrayToImage(Properties.Resources.Steam),
        LibraryEnum.EpicGames => Utils.ByteArrayToImage(Properties.Resources.EpicGames),
        _ => null,
    };

    private static BitmapImage? GetIconWithCache(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        if (IconCache.TryGetValue(filePath, out BitmapImage? cachedImage))
        {
            return cachedImage;
        }

        BitmapImage? icon = Utils.ExtractIconFromExe(filePath);

        if (icon is not null)
        {
            IconCache[filePath] = icon;
        }

        return icon;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
