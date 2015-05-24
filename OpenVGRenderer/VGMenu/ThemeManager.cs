using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Configuration;

namespace VGMenu
{
    public class ThemeManager
    {

        static ThemeManager() {
    

            MenuItemBackground = GetColorFromConfig("menuItemBackground", Color.FromArgb(192, 64, 64, 64).ToShapeColor());
            MenuItemForeground = GetColorFromConfig("menuItemForeground", Color.FromArgb(255, 255, 255, 255).ToShapeColor());
            MenuItemHighlighted = GetColorFromConfig("menuItemHighlighted", Color.FromArgb(192, 255, 255, 0).ToShapeColor());

        }

        private static ShapeController.COLOR GetColorFromConfig(string key, ShapeController.COLOR defaultColor)
        {
            string str = ConfigurationManager.AppSettings[key];
            var parts = str.Split(',');
            if (parts.Length == 4)
            {
                byte[] values = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    byte val;
                    if (byte.TryParse(parts[i], out val))
                        values[i] = val;
                }
                return new ShapeController.COLOR()
                {
                    A = values[0],
                    R = values[1],
                    G = values[2],
                    B = values[3]
                };
            }

            return defaultColor;
        }

        public static ShapeController.COLOR MenuItemBackground;
        public static ShapeController.COLOR MenuItemForeground;
        public static ShapeController.COLOR MenuItemHighlighted;

        public const float DefaultPadding = 0.005f;
    }
}
