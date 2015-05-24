using System;
namespace VGMenu.Screens.Menu
{
    public interface IShapeMenuItem
    {
        void Create(BaseMenuControl controller, int i, System.Drawing.RectangleF rect, bool highlighted);
        void Update(BaseMenuControl controller, int i, MenuItem itm, bool highlighted);
        bool Visible { get; set; }

        void Dispose();
    }
}
