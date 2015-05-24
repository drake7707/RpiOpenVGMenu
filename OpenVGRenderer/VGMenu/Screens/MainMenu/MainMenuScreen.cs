using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.MainMenu.Menu;
using VGMenu.Screens.Menu;
using VGMenu.Screens.SeriesDetails;
using VGMenu.Screens.Details;

namespace VGMenu.Screens.MainMenu
{
    class MainMenuScreen : Screen
    {

        private VGMenu.ShapeController.SHAPE_IMAGE backgroundShape;

        private BaseMenuControl mainMenu;
        private Control submnu;

        private IPlayer player;

        public enum MainMenuEnum
        {
            Recent = 0,
            Movies = 1,
            Episodes = 2,
            Other = 3
        }

        public MainMenuScreen(ScreenManager screenManager, IPlayer player)
            : base(screenManager)
        {
            this.player = player;

            visible = true;
            backgroundShape = CreateBackground();

            mainMenu = new MainMenuControl();

            CreateSubMenu();

            ActiveControl = mainMenu;

            ShapeController.Draw();
        }

        private void CreateSubMenu()
        {
            if (submnu != null)
                submnu.Dispose();

            if (mainMenu.SelectedIndex >= 0 && mainMenu.SelectedIndex < mainMenu.Items.Count)
            {
                if (mainMenu.SelectedIndex == (int)MainMenuEnum.Movies)
                {
                    var submenu = new SubMenuControl();

                    foreach (var m in MovieManager.GetMovies().OrderBy(mv => mv.Title))
                    {
                        submenu.Items.Add(new MovieSubMenuItem(m));
                    }
                    submenu.Initialize();
                    submenu.Refresh();

                    submnu = submenu;
                }
                else if (mainMenu.SelectedIndex == (int)MainMenuEnum.Episodes)
                {
                    var submenu = new SubMenuControl();

                    foreach (var m in EpisodeManager.GetSeries().OrderBy(mv => mv.Name))
                    {
                        submenu.Items.Add(new SeriesSubMenuItem(m));
                    }
                    submenu.Initialize();
                    submenu.Refresh();

                    submnu = submenu;
                }
                else if (mainMenu.SelectedIndex == (int)MainMenuEnum.Recent)
                {
                    var recent = new RecentMenuControl();
                    submnu = recent;
                }
            }
        }

        public override void OnCommand(ConsoleKey key)
        {
            if (ActiveControl == mainMenu && key == ConsoleKey.RightArrow)
            {
                // go to submenu instead
                key = ConsoleKey.Enter;
            }
            else
                base.OnCommand(key);

            if (key == ConsoleKey.UpArrow)
            {
                if (ActiveControl == mainMenu)
                    CreateSubMenu();
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (ActiveControl == mainMenu)
                    CreateSubMenu();
            }
            if (key == ConsoleKey.LeftArrow)
            {
                if (ActiveControl == mainMenu)
                    CreateSubMenu();
            }
            else if (key == ConsoleKey.RightArrow)
            {
                //if (ActiveControl == mainMenu)
                //    CreateSubMenu();
            }
            else if (key == ConsoleKey.Enter)
            {
                if (ActiveControl == mainMenu)
                    ActiveControl = submnu;
                else if(ActiveControl is SubMenuControl)
                {
                    var submenu = (SubMenuControl)ActiveControl;
                    if (submenu.SelectedIndex >= 0 && submenu.SelectedIndex < submenu.Items.Count)
                        OpenDetailsScreen(submenu.Items[submenu.SelectedIndex]);   
                }
                else if (ActiveControl is RecentMenuControl)
                {
                    var submenu = (RecentMenuControl)ActiveControl;
                    var selectedMnuItem = submenu.SelectedMenuItem;
                    OpenDetailsScreen(selectedMnuItem);   

                }
            }
            else if (key == ConsoleKey.Backspace)
            {
                ActiveControl = mainMenu;
            }

            ShapeController.Draw();
        }
        
        

        private void OpenDetailsScreen(MenuItem menuItem)
        {
            if (menuItem is MovieSubMenuItem)
            {
                PlayableDetailsScreen detailsScreen = new PlayableDetailsScreen(screenManager, ((MovieSubMenuItem)menuItem).Movie, player);
                screenManager.OpenScreen(detailsScreen);
            }
            else if(menuItem is SeriesSubMenuItem)
            {
                SeriesDetailsScreen detailsScreen = new SeriesDetailsScreen(screenManager, ((SeriesSubMenuItem)menuItem).Series, player);
                screenManager.OpenScreen(detailsScreen);
            }
            else if (menuItem is EpisodeSubMenuItem)
            {
                PlayableDetailsScreen detailsScreen = new PlayableDetailsScreen(screenManager, ((EpisodeSubMenuItem)menuItem).Episode, player);
                screenManager.OpenScreen(detailsScreen);
            }
        }

        public override void Dispose()
        {
            ShapeController.Remove(backgroundShape.id);

            mainMenu.Dispose();
            if (submnu != null)
                submnu.Dispose();
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
                    mainMenu.Visible = value;
                    if (submnu != null)
                        submnu.Visible = value;

                    backgroundShape.visible = value;
                    ShapeController.SetImage(backgroundShape);
                }
            }
        }

        public override void Refresh()
        {
            if (mainMenu.SelectedIndex == (int)MainMenuEnum.Movies)
            {
                ((SubMenuControl)submnu).Items.Clear();
                foreach (var m in MovieManager.GetMovies().OrderBy(mv => mv.Title))
                    ((SubMenuControl)submnu).Items.Add(new MovieSubMenuItem(m));

                ((SubMenuControl)submnu).Refresh();
            }
            else if (mainMenu.SelectedIndex == (int)MainMenuEnum.Recent)
            {
                var recent = (RecentMenuControl)submnu;
                recent.Refresh();
            }

                
        }
    }
}
