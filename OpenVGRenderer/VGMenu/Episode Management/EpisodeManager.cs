using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using VGMenu.Screens.Details;

namespace VGMenu
{
    class EpisodeManager
    {
        public static HashSet<string> episodeExtensions = new HashSet<string>(new string[] { ".avi", ".mkv", ".mpg", ".mp4", ".mpeg", ".wmv" });

        private static List<Series> seriesCache;
        private static object cacheLock = new object();
        private static DateTime seriesCacheSince;

        public static void RefreshCache()
        {
            
            GetSeries(false);
        }

        public static List<Series> GetSeries(bool useCachedData = true)
        {
            if (!useCachedData || seriesCache == null || seriesCache != null && (DateTime.UtcNow - seriesCacheSince).TotalMinutes > 10)
            {
                lock (cacheLock)
                {
                    seriesCache = GetSeries();
                    seriesCacheSince = DateTime.UtcNow;
                    return seriesCache;
                }
            }
            else
                return seriesCache;
        }

        private static List<Series> GetSeries()
        {
            string path = ConfigurationManager.AppSettings["episodesPath"];

            var metadatas = EpisodeMetaDataService.ReadEpisodeMetadatas();
            var metadataByFolderName = metadatas.Metadatas.GroupBy(m => m.FolderName).ToDictionary(g => g.Key, g => g.First());

            List<Series> series = new List<Series>();

            foreach (var seriesFolderPath in System.IO.Directory.GetDirectories(path))
            {
                var dir = new DirectoryInfo(seriesFolderPath);

                SeriesMetadata metadata = null;
                if (!metadataByFolderName.TryGetValue(dir.Name, out metadata) || !metadata.MetaAvailable)
                    metadata = null;
                Series m = GetSeriesFromMetatadata(dir, metadata);

                series.Add(m);
            }
            return series;
        }

        public static Series GetSeriesFromMetatadata(DirectoryInfo seriesDirectory, SeriesMetadata metadata)
        {
            var coverImage = EpisodeMetaDataService.GetCoverPath(seriesDirectory);
            var thumbCoverImage = EpisodeMetaDataService.GetCoverThumbPath(seriesDirectory);
            var backdropImage = EpisodeMetaDataService.GetBackdropPath(seriesDirectory);


            var seriesFiles = GetAllEpisodeFilesFromSeries(seriesDirectory);


            Series series = new Series()
            {
                Name = (metadata == null || string.IsNullOrEmpty(metadata.Name)) ? seriesDirectory.Name : metadata.Name,
                Description = metadata == null ? "" : metadata.Description,
                Seasons = new List<Season>(),

                FullCoverPath = System.IO.File.Exists(coverImage) ? coverImage : "",
                ThumbCoverPath = System.IO.File.Exists(thumbCoverImage) ? thumbCoverImage : "",
                BackdropPath = System.IO.File.Exists(backdropImage) ? backdropImage : "",
            };

            Dictionary<EpisodeMetadata, SeasonMetadata> seasonsOfEpisodeMetadata = new Dictionary<EpisodeMetadata, SeasonMetadata>();
            Dictionary<string, EpisodeMetadata> episodeMetadataByFilename = new Dictionary<string, EpisodeMetadata>();
            if (metadata != null)
            {
                foreach (var ms in metadata.Seasons)
                {
                    foreach (var me in ms.Episodes)
                    {
                        seasonsOfEpisodeMetadata[me] = ms;
                        episodeMetadataByFilename[me.Filename] = me;
                    }
                }
            }


            Dictionary<int, Season> seasons = new Dictionary<int, Season>();

            foreach (var epFile in seriesFiles)
            {
                EpisodeMetadata me;
                if (episodeMetadataByFilename.TryGetValue(epFile.Name, out me))
                {
                    SeasonMetadata ms = seasonsOfEpisodeMetadata[me];

                    Season season;
                    if (!seasons.TryGetValue(ms.Number, out season))
                    {
                        var seasonCoverImage = EpisodeMetaDataService.GetSeasonCoverPath(seriesDirectory, ms.Number);
                        var seasonCoverThumbImage = EpisodeMetaDataService.GetSeasonCoverThumbPath(seriesDirectory, ms.Number);
                        seasons[ms.Number] = season = new Season()
                        {
                            Series = series,
                            Number = ms.Number,
                            FullCoverPath = System.IO.File.Exists(seasonCoverImage) ? seasonCoverImage : "",
                            ThumbCoverPath = System.IO.File.Exists(seasonCoverThumbImage) ? seasonCoverThumbImage : "",
                            Episodes = new List<Episode>()
                        };
                        series.Seasons.Add(season);
                    }

                    Episode e = new Episode()
                    {
                        DateModified = System.IO.File.GetLastWriteTime(epFile.FullName),
                        Season = season,
                        Number = me.Number,
                        Description = me.Description,
                        EpisodePath = epFile.FullName,
                        Name = me.Name,
                        Subtitles = SubtitleManager.GetSubtitlesForPath(epFile.FullName)
                    };
                    season.Episodes.Add(e);
                }
                else
                {
                    // no metadata for the episode, add the episode to a dummy season
                    Season season;
                    if (!seasons.TryGetValue(-1, out season))
                    {
                        seasons[-1] = season = new Season()
                        {
                            Series = series,
                            Number = -1,
                            FullCoverPath = "",
                            ThumbCoverPath = "",
                            Episodes = new List<Episode>()
                        };
                        series.Seasons.Add(season);
                    }
                    Episode e = new Episode()
                    {
                        DateModified = System.IO.File.GetLastWriteTime(epFile.FullName),
                        Season = season,
                        Number = -1,
                        Description = "",
                        Name = epFile.Name,
                        EpisodePath = epFile.FullName,
                        Subtitles = SubtitleManager.GetSubtitlesForPath(epFile.FullName)
                    };
                    season.Episodes.Add(e);
                }
            }

            return series;
        }

