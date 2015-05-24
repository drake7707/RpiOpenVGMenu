using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens;
using VGMenu.Screens.MainMenu;
using System.Threading;
using System.Threading.Tasks;

namespace VGMenu
{
    public class VGMenuManager : IDisposable
    {
        private LibCECManager cecManager;
        private Queue<ConsoleKey> keyQueue = new Queue<ConsoleKey>();
        private Queue<Action> actionQueue = new Queue<Action>();

        private ScreenManager screenManager;
        private IPlayer player;
        

        public VGMenuManager(LibCECManager cecManager, IPlayer player, MovieMetaDataService movieMetadataService, EpisodeMetaDataService episodeMetadataService)
        {
            this.player = player;
            this.cecManager = cecManager;
            cecManager.KeyPressed += new LibCECManager.KeyHandler(cec_KeyPressed);
            cecManager.KeyReleased += new LibCECManager.KeyHandler(cec_KeyReleased);
            player.PlayStateChanging += new PlayStateChangingHandler(player_PlayStateChanging);

            movieMetadataService.NewMetadataFetched += new EventHandler(movieMetadataService_NewMetadataFetched);
            episodeMetadataService.NewMetadataFetched += new EventHandler(episodeMetadataService_NewMetadataFetched);
        }

        void episodeMetadataService_NewMetadataFetched(object sender, EventArgs e)
        {
            EpisodeManager.RefreshCache();
            EnqueueAction(() =>
            {
                screenManager.Refresh();
            });
        }

        void movieMetadataService_NewMetadataFetched(object sender, EventArgs e)
        {
            MovieManager.RefreshCache();
            EnqueueAction(() =>
            {
                screenManager.Refresh();
            });
            
        }

     
        void player_PlayStateChanging(IPlayer sender, bool oldState, bool newState)
        {
            EnqueueAction(() =>
            {
                if (!oldState && newState) // starting to play
                {
                    screenManager.IsActive = false;
                    // close context
                    LoggingLib.Logging.Add("VGMenu", "Closing VG context", LoggingLib.Logging.LoggingEnum.Info);
                    ShapeController.CloseContext();
                }

                else if (oldState && !newState) // stop playing
                {
                    // open context
                    LoggingLib.Logging.Add("VGMenu", "Reestablishing VG context", LoggingLib.Logging.LoggingEnum.Info);
                    ShapeController.OpenContext();
                    screenManager.IsActive = true;

                    ShapeController.Draw();
                }
            });
        }

        public void CreateInterface()
        {
            ShapeController.Initialize(new VGMenu.ShapeController.OVERSCAN()
            {
                // not necessary anymore with overscan_scale=1
                //paddingLeft = 50,
                //paddingRight = 50,
                //paddingBottom = 25,
                //paddingTop = 25
            });
            screenManager = new ScreenManager();

            MainMenuScreen mainMenuScreen = new MainMenuScreen(screenManager, player);
            screenManager.OpenScreen(mainMenuScreen);
        }

        private void EnqueueAction(Action a)
        {
            lock (actionQueue)
                actionQueue.Enqueue(a);
        }

        public void CheckPendingActions()
        {
            Action a = null;
            lock (actionQueue)
            {
                if (actionQueue.Count > 0)
                {
                    a = actionQueue.Dequeue();
                }
            }

            if (a != null)
            {
                try
                {
                    a();
                }
                catch (Exception ex)
                {
                    LoggingLib.Logging.Add("VGMenu", "Error executing pending action: " + ex.GetType().FullName + " - " + ex.Message + Environment.NewLine + ex.StackTrace, LoggingLib.Logging.LoggingEnum.Error);
                }
            }
        }

        public bool CheckInput()
        {
            CheckConsoleKeys();

            bool hasKey = false;
            ConsoleKey key = ConsoleKey.NoName;
            lock (keyQueue)
            {
                if (keyQueue.Count > 0)
                {
                    key = keyQueue.Dequeue();
                    hasKey = true;
                }
            }

            if (hasKey)
            {
                if (key == ConsoleKey.Q)
                    return false;
                else
                {
                    try
                    {
                        screenManager.ActiveScreen.OnCommand(key);
                    }
                    catch (Exception ex)
                    {
                        LoggingLib.Logging.Add("VGMenu", "Error sending key '" + key.ToString() + "' to active screen '" + screenManager.ActiveScreen.GetType().Name + ": " + ex.GetType().FullName + " - " + ex.Message + Environment.NewLine + ex.StackTrace, LoggingLib.Logging.LoggingEnum.Error);
                    }
                }
            }
            return true;
        }

        private void CheckConsoleKeys()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey().Key;
                lock (keyQueue) keyQueue.Enqueue(key);
            }
        }

        void cec_KeyReleased(string key, byte code)
        {

        }

        void cec_KeyPressed(string key, byte code)
        {
            if (screenManager.IsActive)
            {
                var ckey = GetKeyFromCEC(key, code);
                lock (keyQueue) keyQueue.Enqueue(ckey);
            }
        }

        private static ConsoleKey GetKeyFromCEC(string key, byte code)
        {
            if (key == "up")
                return ConsoleKey.UpArrow;
            else if (key == "down")
                return ConsoleKey.DownArrow;
            else if (key == "left")
                return ConsoleKey.LeftArrow;
            else if (key == "right")
                return ConsoleKey.RightArrow;
            else if (key == "select")
                return ConsoleKey.Enter;
            else if (key == "clear")
                return ConsoleKey.Backspace;

            return ConsoleKey.NoName;
        }


        public void Dispose()
        {
            cecManager.KeyPressed -= new LibCECManager.KeyHandler(cec_KeyPressed);
            cecManager.KeyReleased -= new LibCECManager.KeyHandler(cec_KeyReleased);

            screenManager.Dispose();

            ShapeController.Destroy();
        }
    }
}
