namespace RandomGameLauncher.ViewModels;

/// <summary>
/// UI projection for a selectable language entry in the settings menu.
/// </summary>
public sealed record LanguageOptionItem(string Key, string DisplayName, bool IsCurrent)
{
    public bool IsSelectable => !IsCurrent;
}
