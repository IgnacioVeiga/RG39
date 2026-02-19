namespace RandomGameLauncher.Core.Abstractions;

public interface IPathNormalizer
{
    string? NormalizeAbsolutePath(string? path);
    bool AreEquivalent(string? pathA, string? pathB);
}
