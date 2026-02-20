using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace RandomGameLauncher
{
    /// <summary>
    /// Image helpers shared by UI models and view models.
    /// </summary>
    public static class Utils
    {
        public static BitmapImage ByteArrayToImage(byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        public static BitmapImage? ExtractIconFromExe(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            using Icon? icon = Icon.ExtractAssociatedIcon(filePath);
            if (icon is null)
            {
                return null;
            }

            using var ms = new MemoryStream();
            using Bitmap bitmap = icon.ToBitmap();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }

    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Adds a batch and emits a single reset notification for UI refresh efficiency.
        /// </summary>
        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) return;

            foreach (var item in collection)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
