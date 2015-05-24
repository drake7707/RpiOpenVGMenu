using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMenu.Screens
{
    public class ScreenManager
    {
        public ScreenManager()
        {
            IsActive = true;
            Screens = new Stack<Screen>();
        }

        public Stack<Screen> Screens { get; set; }

        public Screen ActiveScreen { get { return Screens.Count > 0 ? Screens.Peek() : null; } }

        public void OpenScreen(Screen s)
        {
            if (ActiveScreen != null)
                ActiveScreen.Visible = false;

            LoggingLib.Logging.Add("VGMenu", "Opening screen " + s.GetType().FullName, LoggingLib.Logging.LoggingEnum.Info);
            Screens.Push(s);
        }

        public void CloseScreen()
        {
            if (Screens.Count > 0) 
            {
                
                var screen = Screens.Pop();
                screen.Dispose();

                if (ActiveScreen != null)
                    ActiveScreen.Visible = true;

                if(ActiveScreen != null)
                    LoggingLib.Logging.Add("VGMenu", "Closing screen, active screen is now " + ActiveScreen.GetType().FullName, LoggingLib.Logging.LoggingEnum.Info);
                else
                    LoggingLib.Logging.Add("VGMenu", "Closing screen, no active screen", LoggingLib.Logging.LoggingEnum.Info);

                ShapeController.Draw();
            }
            else
                LoggingLib.Logging.Add("VGMenu", "Close screen called while no screens were open", LoggingLib.Logging.LoggingEnum.Warning);
        }

        internal void Dispose()
        {
            while (Screens.Count > 0)
                CloseScreen();
        }

        public void Refresh()
        {
            if (IsActive && ActiveScreen != null)
            {
                ActiveScreen.Refresh();
            }
        }

        public bool IsActive { get; set; }
    }
}
