using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMenu.Screens.Menu
{
    public class ShapeMenuItem : VGMenu.Screens.Menu.IShapeMenuItem
    {
        protected ShapeController.SHAPE_RECT MenuBackground;
        protected ShapeController.SHAPE_TEXT MenuText;

        public ShapeMenuItem()
        {
            visible = true;
        }

        public virtual void Create(BaseMenuControl controller, int i, System.Drawing.RectangleF rect, bool highlighted)
        {
            MenuBackground = new ShapeController.SHAPE_RECT()
            {
                id = ShapeController.NewId(),
                x = rect.X,
                y = rect.Y,
                width = rect.Width,
                height = rect.Height,
                backColor = highlighted ? controller.MenuSelectedColor : controller.MenuBackgroundColor,
                visible = true
            };

            MenuText = new ShapeController.SHAPE_TEXT()
            {
                id = ShapeController.NewId(),
                x = rect.X,
                y = rect.Y + rect.Height * 0.25f,

                width = rect.Width,
                height = rect.Height,

                color = controller.MenuTextColor,
                text = "",
                size = rect.Height * 0.5f,
                visible = true
            };

            ShapeController.SetRect(MenuBackground);
            ShapeController.SetText(MenuText);
        }

        public virtual void Update(BaseMenuControl controller, int i, MenuItem itm, bool highlighted)
        {
            string text = itm == null ? "" : itm.Text;

            // update shape_text[i] to text
            var backColor = highlighted ? controller.MenuSelectedColor : controller.MenuBackgroundColor;
            if (string.IsNullOrEmpty(text))
                backColor.A = 0;

            MenuBackground.backColor = backColor;
            ShapeController.SetRect(MenuBackground);

            if (MenuText.text != text)
            {
                MenuText.text = text;
                ShapeController.SetText(MenuText);
            }
        }

        public virtual void Dispose()
        {
            ShapeController.Remove(MenuBackground.id);
            ShapeController.Remove(MenuText.id);
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

                    MenuBackground.visible = value;
                    ShapeController.SetRect(MenuBackground);
                    MenuText.visible = value;
                    ShapeController.SetText(MenuText);
                }
            }
        }

    }
}
