using Microsoft.UI.Xaml.Media;
using System;
using Boards_WP.Data.Models;

namespace Boards_WP.Helpers
{
    public static class ColorHelper
    {
        public static Brush GetBrushForCategory(int categoryId)
        {
            var themeColor = CategoryThemeMapper.GetColorForCategoryId(categoryId);
            Windows.UI.Color color = themeColor switch
            {
                ThemeColor.Pink => Windows.UI.Color.FromArgb(255, 255, 20, 147),
                ThemeColor.Orange => Windows.UI.Color.FromArgb(255, 255, 140, 0),
                ThemeColor.Turquoise => Windows.UI.Color.FromArgb(255, 0, 139, 139),
                ThemeColor.Yellow => Windows.UI.Color.FromArgb(255, 218, 165, 32),
                ThemeColor.Blue => Windows.UI.Color.FromArgb(255, 0, 0, 255),
                ThemeColor.Green => Windows.UI.Color.FromArgb(255, 0, 128, 0),
                ThemeColor.Red => Windows.UI.Color.FromArgb(255, 220, 20, 60),
                ThemeColor.Purple => Windows.UI.Color.FromArgb(255, 128, 0, 128),
                _ => Windows.UI.Color.FromArgb(255, 120, 90, 200)
            };

            return new SolidColorBrush(color);
        }

        public static Brush HexToBrush(string hex)
        {
            if (string.IsNullOrEmpty(hex) || !hex.StartsWith("#") || (hex.Length != 7 && hex.Length != 9))
            {
                return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }

            try
            {
                if (hex.Length == 7)
                {
                    byte r = Convert.ToByte(hex.Substring(1, 2), 16);
                    byte g = Convert.ToByte(hex.Substring(3, 2), 16);
                    byte b = Convert.ToByte(hex.Substring(5, 2), 16);
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
                }
                else 
                {
                    byte a = Convert.ToByte(hex.Substring(1, 2), 16);
                    byte r = Convert.ToByte(hex.Substring(3, 2), 16);
                    byte g = Convert.ToByte(hex.Substring(5, 2), 16);
                    byte b = Convert.ToByte(hex.Substring(7, 2), 16);
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
                }
            }
            catch
            {
                return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }
    }
}