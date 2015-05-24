using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Configuration;
using VGMenu.Screens.Details;

namespace VGMenu.Screens.Details
{
    public class PlayableDetailsScreen : Screen
    {
        private VGMenu.ShapeController.SHAPE_IMAGE backgroundShape;
        private VGMenu.ShapeController.SHAPE_RECT backgroundClearShape;

        private DetailsMenuControl mnuOptions;

        private VGMenu.ShapeController.SHAPE_RECT shapeTitleContainer;
        private VGMenu.ShapeController.SHAPE_TEXT shapeTitle;
        private VGMenu.ShapeController.SHAPE_RECT shapeDescriptionContainer;
        private VGMenu.ShapeController.SHAPE_TEXT shapeDescription;

        private VGMenu.ShapeController.SHAPE_RECT shapeCoverContainer;
        private VGMenu.ShapeController.SHAPE_IMAGE shapeCover;
        private IPlayer player;
        private IDetailItem detailItem;

        public PlayableDetailsScreen(ScreenManager screenManager, IDetailItem detailItem, IPlayer player)
            : base(screenManager)
        {
            this.player = player;
            this.detailItem = detailItem;

            visible = true;

            if (ConfigurationManager.AppSettings["useRaw"] == "true")
            {
                // raw loads very fast so it's not necessary to load the menu in 2 steps
                CreateInterface(detailItem, true);
            }
            else
            {
                // do in 2 steps so you can read the description already while it's loading
                CreateInterface(detailItem, false);
                CreateInterface(detailItem, true);
            }

        }

        private void CreateInterface(IDetailItem detailItem, bool loadBackground)
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
                if (string.IsNullOrEmpty(detailItem.BackdropPath) || ConfigurationManager.AppSettings["useBackdrop"] != "true")
                    backgroundShape = CreateBackground();
                else
                    backgroundShape = CreateBackground(detailItem.BackdropPath);
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
            if (string.IsNullOrEmpty(detailItem.FullCoverPath))
                shapeCover.filename = "";
            else
                shapeCover.filename = detailItem.FullCoverPath;

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

                text = detailItem.Title,
                size = shapeTitleContainer.height * 0.25f,
                color = Color.White.ToShapeColor()
            };
            ShapeController.SetText(shapeTitle);

            if (shapeDescriptionContainer.id != 0)
                ShapeController.Remove(shapeDescriptionContainer.id);

            shapeDescriptionContainer = new ShapeController.SHAPE_RECT()
            {
                id = ShapeController.NewId(),
                visible = true,
                x = shapeCoverContainer.x + shapeCoverContainer.width + ThemeManager.DefaultPadding,
                y = 0.11f + ThemeManager.DefaultPadding,
                width = 0.59f - ThemeManager.DefaultPadding,
                height = 0.77f,
                backColor = Color.FromArgb(192, 64, 64, 64).ToShapeColor()
            };
            ShapeController.SetRect(shapeDescriptionContainer);

            if (shapeDescription.id != 0)
                ShapeController.Remove(shapeDescription.id);

            shapeDescription = new ShapeController.SHAPE_TEXT()
            {
                id = ShapeController.NewId(),
                visible = true,
                x = shapeCoverContainer.x + ThemeManager.DefaultPadding + shapeCoverContainer.width + ThemeManager.DefaultPadding,
                y = 0.11f + ThemeManager.DefaultPadding + ThemeManager.DefaultPadding,

                width = 0.58f - ThemeManager.DefaultPadding,
                height = 0.77f,

                text = detailItem.Description,
                size = shapeDescriptionContainer.height / 30, // at least 30 lines of description
                color = Color.White.ToShapeColor()
            };
            ShapeController.SetText(shapeDescription);


            if (mnuOptions != null)
                mnuOptions.Dispose();
            mnuOptions = new DetailsMenuControl(detailItem, new PointF(shapeDescription.x, shapeDescription.y + shapeDescription.height + ThemeManager.DefaultPadding), new SizeF(shapeDescription.width, 0.11f));


            ActiveControl = mnuOptions;

            ShapeController.Draw();
        }


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
                if (mnuOptions.SelectedIndex == 0) // subtitles
                {
                    mnuOptions.ToggleSubtitles();
                }
                else if (mnuOptions.SelectedIndex == 1) // play
                {
                    DoPlay();
                }
            }
            else if (key == ConsoleKey.Backspace)
            {
                screenManager.CloseScreen();
            }

            ShapeController.Draw();
        }

        private void DoPlay()
        {
            var playableFiles = detailItem.PlayableFiles.ToArray();

            if (playableFiles.Length == 0)
            {
                LoggingLib.Logging.Add("VGMenu", "Nothing to playback for " + detailItem.Title + "", LoggingLib.Logging.LoggingEnum.Warning);
            }
            else
            {
                // play
                LoggingLib.Logging.Add("VGMenu", "Starting playback of " + detailItem.Title + "", LoggingLib.Logging.LoggingEnum.Info);
                // todo multipart -> play first, enqueue rest

                foreach (var mp in playableFiles)
                {

                    string subtitlePath = "";
                    if (mnuOptions.UseSubtitles)
                    {
                        var sub = mp.Subtitles.Where(s => s.Language == mnuOptions.SelectedLanguage).FirstOrDefault();
                        if (sub != null)
                            subtitlePath = sub.SubtitlePath;
                    }
                    if (mp == playableFiles.First())
                        player.Play(mp.VideoPath, subtitlePath);
                    else
                        player.Queue(mp.VideoPath, subtitlePath);
                }
            }
        }



        public override void Dispose()
        {
            if (backgroundShape.id != 0)
                ShapeController.Remove(backgroundShape.id);

            if (backgroundClearShape.id != 0)
                ShapeController.Remove(backgroundClearShape.id);

            ShapeController.Remove(shapeCover.id);
            ShapeController.Remove(shapeCoverContainer.id);
            ShapeController.Remove(shapeDescriptionContainer.id);
            ShapeController.Remove(shapeTitleContainer.id);
            ShapeController.Remove(shapeDescription.id);
            ShapeController.Remove(shapeTitle.id);
            mnuOptions.Dispose();
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

                    shapeDescriptionContainer.visible = value;
                    ShapeController.SetRect(shapeDescriptionContainer);
                    shapeDescription.visible = value;
                    ShapeController.SetText(shapeDescription);

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
