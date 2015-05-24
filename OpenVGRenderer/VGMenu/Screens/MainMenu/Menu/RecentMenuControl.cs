using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using VGMenu.Screens.Menu;
using VGMenu.Screens.SeriesDetails;

namespace VGMenu.Screens.MainMenu.Menu
{
    public class RecentMenuControl : Control
    {
        private ShapeRecentMenuTitle shapeTitleMovies;
        private ThumbMenuControl recentMoviesMenu;

        private ShapeRecentMenuTitle shapeSelectionDescription;

        private ShapeRecentMenuTitle shapeTitleEpisodes;
        private ThumbMenuControl recentEpisodesMenu;

        public PointF Location { get; private set; }
        public SizeF Size { get; set; }


        private List<ThumbMenuControl> controls = new List<ThumbMenuControl>();
        private int currentActiveControlIndex = 0;

        public RecentMenuControl()
        {
            this.Location = new PointF(0.210f, 0.1f);
            this.Size = new SizeF(0.75f, 0.9f);

            // create shape for movies title
            // create menu for movies thumbs

            // create shape for episodes title
            // create menu for episodes thumbs

            /*
             * [ RECENT MOVIES ]
             * [ ] [ ] [ ] [ ] [ ]
             * 
             * [ RECENT EPISODES ]
             * [ ] [ ] [ ] [ ] [ ]
             * 
             */

            float top = this.Location.Y;

            shapeTitleMovies = new ShapeRecentMenuTitle(new System.Drawing.RectangleF(this.Location.X, top, this.Size.Width, 0.1f), "RECENT MOVIES", ThemeManager.MenuItemBackground, ThemeManager.MenuItemForeground);
            top += shapeTitleMovies.Bounds.Height + ThemeManager.DefaultPadding;
            recentMoviesMenu = new ThumbMenuControl(new PointF(this.Location.X, top), new SizeF(this.Size.Width, 0.2f + 0.01f));
            top += recentMoviesMenu.Size.Height + ThemeManager.DefaultPadding;

            shapeTitleEpisodes = new ShapeRecentMenuTitle(new System.Drawing.RectangleF(this.Location.X, top, this.Size.Width, 0.1f), "RECENT EPISODES", ThemeManager.MenuItemBackground, ThemeManager.MenuItemForeground);
            top += shapeTitleEpisodes.Bounds.Height + ThemeManager.DefaultPadding;

            recentEpisodesMenu = new ThumbMenuControl(new PointF(this.Location.X, top), new SizeF(this.Size.Width, 0.2f + 0.01f));
            top += recentMoviesMenu.Size.Height + ThemeManager.DefaultPadding;

            shapeSelectionDescription = new ShapeRecentMenuTitle(new System.Drawing.RectangleF(this.Location.X, 0.9f - ThemeManager.DefaultPadding, this.Size.Width, 0.1f), "", ThemeManager.MenuItemBackground, ThemeManager.MenuItemForeground);
            top += shapeSelectionDescription.Bounds.Height + ThemeManager.DefaultPadding;


            foreach (var m in MovieManager.GetMovies().OrderByDescending(mv => mv.DateModified).Take(7))
                recentMoviesMenu.Items.Add(new MovieSubMenuItem(m));

            recentMoviesMenu.Initialize();
            recentMoviesMenu.Refresh();

            foreach (var m in EpisodeManager.GetSeries().SelectMany(s => s.Seasons).SelectMany(s => s.Episodes).OrderByDescending(mv => mv.DateModified).Take(7))
                recentEpisodesMenu.Items.Add(new EpisodeSubMenuItem(m));

            recentEpisodesMenu.Initialize();
            recentEpisodesMenu.Refresh();

            controls.Add(recentMoviesMenu);
            controls.Add(recentEpisodesMenu);

        }

        public override void Refresh()
        {
            recentMoviesMenu.Items.Clear();
            foreach (var m in MovieManager.GetMovies().OrderByDescending(mv => mv.DateModified).Take(7))
                recentMoviesMenu.Items.Add(new MovieSubMenuItem(m));

            recentEpisodesMenu.Items.Clear();
            foreach (var m in EpisodeManager.GetSeries().SelectMany(s => s.Seasons).SelectMany(s => s.Episodes).OrderByDescending(mv => mv.DateModified).Take(7))
                recentEpisodesMenu.Items.Add(new EpisodeSubMenuItem(m));

            recentMoviesMenu.Refresh();

        }

        public override void Dispose()
        {
            shapeTitleMovies.Dispose();
            shapeTitleEpisodes.Dispose();

            recentMoviesMenu.Dispose();
            recentEpisodesMenu.Dispose();
        }



        public MenuItem SelectedMenuItem
        {
            get
            {
                var idx = controls[currentActiveControlIndex].SelectedIndex;
                if (idx >= 0 && idx < controls[currentActiveControlIndex].Items.Count)
                    return controls[currentActiveControlIndex].Items[idx];
                else
                    return null;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            controls[currentActiveControlIndex].OnEnter();

            var curItem = SelectedMenuItem;
            shapeSelectionDescription.UpdateText(curItem == null ? "" : curItem.Text);
        }

        public override void OnLeave()
        {
            base.OnLeave();
            controls[currentActiveControlIndex].OnLeave();

            shapeSelectionDescription.UpdateText("");
        }

        public override void OnCommand(ConsoleKey key)
        {
            if (key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
            {
                var oldItem = SelectedMenuItem;
                controls[currentActiveControlIndex].OnCommand(key);

                var curItem = SelectedMenuItem;
                if (oldItem != curItem)
                    shapeSelectionDescription.UpdateText(curItem == null ? "" : curItem.Text);
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (currentActiveControlIndex + 1 >= controls.Count)
                    return;

                var oldIdx = controls[currentActiveControlIndex].SelectedIndex;
                controls[currentActiveControlIndex].SelectedIndex = -1;

                currentActiveControlIndex++;

                if (oldIdx >= 0 && oldIdx < controls[currentActiveControlIndex].Items.Count)
                    controls[currentActiveControlIndex].SelectedIndex = oldIdx;
                else
                    controls[currentActiveControlIndex].SelectedIndex = 0;

                var curItem = SelectedMenuItem;
                shapeSelectionDescription.UpdateText(curItem == null ? "" : curItem.Text);
            }
            else if (key == ConsoleKey.UpArrow)
            {
                if (currentActiveControlIndex - 1 < 0)
                    return;


                var oldIdx = controls[currentActiveControlIndex].SelectedIndex;
                controls[currentActiveControlIndex].SelectedIndex = -1;

                currentActiveControlIndex--;

                if (oldIdx >= 0 && oldIdx < controls[currentActiveControlIndex].Items.Count)
                    controls[currentActiveControlIndex].SelectedIndex = oldIdx;
                else
                    controls[currentActiveControlIndex].SelectedIndex = 0;

                var curItem = SelectedMenuItem;
                shapeSelectionDescription.UpdateText(curItem == null ? "" : curItem.Text);
            }
            else
                base.OnCommand(key);
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
                if (base.Visible != value)
                {
                    shapeTitleEpisodes.Visible = value;
                    shapeTitleMovies.Visible = value;
                    recentEpisodesMenu.Visible = value;
                    recentMoviesMenu.Visible = value;
                }
            }
        }
    }
}
