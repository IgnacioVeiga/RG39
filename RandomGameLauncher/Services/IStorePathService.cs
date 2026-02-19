using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Services;

public interface IStorePathService
{
    string GetStorePath(GameSource source);
}
