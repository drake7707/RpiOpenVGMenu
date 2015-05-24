using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;

namespace VGMenu.Screens.MainMenu.Menu
{
    public class MovieSubMenuItem : MenuItem
    {
        public MovieSubMenuItem(Movie m)
        {
            this.Movie = m;
        }

        public Movie Movie { get; private set; }

        public override string Text
        {
            get
            {
                return Movie.Title;
            }
            set
            {
            }
        }

        public override string Icon
        {
            get
            {
                return Movie.ThumbCoverPath;
            }
        }
    }
}
