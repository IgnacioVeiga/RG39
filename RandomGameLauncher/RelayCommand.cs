using System.Windows.Input;

namespace RandomGameLauncher;

/// <summary>
/// Minimal synchronous ICommand implementation for UI actions that do not require async flow.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) =>
        _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) =>
        _execute();

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// Typed synchronous command variant that safely handles nullable command parameters.
/// </summary>
public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (!TryCastParameter(parameter, out T? typedParameter))
        {
            return false;
        }

        return _canExecute?.Invoke(typedParameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (!TryCastParameter(parameter, out T? typedParameter))
        {
            return;
        }

        _execute(typedParameter);
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);

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
