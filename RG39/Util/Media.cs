using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace RG39.Util
{
    internal class Media
    {
        // ToDo: Acá van las funciones para mostrar imagenes, caratulas y/o sfx
    }
    
    // source: https://stackoverflow.com/questions/1127647/convert-system-drawing-icon-to-system-media-imagesource
    internal static class IconUtilities
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ToImageSource(this Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            if (!DeleteObject(hBitmap)) throw new Win32Exception();

            return wpfBitmap;
        }
    }
}
