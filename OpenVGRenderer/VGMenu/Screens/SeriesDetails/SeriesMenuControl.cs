using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;
using System.Drawing;
using VGMenu.Screens.MainMenu.Menu;

namespace VGMenu.Screens.SeriesDetails
{
    public class SeriesMenuControl : BaseMenuControl
    {

        public SeriesMenuControl(PointF pos, SizeF size)
            : base(pos, size, 0.2f, 0.01f, false)
        {
            MenuBackgroundColor = ThemeManager.MenuItemBackground;
            MenuTextColor = ThemeManager.MenuItemForeground;
            MenuSelectedColor = ThemeManager.MenuItemHighlighted;

            ScrollBarLocation = new PointF(0.965f, 0.2f);
            ScrollBarSize = new SizeF(0.02f, 0.75f);
            Columns = 1;
        }

        protected override IShapeMenuItem CreateMenuItem(int i, RectangleF rect, bool highlighted)
        {
            ShapeSubMenuItem itm = new ShapeSubMenuItem();
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
