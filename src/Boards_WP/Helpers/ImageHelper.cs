using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;

namespace Boards_WP.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage ConvertToBitmap(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            var bitmap = new BitmapImage();
            using var ms = new MemoryStream(data);
            bitmap.SetSource(ms.AsRandomAccessStream());
            return bitmap;
        }

        public static BitmapImage ConvertUrlToBitmap(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                return new BitmapImage(new Uri(url));
            }
            catch
            {
                return null;
            }
        }
    }
}