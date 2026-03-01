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
    /// Raises CanExecute updates for all command instances exposed by the view model.
    /// </summary>
    private void RaiseCommandsCanExecuteChanged()
    {
        _playRandomGameCommand.NotifyCanExecuteChanged();
        _addGameCommand.NotifyCanExecuteChanged();
        _runGameCommand.NotifyCanExecuteChanged();
        _removeGameCommand.NotifyCanExecuteChanged();
        _clearListCommand.NotifyCanExecuteChanged();
        _toggleAllActiveCommand.NotifyCanExecuteChanged();
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
        _ = RunBackgroundTaskCoreAsync(taskFactory);
    }

    /// <summary>
    /// Handles cancellation and unexpected failures from fire-and-forget routines.
    /// </summary>
    private async Task RunBackgroundTaskCoreAsync(Func<Task> taskFactory)
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
    /// Last-resort user-facing error display for unhandled command exceptions.
    /// </summary>
    private static void ShowUnhandledError(Exception ex) =>
        System.Windows.MessageBox.Show(ex.Message);
}
