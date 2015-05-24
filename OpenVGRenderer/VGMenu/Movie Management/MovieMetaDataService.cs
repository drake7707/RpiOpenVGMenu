using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Drawing;
using System.Xml.Serialization;
using System.Diagnostics;

namespace VGMenu
{
    public class MovieMetaDataService : MetadataManager
    {

        public const string BACKDROP_FILENAME = "backdrop";
        public const string ORIGINAL_BACKDROP_FILENAME = "original_backdrop.jpg";

        public const string COVER_FILENAME = "cover";
        public const string COVER_THUMB_FILENAME = "coverThumb";
        public const string ORIGINAL_COVER_FILENAME = "original_cover.jpg";

        public event EventHandler NewMetadataFetched;

        public void Start()
        {
            int interval = int.Parse(ConfigurationManager.AppSettings["checkForMetadataEveryXMinutes"]);
            Thread t = new Thread(() =>
            {

                DateTime nextRun = DateTime.UtcNow;
                while (!Stopped)
                {

                    if (DateTime.UtcNow > nextRun)
                    {
                        LoggingLib.Logging.Add(Name, "Starting scan for movie metadata", LoggingLib.Logging.LoggingEnum.Info);
                        try
                        {
                            CheckMovieMetadata();
                            LoggingLib.Logging.Add(Name, "Finished checking movie metadata", LoggingLib.Logging.LoggingEnum.Info);
                        }
                        catch (Exception ex)
                        {
                            LoggingLib.Logging.Add(Name, "Unable to check for movie metadata", ex);
                        }

                        nextRun = DateTime.UtcNow.AddMinutes(interval);
                    }
                    else
                        System.Threading.Thread.Sleep(1000);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        protected virtual void OnNewMetadataFetched()
        {
            EventHandler temp = NewMetadataFetched;
            if (temp != null)
                temp(this, EventArgs.Empty);
        }

        private static Size GetConfigSize(string configKey)
        {
            string[] parts = ConfigurationManager.AppSettings[configKey].Split('x');
            return new Size(int.Parse(parts[0]), int.Parse(parts[1]));

        }

        public static MovieMetadatas ReadMovieMetadatas()
        {
            string path = ConfigurationManager.AppSettings["moviesPath"];

            MovieMetadatas metadatas;
            string metadataPath = System.IO.Path.Combine(path, "metadata.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(MovieMetadatas));

            if (System.IO.File.Exists(metadataPath))
            {
                try
                {
                    using (FileStream s = File.OpenRead(metadataPath))
                        metadatas = (MovieMetadatas)serializer.Deserialize(s);
                }
                catch (Exception ex)
                {
                    LoggingLib.Logging.Add("Movie metadata service", "Unable to deserialize episodes  metadata", ex, LoggingLib.Logging.LoggingEnum.Error);
                    metadatas = new MovieMetadatas();
                }
                
            }
            else
                metadatas = new MovieMetadatas();

            return metadatas;
        }
        private void CheckMovieMetadata()
        {
            string path = ConfigurationManager.AppSettings["moviesPath"];
            var metadatas = ReadMovieMetadatas();

            var metadataByFolderName = metadatas.Metadatas.GroupBy(m => m.FolderName).ToDictionary(g => g.Key, g => g.First());

            Size coverSize = GetConfigSize("coverSize");
            Size coverThumbSize = GetConfigSize("coverThumbSize");
            Size backdropSize = GetConfigSize("backdropSize");

            bool fetchedNewMetadata = false;
            foreach (var f in System.IO.Directory.GetDirectories(path))
            {
                var dir = new DirectoryInfo(f);
                MovieMetadata data;
                if (!metadataByFolderName.TryGetValue(dir.Name, out data))
                {
                    DownloadMovieMetadata(metadatas, metadataByFolderName, ref coverSize, ref coverThumbSize, ref backdropSize, dir);
                    fetchedNewMetadata = true;
                }
            }
            if (fetchedNewMetadata)
            {
                OnNewMetadataFetched();
                
            }
        }

        private DateTime lastMetadataDownload;
        private void DownloadMovieMetadata(MovieMetadatas metadatas, Dictionary<string, MovieMetadata> metadataByFolderName, ref Size coverSize, ref Size coverThumbSize, ref Size backdropSize, DirectoryInfo dir)
        {
            if ((DateTime.UtcNow - lastMetadataDownload).TotalSeconds < 10)
                System.Threading.Thread.Sleep(10000);
            lastMetadataDownload = DateTime.UtcNow;

            string originalBackdropPath = System.IO.Path.Combine(dir.FullName, ORIGINAL_BACKDROP_FILENAME);
            string backdropPath = GetBackdropPath(dir);
            string originalCoverPath = System.IO.Path.Combine(dir.FullName, ORIGINAL_COVER_FILENAME);
            string coverPath = GetCoverPath(dir);
            string coverThumbPath = GetCoverThumbPath(dir);


            LoggingLib.Logging.Add(Name, "Fetching metadata for movie " + dir.Name, LoggingLib.Logging.LoggingEnum.Info);

            MovieMetadata meta = new MovieMetadata();
            meta.FolderName = dir.Name;

            try
            {
                TMDbLib.Client.TMDbClient client = new TMDbLib.Client.TMDbClient(ConfigurationManager.AppSettings["themoviedbAPIKEY"]);

                string movieName = GetCleanKeywords(dir.Name);

                var results = client.SearchMovie(movieName);
                if (results.Results.Count == 0 && movieName.Contains("("))
                {
                    movieName = movieName.Substring(0, movieName.IndexOf("("));
                    results = client.SearchMovie(movieName);
                }
                if (results.Results.Count > 0)
                {
                    LoggingLib.Logging.Add(Name, results.Results.Count + " results found", LoggingLib.Logging.LoggingEnum.Info);
                    client.GetConfig();

                    var firstResult = results.Results.First();

                    LoggingLib.Logging.Add(Name, "Fetching movie details", LoggingLib.Logging.LoggingEnum.Info);
                    var movie = client.GetMovie(firstResult.Id);


                    System.Net.WebClient wclient = new System.Net.WebClient();
                    if (!string.IsNullOrEmpty(firstResult.PosterPath))
                    {
                        LoggingLib.Logging.Add(Name, "Fetching poster", LoggingLib.Logging.LoggingEnum.Info);
                        var imageUrl = client.GetImageUrl("w1000", firstResult.PosterPath);
                        wclient.DownloadFile(imageUrl, originalCoverPath);

                        using (var orgBmp = new Bitmap(originalCoverPath))
                        {
                            using (var coverBmp = new Bitmap(orgBmp, coverSize))
                                SaveImage(coverBmp, coverPath);

                            using (var coverThumbBmp = new Bitmap(orgBmp, coverThumbSize))
                                SaveImage(coverThumbBmp, coverThumbPath);
                        }
                    }
                    if (!string.IsNullOrEmpty(firstResult.BackdropPath))
                    {
                        LoggingLib.Logging.Add(Name, "Fetching backdrop", LoggingLib.Logging.LoggingEnum.Info);
                        var imageBackdropUrl = client.GetImageUrl("w1920", firstResult.BackdropPath);
                        wclient.DownloadFile(imageBackdropUrl, originalBackdropPath);

                        using (var backdropBmp = new Bitmap(originalBackdropPath))
                        {
                            using (var newBackcoverBmp = new Bitmap(backdropBmp, backdropSize))
                                SaveImage(newBackcoverBmp, backdropPath);
                        }
                    }

                    meta.Description = movie.Overview;
                    meta.Name = movie.Title;
                    meta.IMDBId = movie.ImdbId;
                    meta.Id = movie.Id;
                    meta.MetaAvailable = true;

                    LoggingLib.Logging.Add(Name, "Movie metadata fetched: id=" + meta.Id + ", title=" + movie.Title, LoggingLib.Logging.LoggingEnum.Info);

                    metadataByFolderName[dir.Name] = meta;
                    metadatas.Metadatas.Add(meta);
                }
                else
                {
                    LoggingLib.Logging.Add(Name, "Unable to fetch metadata, no results", LoggingLib.Logging.LoggingEnum.Warning);

                    meta.MetaAvailable = false;
                    metadataByFolderName[dir.Name] = meta;
                    metadatas.Metadatas.Add(meta);
                }

                LoggingLib.Logging.Add(Name, "Saving movie metadata", LoggingLib.Logging.LoggingEnum.Info);
                WriteMovieMetadatas(metadatas);

                

            }
            catch (Exception ex)
            {
                LoggingLib.Logging.Add(Name, "Error fetching metadata for movie, target IL: " + new System.Diagnostics.StackTrace(ex).GetFrame(0).GetILOffset().ToString("x2"), ex);
            }

            

            Movie m = MovieManager.GetMovieFromMetatadata(dir, meta);
            foreach (var mp in m.MoviePaths)
                SubtitleManager.DownloadSubtitles(dir, mp.VideoPath);
        }

        public static string GetBackdropPath(DirectoryInfo dir)
        {
            if (ConfigurationManager.AppSettings["useRaw"] == "true")
                return System.IO.Path.Combine(dir.FullName, BACKDROP_FILENAME) + ".raw";
            else
                return System.IO.Path.Combine(dir.FullName, BACKDROP_FILENAME) + ".jpg";
        }

        public static string GetCoverPath(DirectoryInfo dir)
        {
            if (ConfigurationManager.AppSettings["useRaw"] == "true")
                return System.IO.Path.Combine(dir.FullName, COVER_FILENAME) + ".raw";
            else
                return System.IO.Path.Combine(dir.FullName, COVER_FILENAME) + ".jpg";
        }

        public static string GetCoverThumbPath(DirectoryInfo dir)
        {
            if (ConfigurationManager.AppSettings["useRaw"] == "true")
                return System.IO.Path.Combine(dir.FullName, COVER_THUMB_FILENAME) + ".raw";
            else
                return System.IO.Path.Combine(dir.FullName, COVER_THUMB_FILENAME) + ".jpg";
        }



        private static void WriteMovieMetadatas(MovieMetadatas metadatas)
        {
            string path = ConfigurationManager.AppSettings["moviesPath"];
            string metadataPath = System.IO.Path.Combine(path, "metadata.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(MovieMetadatas));
            using (FileStream s = File.Create(metadataPath))
            {
                serializer.Serialize(s, metadatas);
            }
        }


        public void Stop()
        {
            Stopped = true;

        }


        public string Name
        {
            get { return "Movie metadata service"; }
        }

        private bool stop;
        private object stopLock = new object();
        public bool Stopped
        {
            get { lock (stopLock) return stop; }
            set { lock (stopLock) stop = value; }
        }

    }
}
