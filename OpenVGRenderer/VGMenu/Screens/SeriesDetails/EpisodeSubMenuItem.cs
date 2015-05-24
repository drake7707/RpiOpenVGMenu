using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;

namespace VGMenu.Screens.SeriesDetails
{
    public class EpisodeSubMenuItem : MenuItem
    {
        public EpisodeSubMenuItem(Episode m)
        {
            this.Episode = m;
        }

        public Episode Episode { get; private set; }

        public override string Text
        {
            get
            {
                if (string.IsNullOrEmpty(Episode.Name))
                    return System.IO.Path.GetFileName(Episode.EpisodePath);
                else
                    return Episode.Number.ToString("00") + " - " + Episode.Name;
            }
            set
            {
            }
        }

        public override string Icon
        {
            get
            {
                return Episode.Season.ThumbCoverPath;
            }
        }

        public override string IconTextOverlay
        {
            get
            {
                return Episode.Number == -1 ? "?" : Episode.Number.ToString("00");
            }
        }
    }
}
