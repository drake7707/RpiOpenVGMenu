using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Configuration;
using VGMenu.Screens.Menu;
using VGMenu.Screens.Details;

namespace VGMenu.Screens.SeriesDetails
{
    public class SeriesDetailsScreen : Screen
    {
        private VGMenu.ShapeController.SHAPE_IMAGE backgroundShape;
        private VGMenu.ShapeController.SHAPE_RECT backgroundClearShape;

        private BaseMenuControl mnuItems;

        private VGMenu.ShapeController.SHAPE_RECT shapeTitleContainer;
        private VGMenu.ShapeController.SHAPE_TEXT shapeTitle;
        //private VGMenu.ShapeController.SHAPE_RECT shapeDescriptionContainer;
        //private VGMenu.ShapeController.SHAPE_TEXT shapeDescription;

        private VGMenu.ShapeController.SHAPE_RECT shapeCoverContainer;
        private VGMenu.ShapeController.SHAPE_IMAGE shapeCover;
        private IPlayer player;
        private Series series;

        public SeriesDetailsScreen(ScreenManager screenManager, Series itm, IPlayer player)
            : base(screenManager)
        {
            this.player = player;
            this.series = itm;

            visible = true;


            if (ConfigurationManager.AppSettings["useRaw"] == "true")
            {
                // raw loads very fast so it's not necessary to load the menu in 2 steps
                CreateInterface(itm, true);
            }
            else
            {
                // do in 2 steps so you can read the description already while it's loading
                CreateInterface(itm, false);
                CreateInterface(itm, true);
            }

        }

