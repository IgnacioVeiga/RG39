using Microsoft.WindowsAPICodePack.Dialogs;
using RG39.Language;
using System;

namespace RG39.Util
{
    internal static class Picker
    {
        internal static string SelectExecutable()
        {
            CommonOpenFileDialog exe = new()
            {
                Title = Strings.SEL_EXE_TITLE,
                Multiselect = false,
                EnsurePathExists = true,
                EnsureFileExists = true,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };

            if (exe.ShowDialog() == CommonFileDialogResult.Ok) return exe.FileName;
            else return null;
        }
    }
}
