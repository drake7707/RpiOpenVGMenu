using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMenu.Screens
{
    public abstract class Screen : IDisposable
    {
        protected ScreenManager screenManager;
        public Screen(ScreenManager mgr)
        {
            this.screenManager = mgr;
        }

        protected static VGMenu.ShapeController.SHAPE_IMAGE CreateBackground(string imagePath = "img/background.jpg")
        {
            var backgroundShape = new ShapeController.SHAPE_IMAGE()
            {
                id = ShapeController.NewId(),
                x = 0f,
                y = 0f,
                width = 1f,
                height = 1f,
                filename = imagePath,
                visible = true
            };
            ShapeController.SetImage(backgroundShape);
            return backgroundShape;
        }

        public virtual void OnCommand(ConsoleKey key)
        {
            if (key == ConsoleKey.R)
                Refresh();
            else
            {
                if (ActiveControl != null)
                    ActiveControl.OnCommand(key);
            }
        }


        private Control activeControl;
        public Control ActiveControl
        {
            get { return activeControl; }
            set
            {
                if (activeControl != value)
                {
                    if (activeControl != null)
                        activeControl.OnLeave();

                    activeControl = value;

                    if (value != null)
                        activeControl.OnEnter();
                }
            }
        }


        public abstract void Dispose();

        public abstract void Refresh();

        public abstract bool Visible { get; set; }

    }
}
