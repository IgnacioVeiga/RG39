using RandomGameLauncher.Core.Abstractions;
using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Core.Utilities;

public static class GameIdentity
{
    public static string Build(GameEntry game, IPathNormalizer pathNormalizer)
    {
        if (game.Source == GameSource.Other)
        {
            string normalizedPath = pathNormalizer.NormalizeAbsolutePath(game.FilePath) ?? game.FilePath ?? string.Empty;
            return $"{(int)game.Source}|{normalizedPath}";
        }

        return $"{(int)game.Source}|{game.GameId}";
    }
}
