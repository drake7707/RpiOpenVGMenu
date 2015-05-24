using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Configuration;

namespace VGMenu
{
    class SubtitleManager
    {

        public static List<Subtitle> GetSubtitlesForPath(string path)
        {
            string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
            var srtFiles = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(path)).Where(f => System.IO.Path.GetExtension(f).ToLower() == ".srt").ToArray();
            List<Subtitle> subtitles = new List<Subtitle>();

            var neutralCultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures)
                                                                  .GroupBy(c => c.TwoLetterISOLanguageName)
                                                                  .ToDictionary(c => c.Key, c => c.First());

            foreach (var srt in srtFiles)
            {
                // <moviefilename>.<language>.srt
                if (srt.Contains(baseName))
                {
                    string languageCode = System.IO.Path.GetFileNameWithoutExtension(srt).Split('.').LastOrDefault();
                    CultureInfo lang;
                    if (!string.IsNullOrEmpty(languageCode) && languageCode.Length == 2 && neutralCultures.TryGetValue(languageCode.ToLower(), out lang))
                    {
                        subtitles.Add(new Subtitle() { Language = lang, SubtitlePath = srt });
                    }
                    else
                        subtitles.Add(new Subtitle() { Language = null, SubtitlePath = srt });
                }
            }

            return subtitles;
        }

        public static void DownloadSubtitles(DirectoryInfo dir, string filepath)
        {
            string[] languages = ConfigurationManager.AppSettings["subtitleLanguages"].Split(',').Select(part => part.Trim()).ToArray();
            foreach (var lang in languages)
            {
                LoggingLib.Logging.Add(Name, "Trying to download subtitles for '" + System.IO.Path.GetFileName(filepath) + "', language '" + lang + "'", LoggingLib.Logging.LoggingEnum.Info);
                try
                {
                    //            <add key="subtitleDownloaderPath" value="/usr/local/bin/subliminal" />
                    //<add key="subtitleDownloaderParameters" value="-l %lang% -- &quot;%moviefile%&quot;" />
                    //<add key="subtitleLanguages" value="nl, en" />

                    string targetSubtitlesFilename = System.IO.Path.GetFileNameWithoutExtension(filepath) + "." + lang.ToLower() + ".srt";

                    string subtitleDownloadPath = ConfigurationManager.AppSettings["subtitleDownloaderPath"];
                    string args = ConfigurationManager.AppSettings["subtitleDownloaderParameters"];
                    args = args.Replace("%lang%", lang)
                               .Replace("%moviefile%", System.IO.Path.GetFileName(filepath))
                               .Replace("%targetsubtitlefile%", targetSubtitlesFilename);

                    LoggingLib.Logging.Add(Name, "Starting process '" + subtitleDownloadPath + " " + args + "'", LoggingLib.Logging.LoggingEnum.Info);
                    var subdownProcess = Process.Start(new ProcessStartInfo()
                    {
                        FileName = subtitleDownloadPath,
                        Arguments = args,
                        WorkingDirectory = dir.FullName,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true
                    });

                    var outputStream = subdownProcess.StandardOutput;
                    string log = outputStream.ReadToEnd().Trim();
                    foreach (var logline in log.Split(Environment.NewLine.ToCharArray()).Where(l => !string.IsNullOrEmpty(l)))
                    {
                        LoggingLib.Logging.Add(Name, logline, LoggingLib.Logging.LoggingEnum.Info);
                    }

                    if (!System.IO.File.Exists(System.IO.Path.Combine(dir.FullName, targetSubtitlesFilename)))
                    {
                        LoggingLib.Logging.Add(Name, "Subtitle file not found, assuming the subtitle download failed", LoggingLib.Logging.LoggingEnum.Info);
                    }
                    else
                        LoggingLib.Logging.Add(Name, "Subtitle file succesfully downloaded", LoggingLib.Logging.LoggingEnum.Info);
                }
                catch (Exception ex)
                {
                    LoggingLib.Logging.Add(Name, "Unable to download subtitle file", ex);
                }
            }
        }

        public static string Name { get { return "Subtitle manager"; } }
    }
}
