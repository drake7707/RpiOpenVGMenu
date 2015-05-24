using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;
using System.Drawing;

namespace VGMenu.Screens.MainMenu.Menu
{
    public class ThumbMenuControl : BaseMenuControl
    {

        public ThumbMenuControl(PointF location, SizeF size)
            : base(location, size, 0.2f, 0.01f, true)
        {
            MenuBackgroundColor = ThemeManager.MenuItemBackground;
            MenuTextColor = ThemeManager.MenuItemForeground;
            MenuSelectedColor = ThemeManager.MenuItemHighlighted;
            Columns = 7;
        }

        protected override IShapeMenuItem CreateMenuItem(int i, RectangleF rect, bool highlighted)
        {
            ShapeThumbMenuItem itm = new ShapeThumbMenuItem();
            itm.Create(this, i, rect, highlighted);
            return itm;
        }

        public override void OnEnter()
        {
            SelectedIndex = 0;
        }

        public override void OnLeave()
        {
            SelectedIndex = -1;
        }
        
    }
}
