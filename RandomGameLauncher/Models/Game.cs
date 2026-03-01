using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace RandomGameLauncher.Models;

/// <summary>
/// UI-facing game model. It stores path parts separately to support DataGrid columns
/// while still exposing a composed file path for persistence and comparisons.
/// </summary>
public class Game : ObservableObject
{
    private bool _active;
    private string _gameId = string.Empty;
    private LibraryEnum _from;
    private string _launchArguments = string.Empty;
    private string _folder = string.Empty;
    private string _name = string.Empty;
    private string _type = string.Empty;
    private static readonly BitmapImage SteamStoreIcon = Utils.ByteArrayToImage(Properties.Resources.Steam);
    private static readonly BitmapImage EpicStoreIcon = Utils.ByteArrayToImage(Properties.Resources.EpicGames);

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
        set => SetProperty(ref _active, value);
    }

    [JsonIgnore]
    public string GameId
    {
        get => _gameId;
        set
        {
            string normalizedValue = value ?? string.Empty;
            SetProperty(ref _gameId, normalizedValue);
        }
    }

    [JsonPropertyName("From")]
    public LibraryEnum From
    {
        get => _from;
        set
        {
            if (SetProperty(ref _from, value))
            {
                OnPropertyChanged(nameof(AppIcon));
            }
        }
    }

    [JsonIgnore]
    public string LaunchArguments
    {
        get => _launchArguments;
        set
        {
            string normalizedValue = value ?? string.Empty;
            SetProperty(ref _launchArguments, normalizedValue);
        }
    }

    [JsonIgnore]
    public string Folder
    {
        get => _folder;
        set
        {
            string normalizedValue = value ?? string.Empty;
            if (SetProperty(ref _folder, normalizedValue))
            {
                OnPropertyChanged(nameof(FilePath));
            }
        }
    }

    [JsonIgnore]
    public string Name
    {
        get => _name;
        set
        {
            string normalizedValue = value ?? string.Empty;
            if (SetProperty(ref _name, normalizedValue))
            {
                OnPropertyChanged(nameof(FilePath));
            }
        }
    }

    [JsonIgnore]
    public string Type
    {
        get => _type;
        set
        {
            string normalizedValue = value ?? string.Empty;
            if (SetProperty(ref _type, normalizedValue))
            {
                OnPropertyChanged(nameof(FilePath));
            }
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
            // Split path into folder/name/type so each part can be displayed independently in the grid.
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
        LibraryEnum.Steam => SteamStoreIcon,
        LibraryEnum.EpicGames => EpicStoreIcon,
        _ => null,
    };

    private static BitmapImage? GetIconWithCache(string filePath)
    {
        // Icon extraction can be expensive; cache by path for smoother scrolling.
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

}