        private void CreateInterface(Series series, bool loadBackground)
        {

            if (backgroundShape.id != 0)
            {
                ShapeController.Remove(backgroundShape.id);
                backgroundShape.id = 0;
            }
            if (backgroundClearShape.id != 0)
            {
                ShapeController.Remove(backgroundClearShape.id);
                backgroundClearShape.id = 0; // clear shape
            }

            if (loadBackground)
            {
                if (string.IsNullOrEmpty(series.BackdropPath) || ConfigurationManager.AppSettings["useBackdrop"] != "true")
                    backgroundShape = CreateBackground();
                else
                    backgroundShape = CreateBackground(series.BackdropPath);
            }
            else
            {
                backgroundClearShape = new ShapeController.SHAPE_RECT()
                {
                    backColor = Color.White.ToShapeColor(),
                    id = ShapeController.NewId(),
                    visible = true,
                    x = 0,
                    y = 0,
                    width = 1,
                    height = 1
                };
                ShapeController.SetRect(backgroundClearShape);
            }

            if (shapeCoverContainer.id != 0)
                ShapeController.Remove(shapeCoverContainer.id);
            shapeCoverContainer = new ShapeController.SHAPE_RECT()
            {
                id = ShapeController.NewId(),
                visible = true,
                x = ThemeManager.DefaultPadding,
                y = ThemeManager.DefaultPadding,
                width = 0.4f,
                height = 1f - 2 * ThemeManager.DefaultPadding,
                backColor = Color.FromArgb(192, 64, 64, 64).ToShapeColor()
            };
            ShapeController.SetRect(shapeCoverContainer);


            // create cover first, in case the cover is the same, so it doesn't get loaded twice
            VGMenu.ShapeController.SHAPE_IMAGE oldCover = shapeCover;
            shapeCover = new ShapeController.SHAPE_IMAGE()
            {
                id = ShapeController.NewId(),
                visible = true,
                x = shapeCoverContainer.x + ThemeManager.DefaultPadding,
                y = shapeCoverContainer.y + ThemeManager.DefaultPadding,
                width = shapeCoverContainer.width - 2 * ThemeManager.DefaultPadding,
                height = shapeCoverContainer.height - 2 * ThemeManager.DefaultPadding,
            };
            if (string.IsNullOrEmpty(series.FullCoverPath))
                shapeCover.filename = "";
            else
                shapeCover.filename = series.FullCoverPath;

            if (oldCover.id != 0)
                ShapeController.Remove(oldCover.id);

            ShapeController.SetImage(shapeCover);

            if (shapeTitleContainer.id != 0)
                ShapeController.Remove(shapeTitleContainer.id);
            shapeTitleContainer = new ShapeController.SHAPE_RECT()
            {
                id = ShapeController.NewId(),
                visible = true,
                x = shapeCoverContainer.x + shapeCoverContainer.width + ThemeManager.DefaultPadding,
                y = ThemeManager.DefaultPadding,
                width = 0.59f - ThemeManager.DefaultPadding,
                height = 0.10f,
                backColor = Color.FromArgb(192, 64, 64, 64).ToShapeColor()
            };
            ShapeController.SetRect(shapeTitleContainer);

            if (shapeTitle.id != 0)
                ShapeController.Remove(shapeTitle.id);
            shapeTitle = new ShapeController.SHAPE_TEXT()
            {
                id = ShapeController.NewId(),
                visible = true,
                x = shapeCoverContainer.x + ThemeManager.DefaultPadding + shapeCoverContainer.width + ThemeManager.DefaultPadding,
                y = ThemeManager.DefaultPadding + shapeTitleContainer.height / 2,

                width = 0.59f - ThemeManager.DefaultPadding,
                height = 0.10f,

                text = series.Name,
                size = shapeTitleContainer.height * 0.25f,
                color = Color.White.ToShapeColor()
            };
            ShapeController.SetText(shapeTitle);

            if (mnuItems != null)
                mnuItems.Dispose();

            mnuItems = new SeriesMenuControl(new PointF(shapeCoverContainer.x + shapeCoverContainer.width + ThemeManager.DefaultPadding, shapeTitleContainer.y + shapeTitle.height + ThemeManager.DefaultPadding),
                                     new SizeF(0.59f - ThemeManager.DefaultPadding - 0.04f, 1 - ThemeManager.DefaultPadding - (shapeTitleContainer.y + shapeTitle.height + ThemeManager.DefaultPadding)));
            if (series.Seasons.Count == 1)
            {
                var season = series.Seasons.First();
                mnuItems.Items.AddRange(season.Episodes.OrderByDescending(e => e.Number).Select(e => new EpisodeSubMenuItem(e)));

                // update cover
                shapeCover.filename = season.FullCoverPath;
                ShapeController.SetImage(shapeCover);

                if (season.Number != -1)
                    shapeTitle.text = series.Name + " - " + "Season " + season.Number;
                else
                    shapeTitle.text = series.Name + " - " + "Season unknown";
                ShapeController.SetText(shapeTitle);
            }
            else
            {
                mnuItems.Items.AddRange(series.Seasons.OrderByDescending(s => s.Number).Select(s => new SeasonSubMenuItem(s)));
            }

            mnuItems.Initialize();

            ActiveControl = mnuItems;

            ShapeController.Draw();
        }

