using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;

namespace VGMenu.Screens.Details
{
    public class ShapeSubtitleMenuItem : VGMenu.Screens.Menu.IShapeMenuItem
    {
        protected ShapeController.SHAPE_RECT menuBackground;
        protected ShapeController.SHAPE_TEXT menuText;
        protected ShapeController.SHAPE_TEXT menuSelectedSubtitle;

        public ShapeSubtitleMenuItem()
        {
            visible = true;
        }

        public virtual void Create(BaseMenuControl controller, int i, System.Drawing.RectangleF rect, bool highlighted)
        {
            menuBackground = new ShapeController.SHAPE_RECT()
            {
                id = ShapeController.NewId(),
                x = rect.X,
                y = rect.Y,
                width = rect.Width,
                height = rect.Height,
                backColor = highlighted ? controller.MenuSelectedColor : controller.MenuBackgroundColor,
                visible = true
            };

            menuText = new ShapeController.SHAPE_TEXT()
            {
                id = ShapeController.NewId(),
                x = rect.X,
                y = rect.Y + ThemeManager.DefaultPadding,

                width = rect.Width,
                height = rect.Height * 0.5f,

                color = controller.MenuTextColor,
                text = "Subtitles",
                size = rect.Height * 0.5f,
                visible = true
            };

            menuSelectedSubtitle = new ShapeController.SHAPE_TEXT()
            {
                id = ShapeController.NewId(),
                x = rect.X,
                y = menuText.y + menuText.height + ThemeManager.DefaultPadding,

                width = rect.Width,
                height = rect.Height * 0.25f,

                color = controller.MenuTextColor,
                text = "",
                size = rect.Height * 0.25f,
                visible = true
            };


            ShapeController.SetRect(menuBackground);
            ShapeController.SetText(menuText);
            ShapeController.SetText(menuSelectedSubtitle);
        }

        public virtual void Update(BaseMenuControl controller, int i, MenuItem itm, bool highlighted)
        {
            if (itm is SubtitleMenuItem)
            {
                var sitm = (SubtitleMenuItem)itm;
                if (sitm.Text != menuSelectedSubtitle.text)
                {
                    menuSelectedSubtitle.text = sitm.Text;
                    ShapeController.SetText(menuSelectedSubtitle);
                }

                string text = itm == null ? "" : itm.Text;

                if (menuText.text != "Subtitles")
                {
                    menuText.text = "Subtitles";
                    ShapeController.SetText(menuText);
                }
            }

            var backColor = highlighted ? controller.MenuSelectedColor : controller.MenuBackgroundColor;
            menuBackground.backColor = backColor;
            ShapeController.SetRect(menuBackground);
        }

        public virtual void Dispose()
        {
            ShapeController.Remove(menuBackground.id);
            ShapeController.Remove(menuText.id);
            ShapeController.Remove(menuSelectedSubtitle.id);
        }


        private bool visible;
        public virtual bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;

                    menuBackground.visible = value;
                    ShapeController.SetRect(menuBackground);
                    menuText.visible = value;
                    ShapeController.SetText(menuText);
                    menuSelectedSubtitle.visible = value;
                    ShapeController.SetText(menuSelectedSubtitle);
                }
            }
        }

    }
}
