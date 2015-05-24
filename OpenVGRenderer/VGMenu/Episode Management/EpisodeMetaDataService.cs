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
    public class EpisodeMetaDataService : MetadataManager
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
                        LoggingLib.Logging.Add(Name, "Starting scan for episode metadata", LoggingLib.Logging.LoggingEnum.Info);
                        try
                        {
                            CheckEpisodeMetadata();
                            LoggingLib.Logging.Add(Name, "Finished checking episode metadata", LoggingLib.Logging.LoggingEnum.Info);
                        }
                        catch (Exception ex)
                        {
                            LoggingLib.Logging.Add(Name, "Unable to check for episode metadata", ex);
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

        public static SeriesMetadatas ReadEpisodeMetadatas()
        {
            string path = ConfigurationManager.AppSettings["episodesPath"];

            SeriesMetadatas metadatas;
            string metadataPath = System.IO.Path.Combine(path, "metadata.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(SeriesMetadatas));

            if (System.IO.File.Exists(metadataPath))
            {
                try
                {
                    using (FileStream s = File.OpenRead(metadataPath))
                        metadatas = (SeriesMetadatas)serializer.Deserialize(s);
                }
                catch (Exception ex)
                {
                    LoggingLib.Logging.Add("Episode metadata service", "Unable to deserialize episodes  metadata", ex, LoggingLib.Logging.LoggingEnum.Error);
                    metadatas = new SeriesMetadatas();
                }
            }
            else
                metadatas = new SeriesMetadatas();

            return metadatas;
        }

        private void CheckEpisodeMetadata()
        {
            string path = ConfigurationManager.AppSettings["episodesPath"];
            var metadatas = ReadEpisodeMetadatas();

            var metadataByFolderName = metadatas.Metadatas.GroupBy(m => m.FolderName).ToDictionary(g => g.Key, g => g.First());

            Size coverSize = GetConfigSize("coverSize");
            Size coverThumbSize = GetConfigSize("coverThumbSize");
            Size backdropSize = GetConfigSize("backdropSize");

            bool fetchedNewMetadata = false;
            foreach (var f in System.IO.Directory.GetDirectories(path))
            {
                var dir = new DirectoryInfo(f);
                SeriesMetadata data;
                if (!metadataByFolderName.TryGetValue(dir.Name, out data))
                {
                    data = DownloadSeriesMetadata(metadatas, metadataByFolderName, ref coverSize, ref coverThumbSize, ref backdropSize, dir);
                    fetchedNewMetadata = true;
                }

                if (data != null)
                {
                    foreach (var epFile in EpisodeManager.GetAllEpisodeFilesFromSeries(dir))
                    {
                        bool downloadedData = DownloadEpisodeMetadata(epFile, dir, data, ref coverSize, ref coverThumbSize, ref backdropSize);
                        if (downloadedData)
                            fetchedNewMetadata = true;

                    }
                }

            }
            if (fetchedNewMetadata)
            {
                WriteSeriesMetadatas(metadatas);
                OnNewMetadataFetched();
            }
        }

        private DateTime lastMetadataDownload;
        private SeriesMetadata DownloadSeriesMetadata(SeriesMetadatas metadatas, Dictionary<string, SeriesMetadata> metadataByFolderName, ref Size coverSize, ref Size coverThumbSize, ref Size backdropSize, DirectoryInfo dir)
        {
            if ((DateTime.UtcNow - lastMetadataDownload).TotalSeconds < 10)
                System.Threading.Thread.Sleep(10000);
            lastMetadataDownload = DateTime.UtcNow;

            string originalBackdropPath = System.IO.Path.Combine(dir.FullName, ORIGINAL_BACKDROP_FILENAME);
            string backdropPath = GetBackdropPath(dir);
            string originalCoverPath = System.IO.Path.Combine(dir.FullName, ORIGINAL_COVER_FILENAME);
            string coverPath = GetCoverPath(dir);
            string coverThumbPath = GetCoverThumbPath(dir);

            LoggingLib.Logging.Add(Name, "Fetching metadata for series " + dir.Name, LoggingLib.Logging.LoggingEnum.Info);

            SeriesMetadata meta = new SeriesMetadata();
            meta.FolderName = dir.Name;

            try
            {
                TMDbLib.Client.TMDbClient client = new TMDbLib.Client.TMDbClient(ConfigurationManager.AppSettings["themoviedbAPIKEY"]);

                string seriesName = GetCleanKeywords(dir.Name);


                var results = client.SearchTvShow(seriesName);
                if (results.Results.Count == 0 && seriesName.Contains("("))
                {
                    seriesName = seriesName.Substring(0, seriesName.IndexOf("("));
                    results = client.SearchTvShow(seriesName);
                }
                if (results.Results.Count > 0)
                {

                    LoggingLib.Logging.Add(Name, results.Results.Count + " results found", LoggingLib.Logging.LoggingEnum.Info);
                    client.GetConfig();

                    var firstResult = results.Results.First();

                    LoggingLib.Logging.Add(Name, "Fetching series details", LoggingLib.Logging.LoggingEnum.Info);
                    var series = client.GetTvShow(firstResult.Id);


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

                    meta.Description = series.Overview;
                    meta.Name = series.Name;
                    meta.Id = series.Id;
                    meta.MetaAvailable = true;

                    LoggingLib.Logging.Add(Name, "Series metadata fetched: id=" + meta.Id + ", title=" + series.Name, LoggingLib.Logging.LoggingEnum.Info);

                    metadataByFolderName[dir.Name] = meta;
                    metadatas.Metadatas.Add(meta);

                    return meta;
                }
                else
                {
                    LoggingLib.Logging.Add(Name, "Unable to fetch metadata, no results", LoggingLib.Logging.LoggingEnum.Warning);

                    meta.MetaAvailable = false;
                    metadataByFolderName[dir.Name] = meta;
                    metadatas.Metadatas.Add(meta);

                    return meta;
                }
            }
            catch (Exception ex)
            {
                LoggingLib.Logging.Add(Name, "Error fetching metadata for series, target IL: " + new System.Diagnostics.StackTrace(ex).GetFrame(0).GetILOffset().ToString("x2"), ex);
                return null;
            }
        }


        private bool DownloadEpisodeMetadata(FileInfo seriesFile, DirectoryInfo baseSeriesDir, SeriesMetadata mdata, ref Size coverSize, ref Size coverThumbSize, ref Size backdropSize)
        {

            try
            {
                // check if the episode metadata was already downloaded
                if (mdata.Seasons.SelectMany(s => s.Episodes).Any(e => e.Filename == seriesFile.Name))
                    return false;

                bool downloadedData = false;

                int seasonNumber = 0;
                int episodeNumber = 0;
                bool success = false;
                var match = Regex.Match(seriesFile.Name, "S([0-9]+)E([0-9]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    seasonNumber = int.Parse(match.Groups[1].Value);
                    episodeNumber = int.Parse(match.Groups[2].Value);
                    success = true;
                }
                else
                {
                    match = Regex.Match(seriesFile.Name, "([0-9]+)x([0-9]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        seasonNumber = int.Parse(match.Groups[1].Value);
                        episodeNumber = int.Parse(match.Groups[2].Value);
                        success = true;
                    }
                }

                if (success)
                {
                    var season = mdata.Seasons.Where(s => s.Number == seasonNumber).FirstOrDefault();
                    if (season == null)
                    {
                        season = DownloadSeasonMetadata(seriesFile, baseSeriesDir, mdata, ref coverSize, ref coverThumbSize, seasonNumber);
                        if (season != null)
                        {
                            mdata.Seasons.Add(season);
                            downloadedData = true;
                        }
                        else
                            return false;
                    }

                    var episode = season.Episodes.Where(e => e.Number == episodeNumber).FirstOrDefault();
                    if (episode == null)
                    {
                        TMDbLib.Client.TMDbClient client = new TMDbLib.Client.TMDbClient(ConfigurationManager.AppSettings["themoviedbAPIKEY"]);
                        client.GetConfig();

                        var tvepisode = client.GetTvEpisode(mdata.Id, season.Number, episodeNumber);
                        if (tvepisode != null)
                        {
                            episode = new EpisodeMetadata()
                            {
                                Filename = seriesFile.Name,
                                Description = tvepisode.Overview,
                                Number = tvepisode.EpisodeNumber,
                                Name = tvepisode.Name
                            };
                            season.Episodes.Add(episode);
                            downloadedData = true;
                        }
                        else
                        {
                            episode = new EpisodeMetadata()
                            {
                                Filename = seriesFile.Name,
                                Description = "",
                                Number = episodeNumber,
                                Name = seriesFile.Name
                            };
                            season.Episodes.Add(episode);
                        }

                        // download subtitles where the video files are
                        SubtitleManager.DownloadSubtitles(seriesFile.Directory, seriesFile.FullName);
                    }
                }

                return downloadedData;
            }
            catch (Exception ex)
            {
                LoggingLib.Logging.Add(Name, "Error fetching metadata for episode, target IL: " + new System.Diagnostics.StackTrace(ex).GetFrame(0).GetILOffset().ToString("x2"), ex);
                return false;
            }
        }

        private SeasonMetadata DownloadSeasonMetadata(FileInfo seriesFile, DirectoryInfo baseSeriesDir, SeriesMetadata mdata, ref Size coverSize, ref Size coverThumbSize, int seasonNumber)
        {
            SeasonMetadata season;
            // download season data
            TMDbLib.Client.TMDbClient client = new TMDbLib.Client.TMDbClient(ConfigurationManager.AppSettings["themoviedbAPIKEY"]);
            client.GetConfig();

            var tvseason = client.GetTvSeason(mdata.Id, seasonNumber);
            if (tvseason != null)
            {
                string originalSeasonCoverPath = System.IO.Path.Combine(baseSeriesDir.FullName, "series" + seasonNumber + ".jpg");
                var seasonCoverPath = GetSeasonCoverPath(baseSeriesDir, tvseason.SeasonNumber);
                var seasonCoverThumbPath = GetSeasonCoverThumbPath(baseSeriesDir, tvseason.SeasonNumber);
                System.Net.WebClient wclient = new System.Net.WebClient();
                LoggingLib.Logging.Add(Name, "Fetching poster for season", LoggingLib.Logging.LoggingEnum.Info);
                var imageUrl = client.GetImageUrl("w1000", tvseason.PosterPath);
                wclient.DownloadFile(imageUrl, originalSeasonCoverPath);

                using (var orgBmp = new Bitmap(originalSeasonCoverPath))
                {
                    using (var coverBmp = new Bitmap(orgBmp, coverSize))
                        SaveImage(coverBmp, seasonCoverPath);

                    using (var coverThumbBmp = new Bitmap(orgBmp, coverThumbSize))
                        SaveImage(coverThumbBmp, seasonCoverThumbPath);
                }
                season = new SeasonMetadata()
                {
                    Id = tvseason.Id.Value,
                    Number = tvseason.SeasonNumber,
                };
                return season;
            }
            else
                return null;
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

        public static string GetSeasonCoverPath(DirectoryInfo dir, int seasonNumber)
        {
            if (ConfigurationManager.AppSettings["useRaw"] == "true")
                return System.IO.Path.Combine(dir.FullName, "season" + seasonNumber) + ".raw";
            else
                return System.IO.Path.Combine(dir.FullName, "season" + seasonNumber) + ".jpg";
        }

        public static string GetSeasonCoverThumbPath(DirectoryInfo dir, int seasonNumber)
        {
            if (ConfigurationManager.AppSettings["useRaw"] == "true")
                return System.IO.Path.Combine(dir.FullName, "season" + seasonNumber + "_thumb") + ".raw";
            else
                return System.IO.Path.Combine(dir.FullName, "season" + seasonNumber + "_thumb") + ".jpg";
        }


        private static void WriteSeriesMetadatas(SeriesMetadatas metadatas)
        {
            string path = ConfigurationManager.AppSettings["episodesPath"];
            string metadataPath = System.IO.Path.Combine(path, "metadata.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(SeriesMetadatas));
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
            get { return "Episode metadata service"; }
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
