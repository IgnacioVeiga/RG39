using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace RandomGameLauncher;

/// <summary>
/// UI image helper methods used by view models and models.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Converts raw image bytes to a frozen WPF bitmap loaded in memory.
    /// </summary>
    public static BitmapImage ByteArrayToImage(byte[] imageData)
    {
        using MemoryStream stream = new(imageData);

        BitmapImage image = new();
        image.BeginInit();
        image.StreamSource = stream;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();

        return image;
    }

    /// <summary>
    /// Extracts an executable icon and converts it to a PNG-backed WPF bitmap.
    /// </summary>
    public static BitmapImage? ExtractIconFromExe(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        using Icon? icon = Icon.ExtractAssociatedIcon(filePath);
        if (icon is null)
        {
            return null;
        }

        using MemoryStream stream = new();
        using Bitmap bitmap = icon.ToBitmap();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;

        BitmapImage image = new();
        image.BeginInit();
        image.StreamSource = stream;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();

        return image;
    }
}
