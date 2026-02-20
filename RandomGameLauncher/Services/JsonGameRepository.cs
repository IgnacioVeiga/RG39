using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;
using System.IO;
using System.Text.Json;

namespace RandomGameLauncher.Services;

/// <summary>
/// Persists manual entries in local app data and transparently migrates legacy storage
/// from older app versions when needed.
/// </summary>
public sealed class JsonGameRepository : IGameRepository
{
    private const string StorageFileName = "list.json";
    private readonly IPathNormalizer _pathNormalizer;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _storageDirectoryPath;
    private readonly string _storageFilePath;
    private readonly string[] _legacyStorageCandidates;

    public JsonGameRepository(IPathNormalizer pathNormalizer)
    {
        _pathNormalizer = pathNormalizer;

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _storageDirectoryPath = Path.Combine(localAppData, "RandomGameLauncher");
        _storageFilePath = Path.Combine(_storageDirectoryPath, StorageFileName);

        _legacyStorageCandidates =
        [
            Path.Combine(Environment.CurrentDirectory, StorageFileName),
            Path.Combine(AppContext.BaseDirectory, StorageFileName)
        ];
    }

    public async Task<IReadOnlyList<StoredGame>> LoadAsync(CancellationToken ct = default)
    {
        // Migration runs before reads so users keep old data without manual steps.
        await MigrateLegacyIfNeededAsync(ct);

        if (!File.Exists(_storageFilePath))
        {
            return [];
        }

        try
        {
            await using FileStream stream = File.OpenRead(_storageFilePath);
            List<StoredGame>? games = await JsonSerializer.DeserializeAsync<List<StoredGame>>(stream, cancellationToken: ct);

            if (games is null)
            {
                return [];
            }

            return games
                .Select(ValidateAndNormalize)
                .Where(g => g is not null)
                .Select(g => g!)
                .DistinctBy(g => g.FilePath, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task SaveAsync(IReadOnlyList<StoredGame> games, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        Directory.CreateDirectory(_storageDirectoryPath);

        List<StoredGame> normalizedGames = games
            .Select(ValidateAndNormalize)
            .Where(g => g is not null)
            .Select(g => g!)
            .DistinctBy(g => g.FilePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        await using FileStream stream = File.Create(_storageFilePath);
        await JsonSerializer.SerializeAsync(stream, normalizedGames, _jsonOptions, ct);
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (File.Exists(_storageFilePath))
        {
            File.Delete(_storageFilePath);
        }

        return Task.CompletedTask;
    }

    private async Task MigrateLegacyIfNeededAsync(CancellationToken ct)
    {
        if (File.Exists(_storageFilePath))
        {
            return;
        }

        string? legacyFilePath = _legacyStorageCandidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(File.Exists);

        if (legacyFilePath is null)
        {
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(legacyFilePath, ct);
            List<StoredGame>? legacyGames = JsonSerializer.Deserialize<List<StoredGame>>(json);
            if (legacyGames is null)
            {
                return;
            }

            await SaveAsync(legacyGames, ct);
            File.Delete(legacyFilePath);
        }
        catch
        {
            // Migration is best-effort. Legacy file is left untouched on failure.
        }
    }

    private StoredGame? ValidateAndNormalize(StoredGame game)
    {
        // Validation is strict to avoid persisting broken or stale paths.
        string? normalizedPath = _pathNormalizer.NormalizeAbsolutePath(game.FilePath);
        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return null;
        }

        if (!string.Equals(Path.GetExtension(normalizedPath), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!File.Exists(normalizedPath))
        {
            return null;
        }

        string launchArguments = game.LaunchArguments ?? string.Empty;
        return game with
        {
            FilePath = normalizedPath,
            LaunchArguments = launchArguments
        };
    }
}
