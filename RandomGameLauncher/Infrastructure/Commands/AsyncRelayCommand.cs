using System.Windows.Input;

namespace RandomGameLauncher;

/// <summary>
/// ICommand implementation for async operations with built-in reentrancy protection.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly Action<Exception>? _onException;
    private bool _isExecuting;

    /// <summary>
    /// Constructor receiving async execute and optional can-execute/error delegates.
    /// </summary>
    public AsyncRelayCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null,
        Action<Exception>? onException = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onException = onException;
    }

    public bool CanExecute(object? parameter) =>
        !_isExecuting && (_canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter) =>
        await ExecuteAsync();

    /// <summary>
    /// Executes the command with reentrancy protection and centralized exception routing.
    /// </summary>
    public async Task ExecuteAsync()
    {
        if (!CanExecute(null))
        {
            return;
        }

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute();
        }
        catch (Exception ex)
        {
            _onException?.Invoke(ex);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Notifies WPF that command execution eligibility may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// Generic async command variant used when the command needs a typed parameter.
/// </summary>
public sealed class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T?, Task> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private readonly Action<Exception>? _onException;
    private bool _isExecuting;

    /// <summary>
    /// Constructor receiving typed async execute and optional can-execute/error delegates.
    /// </summary>
    public AsyncRelayCommand(
        Func<T?, Task> execute,
        Func<T?, bool>? canExecute = null,
        Action<Exception>? onException = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onException = onException;
    }

    public bool CanExecute(object? parameter)
    {
        if (!TryCastParameter(parameter, out T? typedParameter))
        {
            return false;
        }

        return !_isExecuting && (_canExecute?.Invoke(typedParameter) ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (!TryCastParameter(parameter, out T? typedParameter))
        {
            return;
        }

        await ExecuteAsync(typedParameter);
    }

    /// <summary>
    /// Executes the typed command with reentrancy protection and centralized exception routing.
    /// </summary>
    public async Task ExecuteAsync(T? parameter)
    {
        if (_isExecuting || !(_canExecute?.Invoke(parameter) ?? true))
        {
            return;
        }

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute(parameter);
        }
        catch (Exception ex)
        {
            _onException?.Invoke(ex);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Notifies WPF that command execution eligibility may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Converts command parameters to the expected generic type without runtime exceptions.
    /// </summary>
    private static bool TryCastParameter(object? parameter, out T? typedParameter)
    {
        if (parameter is null)
        {
            typedParameter = default;
            return default(T) is null;
        }

        if (parameter is T value)
        {
            typedParameter = value;
            return true;
        }

        typedParameter = default;
        return false;
    }
}
