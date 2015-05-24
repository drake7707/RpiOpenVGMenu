using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using VGMenu.Screens.Details;

namespace VGMenu
{
    public class MovieManager
    {
        private static HashSet<string> movieExtensions = new HashSet<string>(new string[] { ".avi", ".mkv", ".mpg", ".mp4", ".mpeg", ".wmv" });


        private static List<Movie> moviesCache;
        private static object cacheLock = new object();
        private static DateTime moviesCacheSince;

        public static void RefreshCache()
        {
            GetMovies(false);
        }

        public static List<Movie> GetMovies(bool useCachedData = true)
        {
            if (!useCachedData || moviesCache == null || moviesCache != null && (DateTime.UtcNow - moviesCacheSince).TotalMinutes > 10)
            {
                lock (cacheLock)
                {
                    moviesCache = GetMovies();
                    moviesCacheSince = DateTime.UtcNow;
                    return moviesCache;
                }
            }
            else
                return moviesCache;
        }

        private static List<Movie> GetMovies()
        {
            string path = ConfigurationManager.AppSettings["moviesPath"];

            var metadatas = MovieMetaDataService.ReadMovieMetadatas();
            var metadataByFolderName = metadatas.Metadatas.GroupBy(m => m.FolderName).ToDictionary(g => g.Key, g => g.First());

            List<Movie> movies = new List<Movie>();

            foreach (var movieFolderPath in System.IO.Directory.GetDirectories(path))
            {
                var dir = new DirectoryInfo(movieFolderPath);

                MovieMetadata metadata = null;
                if (!metadataByFolderName.TryGetValue(dir.Name, out metadata) || !metadata.MetaAvailable)
                    metadata = null;
                Movie m = GetMovieFromMetatadata(dir, metadata);

                movies.Add(m);
            }
            return movies;
        }

        public static Movie GetMovieFromMetatadata(DirectoryInfo movieDirectory, MovieMetadata metadata)
        {


            var coverImage = MovieMetaDataService.GetCoverPath(movieDirectory);
            var thumbCoverImage = MovieMetaDataService.GetCoverThumbPath(movieDirectory);
            var backdropImage = MovieMetaDataService.GetBackdropPath(movieDirectory);


            var movieFiles = movieDirectory.GetFiles().Where(file => !file.Name.ToLower().Contains("sample") && movieExtensions.Contains(System.IO.Path.GetExtension(file.FullName).ToLower()))
                                                            .OrderBy(fl => fl.Name);



            Movie m = new Movie()
            {
                DateModified = System.IO.File.GetLastWriteTime(movieFiles.First().FullName),
                FullCoverPath = System.IO.File.Exists(coverImage) ? coverImage : "",
                ThumbCoverPath = System.IO.File.Exists(thumbCoverImage) ? thumbCoverImage : "",
                BackdropPath = System.IO.File.Exists(backdropImage) ? backdropImage : "",
                Title = metadata == null ? movieDirectory.Name : metadata.Name,
                Description = metadata == null ? "" : metadata.Description,
                MoviePaths = movieFiles.Select(mf => new PlayableFile()
                {
                    VideoPath = mf.FullName,
                    Subtitles = SubtitleManager.GetSubtitlesForPath(mf.FullName)
                }).ToList()
            };
            return m;
        }

        

    }



    public class Movie : IDetailItem
    {
        public string Title { get; set; }

        public string Description { get; set; }
        public string FullCoverPath { get; set; }
        public string ThumbCoverPath { get; set; }
        public string BackdropPath { get; set; }

        public List<PlayableFile> MoviePaths { get; set; }

        public DateTime DateModified { get; set; }


        public IEnumerable<PlayableFile> PlayableFiles
        {
            get
            {
                return MoviePaths;
            }
        }
    }
}