        private int oldSeasonIndex;
        public override void OnCommand(ConsoleKey key)
        {
            base.OnCommand(key);
            if (key == ConsoleKey.UpArrow)
            {

            }
            else if (key == ConsoleKey.DownArrow)
            {

            }
            else if (key == ConsoleKey.Enter)
            {
                if (mnuItems.SelectedIndex >= 0 && mnuItems.SelectedIndex < mnuItems.Items.Count)
                {
                    var itm = mnuItems.Items[mnuItems.SelectedIndex];
                    if (itm is SeasonSubMenuItem)
                    {
                        oldSeasonIndex = mnuItems.SelectedIndex;
                        mnuItems.Items.Clear();
                        mnuItems.Items.AddRange(((SeasonSubMenuItem)itm).Season.Episodes.OrderByDescending(e => e.Number).Select(e => new EpisodeSubMenuItem(e)));

                        if (mnuItems.Items.Count > 0)
                            mnuItems.SelectedIndex = 0;
                        mnuItems.Refresh();

                        // update cover
                        shapeCover.filename = ((SeasonSubMenuItem)itm).Season.FullCoverPath;
                        ShapeController.SetImage(shapeCover);

                        if (((SeasonSubMenuItem)itm).Season.Number != -1)
                            shapeTitle.text = series.Name + " - " + "Season " + ((SeasonSubMenuItem)itm).Season.Number;
                        else
                            shapeTitle.text = series.Name + " - " + "Season unknown";
                        
                        ShapeController.SetText(shapeTitle);
                    }
                    else if (itm is EpisodeSubMenuItem)
                    {
                        // open episode screen
                        PlayableDetailsScreen screen = new PlayableDetailsScreen(screenManager, ((EpisodeSubMenuItem)itm).Episode, player);
                        screenManager.OpenScreen(screen);
                    }
                }

            }
            else if (key == ConsoleKey.Backspace)
            {
                if (mnuItems.SelectedIndex >= 0 && mnuItems.SelectedIndex < mnuItems.Items.Count)
                {
                    var itm = mnuItems.Items[mnuItems.SelectedIndex];
                    if (itm is SeasonSubMenuItem)
                    {
                        screenManager.CloseScreen();
                    }
                    else if (itm is EpisodeSubMenuItem)
                    {
                        mnuItems.Items.Clear();
                        mnuItems.Items.AddRange(series.Seasons.OrderByDescending(s => s.Number).Select(s => new SeasonSubMenuItem(s)));
                        if (oldSeasonIndex >= 0 && oldSeasonIndex < mnuItems.Items.Count)
                            mnuItems.SelectedIndex = oldSeasonIndex;
                        else if (mnuItems.Items.Count > 0)
                            mnuItems.SelectedIndex = 0;
                        mnuItems.Refresh();

                        // update cover back to series
                        shapeCover.filename = series.FullCoverPath;
                        ShapeController.SetImage(shapeCover);

                        shapeTitle.text = series.Name;
                        ShapeController.SetText(shapeTitle);
                    }

                }

            }

            ShapeController.Draw();
        }

        private void ShowSeasonDetails()
        {

        }

        public override void Dispose()
        {
            if (backgroundShape.id != 0)
                ShapeController.Remove(backgroundShape.id);

            if (backgroundClearShape.id != 0)
                ShapeController.Remove(backgroundClearShape.id);

            ShapeController.Remove(shapeCover.id);
            ShapeController.Remove(shapeCoverContainer.id);
            //ShapeController.Remove(shapeDescriptionContainer.id);
            ShapeController.Remove(shapeTitleContainer.id);
            //ShapeController.Remove(shapeDescription.id);
            ShapeController.Remove(shapeTitle.id);
            mnuItems.Dispose();
        }

        private bool visible;
        public override bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                {
                    visible = value;

                    if (backgroundShape.id != 0)
                    {
                        backgroundShape.visible = value;
                        ShapeController.SetImage(backgroundShape);
                    }

                    if (backgroundClearShape.id != 0)
                    {
                        backgroundClearShape.visible = value;
                        ShapeController.SetRect(backgroundClearShape);
                    }

                    shapeCover.visible = value;
                    ShapeController.SetImage(shapeCover);
                    shapeCoverContainer.visible = value;
                    ShapeController.SetRect(shapeCoverContainer);

                    //shapeDescriptionContainer.visible = value;
                    //ShapeController.SetRect(shapeDescriptionContainer);
                    //shapeDescription.visible = value;
                    //ShapeController.SetText(shapeDescription);

                    shapeTitleContainer.visible = value;
                    ShapeController.SetRect(shapeTitleContainer);
                    shapeTitle.visible = value;
                    ShapeController.SetText(shapeTitle);
                }
            }
        }

        public override void Refresh()
        {

        }
    }
}
