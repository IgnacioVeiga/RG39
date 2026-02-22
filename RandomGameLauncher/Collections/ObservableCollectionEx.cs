using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace RandomGameLauncher.Collections;

/// <summary>
/// Observable collection variant with AddRange support.
/// The implementation emits a single reset event to avoid excessive UI refresh churn.
/// </summary>
public sealed class ObservableCollectionEx<T> : ObservableCollection<T>
{
    /// <summary>
    /// Adds a batch and emits one reset notification.
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        if (items is null)
        {
            return;
        }

        foreach (T item in items)
        {
            Items.Add(item);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
