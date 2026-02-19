using RandomGameLauncher.Core.Abstractions;

namespace RandomGameLauncher.Core.Services;

public sealed class WindowsPathNormalizer : IPathNormalizer
{
    public string? NormalizeAbsolutePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            string fullPath = Path.GetFullPath(path.Trim());

            // Keep normalization stable for comparisons while preserving a valid Windows-like path.
            return fullPath
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
        catch
        {
            return null;
        }
    }

    public bool AreEquivalent(string? pathA, string? pathB)
    {
        string? normalizedA = NormalizeAbsolutePath(pathA);
        string? normalizedB = NormalizeAbsolutePath(pathB);

        if (normalizedA is null || normalizedB is null)
        {
            return false;
        }

        return string.Equals(normalizedA, normalizedB, StringComparison.OrdinalIgnoreCase);
    }
}
