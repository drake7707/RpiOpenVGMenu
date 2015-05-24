using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;

namespace VGMenu.Screens
{
    public class ShapeSubMenuItem : ShapeMenuItem
    {

        private ShapeController.SHAPE_IMAGE MenuCover;

        public override void Create(BaseMenuControl controller, int i, System.Drawing.RectangleF rect, bool highlighted)
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
                y = rect.Y + rect.Height / 2 - rect.Height * 0.15f / 2,

                height = rect.Height - 2 * rect.Height * 0.05f,

                color = controller.MenuTextColor,
                text = "",
                size = rect.Height * 0.15f,
                visible = true
            };

            MenuCover = new ShapeController.SHAPE_IMAGE()
            {
                id = ShapeController.NewId(),
                x = rect.X + ThemeManager.DefaultPadding,
                y = rect.Y + rect.Height * 0.03f,
                height = rect.Height - 2 * ThemeManager.DefaultPadding,
                filename = "",
                visible = true
            };
            MenuCover.width = 0.10f;

            MenuText.x = rect.X + ThemeManager.DefaultPadding + MenuCover.width + ThemeManager.DefaultPadding;
            MenuText.width = rect.X + rect.Width - MenuText.x - ThemeManager.DefaultPadding;
            ShapeController.SetRect(MenuBackground);
            ShapeController.SetText(MenuText);
            ShapeController.SetImage(MenuCover);
        }

        public override void Update(BaseMenuControl controller, int i, MenuItem itm, bool highlighted)
        {
            base.Update(controller, i, itm, highlighted);

            if (itm != null)
            {
                bool hasChanged = false;
                if (MenuCover.filename != itm.Icon)
                {
                    MenuCover.filename = itm.Icon;
                    hasChanged = true;
                }

                if (MenuCover.visible != true)
                {
                    MenuCover.visible = true;
                    hasChanged = true;
                }

                if (hasChanged)
                    ShapeController.SetImage(MenuCover);
            }
            else
            {
                if (MenuCover.visible != false)
                {
                    MenuCover.visible = false;
                    ShapeController.SetImage(MenuCover);
                }
            }
        }

        public override void Dispose()
        {
            ShapeController.Remove(MenuCover.id);

            base.Dispose();
        }

    }
}
