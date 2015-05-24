using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu;

namespace Player
{
    public class Program
    {

        private class Player : IPlayer
        {
            public void Play(string path, string subtitlePath)
            {
                IsPlaying = true;
                Console.WriteLine("Simulating play for 10 sec");
                Console.WriteLine("Playing done");
                IsPlaying = false;
            }

            public bool IsPlaying { get; set; }

            public event PlayStateChangingHandler PlayStateChanging;


            public void Queue(string path, string subtitle)
            {

            }
        }

        private static void TestMenu()
        {
            MovieMetaDataService movieMetadataService = new MovieMetaDataService();
            EpisodeMetaDataService episodeMetadataService = new EpisodeMetaDataService();
            LibCECManager cec = new LibCECManager("./cec-client");
            //cec.Start();

            VGMenuManager menuManager = new VGMenuManager(cec, new Player(), movieMetadataService, episodeMetadataService);
            menuManager.CreateInterface();

            bool stop = false;
            while (!stop)
            {
                stop = !menuManager.CheckInput();
                System.Threading.Thread.Sleep(25);
            }

            menuManager.Dispose();

            while (true)
                System.Threading.Thread.Sleep(25);

        }
        public static void Main(string[] args)
        {
           // TestMenu();

            // ensure working directory is application path, otherwise the DllImport won't find its library
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            MovieMetaDataService movieMetadataService = new MovieMetaDataService();
            movieMetadataService.Start();

            EpisodeMetaDataService episodeMetadataService = new EpisodeMetaDataService();
            episodeMetadataService.Start();

            //while (true) { System.Threading.Thread.Sleep(100);  }

            LibCECManager cec = new LibCECManager("./cec-client");
            try
            {
                cec.Start();
            }
            catch (Exception ex)
            {
                LoggingLib.Logging.Add("Player", "Error initializing CEC: " + ex.GetType().FullName + " - " + ex.Message, LoggingLib.Logging.LoggingEnum.Error);
            }
            

            OMXPlayerManager playerManager = new OMXPlayerManager(cec, 7710);
            playerManager.Start();

            VGMenuManager menuManager = new VGMenuManager(cec, playerManager,movieMetadataService, episodeMetadataService);
            menuManager.CreateInterface();

            bool stop = false;
            while (!stop)
            {
                menuManager.CheckPendingActions();
                stop = !menuManager.CheckInput();
                System.Threading.Thread.Sleep(25);
            }

            episodeMetadataService.Stop();
            movieMetadataService.Stop();
            playerManager.StopService();
            cec.Stop();
            menuManager.Dispose();

        }
    }
}
