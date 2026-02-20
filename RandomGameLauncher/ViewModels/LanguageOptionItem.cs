namespace RandomGameLauncher.ViewModels;

public sealed record LanguageOptionItem(string Key, string DisplayName, bool IsCurrent)
{
    public bool IsSelectable => !IsCurrent;
}
