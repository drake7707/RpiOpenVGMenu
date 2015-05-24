using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using VGMenu.Screens.Menu;
using System.Globalization;

namespace VGMenu.Screens.Details
{
    public class DetailsMenuControl : BaseMenuControl
    {

        public DetailsMenuControl(IDetailItem detailItem, PointF location, SizeF size)
            : base(location,size, 0.1f, 0.01f, true)
        {
            MenuBackgroundColor = ThemeManager.MenuItemBackground;
            MenuTextColor = ThemeManager.MenuItemForeground;
            MenuSelectedColor = ThemeManager.MenuItemHighlighted;
            Columns = 2;

            Items.Add(new SubtitleMenuItem(detailItem));
            Items.Add(new MenuItem() { Text = "PLAY" });
            Initialize();
            SelectedIndex = 0;
        }

        public void ToggleSubtitles()
        {
            ((SubtitleMenuItem)Items[0]).Toggle();

            Refresh();
        }

        public CultureInfo SelectedLanguage
        {
            get
            {
                return ((SubtitleMenuItem)Items[0]).SelectedLanguage;
            }
        }

        public bool UseSubtitles { get { return ((SubtitleMenuItem)Items[0]).UseSubtitles; } }


        protected override IShapeMenuItem CreateMenuItem(int i, RectangleF rect, bool highlighted)
        {
            if (i >= 0 && i < Items.Count && Items[i] is SubtitleMenuItem)
            {
                var itm = new ShapeSubtitleMenuItem();
                itm.Create(this, i, rect, highlighted);
                return itm;
            }
            else 
                return base.CreateMenuItem(i, rect, highlighted);
        }
    }
}
