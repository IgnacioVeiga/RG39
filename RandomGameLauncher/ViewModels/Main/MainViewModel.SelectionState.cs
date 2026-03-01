using CommunityToolkit.Mvvm.Input;
using RandomGameLauncher.Models;
using System.Collections.Specialized;
using System.ComponentModel;

namespace RandomGameLauncher.ViewModels;

/// <summary>
/// Maintains tri-state selection and incremental counters for efficient large-list updates.
/// </summary>
public partial class MainViewModel
{
    /// <summary>
    /// Bulk toggle routine that suppresses per-item side effects until the operation completes.
    /// </summary>
    private void SetAllGamesActive(bool isChecked)
    {
        _isUpdatingActiveState = true;

        try
        {
            foreach (Game game in Games)
            {
                game.Active = isChecked;
            }
        }
        finally
        {
            _isUpdatingActiveState = false;
        }

        _activeGamesCount = isChecked ? _totalGamesCount : 0;
        RunBackgroundTask(PersistManualGamesAsync);
        _randomGameSelector.ResetCycle();
        UpdateHeaderCheckboxState();
    }

    /// <summary>
    /// Header click policy: partial selection becomes all-on, full selection becomes all-off.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanToggleAllActive))]
    private void ToggleAllActive()
    {
        if (_totalGamesCount == 0)
        {
            return;
        }

        bool shouldActivateAll = _activeGamesCount < _totalGamesCount;
        SetAllGamesActive(shouldActivateAll);
    }

    /// <summary>
    /// Recomputes the tri-state value in O(1) using maintained counters.
    /// </summary>
    private void UpdateHeaderCheckboxState()
    {
        bool? newHeaderState = _totalGamesCount switch
        {
            0 => false,
            _ when _activeGamesCount == 0 => false,
            _ when _activeGamesCount == _totalGamesCount => true,
            _ => null
        };

        IsAllActiveChecked = newHeaderState;
    }

    /// <summary>
    /// Collection change hook that updates subscriptions, counters and command states.
    /// </summary>
    private void Games_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (Game game in Games)
            {
                game.PropertyChanged -= Game_PropertyChanged;
                game.PropertyChanged += Game_PropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (Game game in e.NewItems.OfType<Game>())
            {
                game.PropertyChanged += Game_PropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (Game game in e.OldItems.OfType<Game>())
            {
                game.PropertyChanged -= Game_PropertyChanged;
            }
        }

        OnPropertyChanged(nameof(GamesCount));
        OnPropertyChanged(nameof(StatusText));
        RecalculateCountersAfterCollectionChange(e);
        UpdateHeaderCheckboxState();
        RaiseCommandsCanExecuteChanged();
    }

    /// <summary>
    /// Counter maintenance routine for add/remove/reset events without scanning the whole list.
    /// </summary>
    private void RecalculateCountersAfterCollectionChange(NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _totalGamesCount = Games.Count;
            _activeGamesCount = Games.Count(game => game.Active);
            return;
        }

        if (e.NewItems is not null)
        {
            foreach (Game newGame in e.NewItems.OfType<Game>())
            {
                _totalGamesCount++;
                if (newGame.Active)
                {
                    _activeGamesCount++;
                }
            }
        }

        if (e.OldItems is not null)
        {
            foreach (Game oldGame in e.OldItems.OfType<Game>())
            {
                _totalGamesCount--;
                if (oldGame.Active)
                {
                    _activeGamesCount--;
                }
            }
        }

        _totalGamesCount = Math.Max(0, _totalGamesCount);
        _activeGamesCount = Math.Clamp(_activeGamesCount, 0, _totalGamesCount);
    }

    /// <summary>
    /// Per-row change hook used to persist manual edits and reset random cycles when needed.
    /// </summary>
    private void Game_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        bool isActiveChanged = string.Equals(e.PropertyName, nameof(Game.Active), StringComparison.Ordinal);
        bool isLaunchArgsChanged = string.Equals(e.PropertyName, nameof(Game.LaunchArguments), StringComparison.Ordinal);

        if (!isActiveChanged && !isLaunchArgsChanged)
        {
            return;
        }

        if (isActiveChanged)
        {
            if (_isUpdatingActiveState)
            {
                return;
            }

            if (sender is Game changedGame)
            {
                _activeGamesCount += changedGame.Active ? 1 : -1;
                _activeGamesCount = Math.Clamp(_activeGamesCount, 0, _totalGamesCount);
            }

            UpdateHeaderCheckboxState();
            _randomGameSelector.ResetCycle();
        }

        if (sender is Game game && game.From == LibraryEnum.Other)
        {
            RunBackgroundTask(PersistManualGamesAsync);
        }
    }
}
