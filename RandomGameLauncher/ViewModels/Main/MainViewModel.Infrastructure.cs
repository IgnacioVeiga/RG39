using RandomGameLauncher.Models;

namespace RandomGameLauncher.ViewModels;

/// <summary>
/// Internal infrastructure helpers for command state and fire-and-forget task safety.
/// </summary>
public partial class MainViewModel
{
    /// <summary>
    /// Returns true when interactive commands should be available.
    /// </summary>
    private bool CanUseInteractiveCommands() => !IsLoading;

    /// <summary>
    /// Returns true when the header selection toggle can be executed.
    /// </summary>
    private bool CanToggleAllActive() => !IsLoading && _totalGamesCount > 0;

    /// <summary>
    /// Returns true when a row launch action can execute.
    /// </summary>
    private bool CanRunGame(Game? game) => game is not null && !IsLoading;

    /// <summary>
    /// Returns true when a row can be removed from the manual list.
    /// </summary>
    private bool CanRemoveGame(Game? game) =>
        game is not null &&
        game.From == LibraryEnum.Other &&
        !IsLoading;

    /// <summary>
    /// Raises CanExecute updates for all command instances exposed by the view model.
    /// </summary>
    private void RaiseCommandsCanExecuteChanged()
    {
        PlayRandomGameCommand.NotifyCanExecuteChanged();
        AddGameCommand.NotifyCanExecuteChanged();
        RunGameCommand.NotifyCanExecuteChanged();
        RemoveGameCommand.NotifyCanExecuteChanged();
        ClearListCommand.NotifyCanExecuteChanged();
        ToggleAllActiveCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Executes a command body and keeps exception reporting centralized.
    /// </summary>
    private async Task ExecuteCommandSafeAsync(Func<Task> taskFactory)
    {
        try
        {
            await taskFactory();
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellations from stale operations.
        }
        catch (Exception ex)
        {
            ShowUnhandledError(ex);
        }
    }

    /// <summary>
    /// Launches a background task with centralized exception handling.
    /// </summary>
    private void RunBackgroundTask(Func<Task> taskFactory)
    {
        _ = ExecuteCommandSafeAsync(taskFactory);
    }

    /// <summary>
    /// Last-resort user-facing error display for unhandled command exceptions.
    /// </summary>
    private static void ShowUnhandledError(Exception ex) =>
        System.Windows.MessageBox.Show(ex.Message);
}
