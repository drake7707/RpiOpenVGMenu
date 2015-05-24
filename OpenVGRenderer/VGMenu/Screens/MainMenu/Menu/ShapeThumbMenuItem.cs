using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;

namespace VGMenu.Screens.MainMenu.Menu
{
    public class ShapeThumbMenuItem : IDisposable, IShapeMenuItem
    {

        private ShapeController.SHAPE_IMAGE shapeMenuThumb;
        private ShapeController.SHAPE_RECT shapeMenuBackground;
        private ShapeController.SHAPE_TEXT shapeMenuText;

        public void Create(BaseMenuControl controller, int i, System.Drawing.RectangleF rect, bool highlighted)
        {
            shapeMenuBackground = new ShapeController.SHAPE_RECT()
            {
                id = ShapeController.NewId(),
                x = rect.X,
                y = rect.Y,
                width = rect.Width,
                height = rect.Height,
                backColor = highlighted ? controller.MenuSelectedColor : controller.MenuBackgroundColor,
                visible = true
            };

            shapeMenuThumb = new ShapeController.SHAPE_IMAGE()
            {
                id = ShapeController.NewId(),
                x = rect.X + ThemeManager.DefaultPadding,
                y = rect.Y + rect.Height * 0.03f,
                width = rect.Width - 2 * ThemeManager.DefaultPadding,
                height = rect.Height - 2 * ThemeManager.DefaultPadding,
                filename = "",
                visible = true
            };
            shapeMenuThumb.width = 0.10f;


            float textHeight = (rect.Height - 2 * ThemeManager.DefaultPadding) / 3;
            shapeMenuText = new ShapeController.SHAPE_TEXT()
            {
                id = ShapeController.NewId(),
                x = rect.X + ThemeManager.DefaultPadding,
                y = rect.Y + textHeight,
                width = rect.Width - 2 * ThemeManager.DefaultPadding,
                height = rect.Height - 2 * ThemeManager.DefaultPadding,
                size = textHeight,
                color = ThemeManager.MenuItemForeground,
                text = "",
                visible = true
            };

            ShapeController.SetRect(shapeMenuBackground);
            ShapeController.SetImage(shapeMenuThumb);
            ShapeController.SetText(shapeMenuText);
        }

        public void Update(BaseMenuControl controller, int i, MenuItem itm, bool highlighted)
        {
            if (itm != null)
            {
                // update shape_text[i] to text
                var backColor = highlighted ? controller.MenuSelectedColor : controller.MenuBackgroundColor;

                shapeMenuBackground.backColor = backColor;
                ShapeController.SetRect(shapeMenuBackground);

                bool hasChanged = false;
                if (shapeMenuThumb.filename != itm.Icon)
                {
                    shapeMenuThumb.filename = itm.Icon;
                    hasChanged = true;
                }





                if (shapeMenuThumb.visible != true)
                {
                    shapeMenuThumb.visible = true;
                    hasChanged = true;
                }

                if (hasChanged)
                    ShapeController.SetImage(shapeMenuThumb);

                if (shapeMenuText.text != itm.IconTextOverlay + "")
                {
                    shapeMenuText.text = itm.IconTextOverlay + "";
                    ShapeController.SetText(shapeMenuText);
                }
            }
            else
            {
                if (shapeMenuThumb.visible != false)
                {
                    shapeMenuThumb.visible = false;
                    ShapeController.SetImage(shapeMenuThumb);
                }

                if (shapeMenuBackground.visible != false)
                {
                    shapeMenuBackground.visible = false;
                    ShapeController.SetRect(shapeMenuBackground);
                }

                if (shapeMenuText.visible != false)
                {
                    shapeMenuText.visible = false;
                    ShapeController.SetText(shapeMenuText);
                }
            }
        }

        public void Dispose()
        {
            ShapeController.Remove(shapeMenuThumb.id);
            ShapeController.Remove(shapeMenuBackground.id);
            ShapeController.Remove(shapeMenuText.id);
        }


        private bool visible;
        public virtual bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                {
                    visible = value;

                    shapeMenuBackground.visible = value;
                    ShapeController.SetRect(shapeMenuBackground);

                    shapeMenuThumb.visible = value;
                    ShapeController.SetImage(shapeMenuThumb);

                    shapeMenuText.visible = value;
                    ShapeController.SetText(shapeMenuText);
                }
            }
        }
    }
}
