using RandomGameLauncher.Core.Models;
using RandomGameLauncher.Models;
using RandomGameLauncher.Resources.Language;
using System.Windows;

namespace RandomGameLauncher.ViewModels;

/// <summary>
/// Command handlers for game lifecycle actions (load, add, play, remove, clear).
/// </summary>
public partial class MainViewModel
{
    /// <summary>
    /// Performs startup loading without blocking the UI thread.
    /// </summary>
    private async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            ConfigureStorePathStatus();

            IReadOnlyList<Game> loadedGames = await _gameCatalogService.LoadGamesAsync();
            Games.AddRange(loadedGames);

            OnPropertyChanged(nameof(GamesCount));
            UpdateHeaderCheckboxState();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Picks a random active game and delegates launch to the platform launcher.
    /// </summary>
    private async Task PlayRandomGameAsync()
    {
        List<Game> activeGames = Games.Where(game => game.Active).ToList();

        if (activeGames.Count == 0)
        {
            MessageBox.Show(Strings.CANNOT_LOAD_GAME_MSG, Strings.PLAY_GAME, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IReadOnlyList<GameEntry> activeEntries = activeGames.Select(_gameCatalogService.ToCoreGameEntry).ToList();
        GameEntry? randomEntry = _randomGameSelector.PickNext(activeEntries);

        if (randomEntry is null)
        {
            MessageBox.Show(Strings.CANNOT_LOAD_GAME_MSG, Strings.PLAY_GAME, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string selectedId = _gameCatalogService.BuildIdentity(randomEntry);
        Game? gameToLaunch = activeGames.FirstOrDefault(game =>
            string.Equals(_gameCatalogService.BuildIdentity(game), selectedId, StringComparison.OrdinalIgnoreCase));

        if (gameToLaunch is null)
        {
            return;
        }

        await RunGameAsync(gameToLaunch);
    }

    /// <summary>
    /// Opens the add-game dialog, validates the selected executable, and persists manual entries.
    /// </summary>
    private async Task AddGameAsync()
    {
        AddGameDialogResult dialogResult = _addGameDialogService.ShowDialog();
        if (!dialogResult.Accepted)
        {
            return;
        }

        string selectedFilePath = dialogResult.FilePath;
        string selectedLaunchArguments = dialogResult.LaunchArguments;

        if (!_gameCatalogService.TryCreateManualGame(selectedFilePath, selectedLaunchArguments, out Game? newGame))
        {
            MessageBox.Show($"{Strings.CANNOT_LOAD_GAME_MSG}\n\"{selectedFilePath}\"");
            return;
        }

        string newGameId = _gameCatalogService.BuildIdentity(newGame);
        bool alreadyExists = Games.Any(game =>
            game.From == LibraryEnum.Other &&
            string.Equals(_gameCatalogService.BuildIdentity(game), newGameId, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            MessageBox.Show($"\"{newGame.FilePath}\"\n {Strings.REPEATED_GAME_MSG}", Strings.REPEATED_TITLE);
            return;
        }

        Games.Add(newGame);
        await PersistManualGamesAsync();
        _randomGameSelector.ResetCycle();
    }

    /// <summary>
    /// Launches a selected game and closes the application on success.
    /// </summary>
    private async Task RunGameAsync(Game? game)
    {
        if (game is null)
        {
            return;
        }

        try
        {
            await _gameLauncher.LaunchAsync(_gameCatalogService.ToCoreGameEntry(game));
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}\n-> '{game.Name}' <-");
        }
    }

    /// <summary>
    /// Removes a manual game after a user confirmation and persists the new snapshot.
    /// </summary>
    private async Task RemoveGameAsync(Game? game)
    {
        if (game is null || game.From != LibraryEnum.Other)
        {
            return;
        }

        string message = $"{Strings.REMOVE_GAME_MSG}\n{game.Name}?";
        MessageBoxResult result = MessageBox.Show(message, string.Empty, MessageBoxButton.YesNo);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        Games.Remove(game);
        await PersistManualGamesAsync();
        _randomGameSelector.ResetCycle();
    }

    /// <summary>
    /// Clears all manual games while keeping store-discovered entries untouched.
    /// </summary>
    private async Task ClearListAsync()
    {
        MessageBoxResult result = MessageBox.Show(Strings.CLEAR_LIST_MSG, Strings.CLEAR_LIST, MessageBoxButton.YesNo);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        for (int index = Games.Count - 1; index >= 0; index--)
        {
            if (Games[index].From == LibraryEnum.Other)
            {
                Games.RemoveAt(index);
            }
        }

        await _gameCatalogService.ClearManualGamesAsync();
        _randomGameSelector.ResetCycle();
    }

    /// <summary>
    /// Saves the current manual game snapshot to the repository.
    /// </summary>
    private Task PersistManualGamesAsync() =>
        _gameCatalogService.SaveManualGamesAsync(Games);
}