        public static FileInfo[] GetAllEpisodeFilesFromSeries(DirectoryInfo seriesDirectory)
        {
            var seriesFiles = seriesDirectory.GetAllFilesInDirectory().Where(file => !file.Name.ToLower().Contains("sample") && episodeExtensions.Contains(System.IO.Path.GetExtension(file.FullName).ToLower()))
                                                            .OrderBy(fl => fl.Name).ToArray();
            return seriesFiles;
        }
    }



    public class Series
    {
        public Series()
        {
            Seasons = new List<Season>();
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<Season> Seasons { get; set; }

        public string FullCoverPath { get; set; }

        public string ThumbCoverPath { get; set; }

        public string BackdropPath { get; set; }
    }

    public class Season
    {
        public Series Series { get; set; }

        public int Number { get; set; }

        public List<Episode> Episodes { get; set; }

        public string FullCoverPath { get; set; }

        public string ThumbCoverPath { get; set; }
    }

    public class Episode : IDetailItem
    {
        public Season Season { get; set; }

        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return System.IO.Path.GetFileName(EpisodePath);
                else
                {
                    if (Season.Number == -1)
                    {
                        if (Number != -1)
                            return Number.ToString("00") + " " + Name;
                        else
                            return Name;
                    }
                    else
                    {
                        if (Number != -1)
                            return Season.Number.ToString("00") + "x" + Number.ToString("00") + " " + Name;
                        else
                            return Name;
                        
                    }
                }
            }
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string EpisodePath { get; set; }
        public List<Subtitle> Subtitles { get; set; }

        public int Number { get; set; }

        public DateTime DateModified { get; set; }

        public string BackdropPath
        {
            get { return Season.Series.BackdropPath; }
        }

        public string FullCoverPath
        {
            get { return Season.FullCoverPath; }
        }

        public IEnumerable<PlayableFile> PlayableFiles
        {
            get { yield return new PlayableFile() { Subtitles = Subtitles, VideoPath = EpisodePath }; }
        }
    }
}
