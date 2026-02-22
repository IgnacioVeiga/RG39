using Microsoft.Win32;
using RandomGameLauncher.Resources.Language;

namespace RandomGameLauncher.Services;

/// <summary>
/// Windows file picker for selecting executable files.
/// </summary>
public sealed class ExecutableFilePicker
{
    /// <summary>
    /// Opens the native file dialog and returns an executable path or null when canceled.
    /// </summary>
    public string? SelectExecutableFile()
    {
        OpenFileDialog openFileDialog = new()
        {
            Title = Strings.SEL_EXE_TITLE,
            Filter = "(*.exe)|*.exe",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        };

        return openFileDialog.ShowDialog() is true
            ? openFileDialog.FileName
            : null;
    }
}
