using RandomGameLauncher.Core.Services;

namespace RandomGameLauncher.Core.Tests.Utilities;

public class WindowsPathNormalizerTests
{
    private readonly WindowsPathNormalizer _sut = new();

    [Fact]
    public void AreEquivalent_IsCaseInsensitiveForWindowsPaths()
    {
        bool areEquivalent = _sut.AreEquivalent(@"C:\\Games\\MyGame.exe", @"c:\\games\\MYGAME.exe");

        Assert.True(areEquivalent);
    }

    [Fact]
    public void NormalizeAbsolutePath_ReturnsNull_WhenPathIsEmpty()
    {
        string? normalizedPath = _sut.NormalizeAbsolutePath("   ");

        Assert.Null(normalizedPath);
    }
}
