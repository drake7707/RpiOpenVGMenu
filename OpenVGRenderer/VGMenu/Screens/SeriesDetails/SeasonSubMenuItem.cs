using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;

namespace VGMenu.Screens.SeriesDetails
{
    public class SeasonSubMenuItem : MenuItem
    {
        public SeasonSubMenuItem(Season m)
        {
            this.Season = m;
        }

        public Season Season { get; private set; }

        public override string Text
        {
            get
            {
                if(Season.Number == -1)
                    return "Unknown season";
                else
                    return "Season " + Season.Number;
            }
            set
            {
            }
        }

        public override string Icon
        {
            get
            {
                return Season.ThumbCoverPath;
            }
        }
    }
}
