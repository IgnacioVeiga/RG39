using System.Windows;

namespace RandomGameLauncher;

public static class DialogResultBehavior
{
    public static readonly DependencyProperty DialogResultProperty =
        DependencyProperty.RegisterAttached(
            "DialogResult",
            typeof(bool?),
            typeof(DialogResultBehavior),
            new PropertyMetadata(null, OnDialogResultChanged));

    public static void SetDialogResult(Window target, bool? value) =>
        target.SetValue(DialogResultProperty, value);

    public static bool? GetDialogResult(Window target) =>
        (bool?)target.GetValue(DialogResultProperty);

    private static void OnDialogResultChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not Window window)
        {
            return;
        }

        if (args.NewValue is bool dialogResult)
        {
            window.DialogResult = dialogResult;
        }
    }
}
