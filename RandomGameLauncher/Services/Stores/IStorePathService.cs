using RandomGameLauncher.Core.Models;

namespace RandomGameLauncher.Services;

/// <summary>
/// Service for discovering launcher installation paths used in status diagnostics.
/// </summary>
public interface IStorePathService
{
    /// <summary>
    /// Returns a known launcher path for the provided store source, or empty when unavailable.
    /// </summary>
    string GetStorePath(GameSource source);
}
