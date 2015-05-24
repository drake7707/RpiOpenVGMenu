using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMenu.Screens.Menu
{
    public class MenuItem
    {
        public virtual string Text { get; set; }

        public virtual string Icon { get; set; }

        public virtual string IconTextOverlay { get; set; }
    }

}
