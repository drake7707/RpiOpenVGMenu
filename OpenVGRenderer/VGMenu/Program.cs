using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace VGMenu
{







    class Program
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
        static void Main(string[] args)
        {

            if (args.Length > 0 && args[1] == "-convertdump")
            {
                ConvertFrameDump();
                return;
            }

              MovieMetaDataService movieMetadataService = new MovieMetaDataService();
              EpisodeMetaDataService episodeMetadataService = new EpisodeMetaDataService();
            //    movieMetadataService.Start();

            LibCECManager cec = new LibCECManager("./cec-client");
            //cec.Start();


            VGMenuManager menuManager = new VGMenuManager(cec, new Player(),movieMetadataService, episodeMetadataService);
            menuManager.CreateInterface();

            bool stop = false;
            while (!stop)
            {
                stop = !menuManager.CheckInput();
                System.Threading.Thread.Sleep(25);
            }

            menuManager.Dispose();
            
            //tmrKeyRepeat = new Timer() { Interval = 200 };
            //tmrKeyRepeat.Tick+=new EventHandler(tmrKeyRepeat_Tick);


           
            Console.WriteLine("SHAPE_IMAGE size: " + Marshal.SizeOf(typeof(VGMenu.ShapeController.SHAPE_IMAGE)));

           
        }

        //static void tmrKeyRepeat_Tick(object sender, EventArgs e)
        //{
        //    Action a = (Action)tmrKeyRepeat.Tag;
        //    a();
        //}

        private static void ConvertFrameDump()
        {
            var bytes = System.IO.File.ReadAllBytes("frame.raw");

            Bitmap bmp = new Bitmap(1920, 1080);

            int pos = 0;
            for (int j = 0; j < 1080; j++)
            {
                for (int i = 0; i < 1920; i++)
                {
                    bmp.SetPixel(i, 1079 - j, Color.FromArgb(bytes[pos + 3], bytes[pos], bytes[pos + 1], bytes[pos + 2]));
                    pos += 4;
                }
            }
            bmp.Save("frame.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
