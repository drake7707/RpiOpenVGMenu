using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace VGMenu.Screens.MainMenu.Menu
{
    public class ShapeRecentMenuTitle : Control
    {
        protected ShapeController.SHAPE_RECT MenuBackground;
        protected ShapeController.SHAPE_TEXT MenuText;

        public RectangleF Bounds { get; private set; }
        public ShapeRecentMenuTitle(System.Drawing.RectangleF rect, string text, ShapeController.COLOR backColor, ShapeController.COLOR foreColor)
        {
            this.Bounds = rect;
            MenuBackground = new ShapeController.SHAPE_RECT()
            {
                id = ShapeController.NewId(),
                x = rect.X,
                y = rect.Y,
                width = rect.Width,
                height = rect.Height,
                backColor = backColor,
                visible = !string.IsNullOrEmpty(text)
            };

            MenuText = new ShapeController.SHAPE_TEXT()
            {
                id = ShapeController.NewId(),
                x = rect.X,
                y = rect.Y + rect.Height * 0.25f,

                width = rect.Width,
                height = rect.Height,

                color = foreColor,
                text = text,
                size = rect.Height * 0.5f,
                visible = !string.IsNullOrEmpty(text)
            };

            ShapeController.SetRect(MenuBackground);
            ShapeController.SetText(MenuText);
        }

        public void UpdateText(string text)
        {
            if (MenuText.text != text || (MenuText.visible != (Visible && !string.IsNullOrEmpty(text))))
            {
                MenuText.text = text;
                MenuText.visible = Visible && !string.IsNullOrEmpty(text);
                ShapeController.SetText(MenuText);
            }

            if (MenuBackground.visible != !string.IsNullOrEmpty(text))
            {
                MenuBackground.visible = Visible && !string.IsNullOrEmpty(text);
                ShapeController.SetRect(MenuBackground);
            }
        }

        public override void Dispose()
        {
            ShapeController.Remove(MenuBackground.id);
            ShapeController.Remove(MenuText.id);
        }


        public override bool Visible
        {
            get { return base.Visible; }
            set
            {
                if (base.Visible != value)
                {
                    base.Visible = value;
                    MenuBackground.visible = value;
                    ShapeController.SetRect(MenuBackground);
                    MenuText.visible = value;
                    ShapeController.SetText(MenuText);
                }
            }
        }
    }
}
