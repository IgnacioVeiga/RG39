using Microsoft.Win32;
using RandomGameLauncher.Resources.Language;

namespace RandomGameLauncher.Services;

public sealed class ExecutableFilePicker : IExecutablePicker
{
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
