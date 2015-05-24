using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using VGMenu.Screens.Menu;

namespace VGMenu.Screens.MainMenu.Menu
{
    public class MainMenuControl : BaseMenuControl
    {
        public MainMenuControl()
            : base(new PointF(ThemeManager.DefaultPadding, 0.2f), new SizeF(0.2f, 0.8f), 0.1f, 0.01f, true)
        {
            MenuBackgroundColor = ThemeManager.MenuItemBackground;
            MenuTextColor = ThemeManager.MenuItemForeground;
            MenuSelectedColor = ThemeManager.MenuItemHighlighted;

            Items.Add(new MenuItem() { Text = "RECENT" });
            Items.Add(new MenuItem() { Text = "MOVIES" });
            Items.Add(new MenuItem() { Text = "EPISODES" });
            Initialize();

            SelectedIndex = 0;
        }
    }
}
