using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;

namespace VGMenu.Screens.MainMenu.Menu
{
    public class SeriesSubMenuItem : MenuItem
    {
        public SeriesSubMenuItem(Series s)
        {
            this.Series = s;
        }

        public Series Series { get; private set; }

        public override string Text
        {
            get
            {
                return Series.Name;
            }
            set
            {
            }
        }

        public override string Icon
        {
            get
            {
                return Series.ThumbCoverPath;
            }
        }
    }
}
