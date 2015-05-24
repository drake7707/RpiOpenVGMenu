using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoggingLib;
using System.Configuration;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using ServiceConnector;

namespace VGMenu
{
    public class OMXPlayerManager : StandardDataHost, IPlayer
    {
        public OMXPlayerManager(LibCECManager mgr, int port)
            : base(port)
        {
            var externalConfigurationFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), System.IO.Path.GetFileName(Assembly.GetExecutingAssembly().Location) + ".config");
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = externalConfigurationFile };
            System.Configuration.Configuration externalConfiguration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            rootDirectory = externalConfiguration.AppSettings.Settings["rootDirectory"].Value;
            playVideoScript = externalConfiguration.AppSettings.Settings["playvideoScript"].Value;

            reloadPathScriptPath = externalConfiguration.AppSettings.Settings["reloadPathScript"].Value;

            try
            {
                if (mgr == null)
                {
                    cecManager = new LibCECManager("cec-client");
                    cecManager.Start();
                }
                else
                    cecManager = mgr;

                cecManager.KeyPressed += new LibCECManager.KeyHandler(cecManager_KeyPressed);
                cecManager.KeyReleased += new LibCECManager.KeyHandler(cecManager_KeyReleased);

            }
            catch (Exception ex)
            {
                Logging.Add(Name, "Unable to start cec-client: " + ex.GetType().FullName + " - " + ex.Message, Logging.LoggingEnum.Error);
            }
        }

        private string rootDirectory;
        private string playVideoScript;
        private string reloadPathScriptPath;

        private bool stopped = true;

        private LibCECManager cecManager;

        private PlayInfo playInfo;

        private object playListLock = new object();
        private Queue<QueueEntry> playlist = new Queue<QueueEntry>();
        class QueueEntry
        {
            public string Guid { get; set; }
            public PlayTypeEnum PlayType { get; set; }
            public string Source { get; set; }
            public string ExtraInfo { get; set; }
        }

        private void AddToQueue(PlayTypeEnum playType, string source, string extraInfo)
        {
            lock (playListLock)
            {
                playlist.Enqueue(new QueueEntry() { Guid = System.Guid.NewGuid().ToString(), PlayType = playType, Source = source, ExtraInfo = extraInfo });
            }
        }

        private void RemoveFromQueue(string guid)
        {
            lock (playListLock)
            {
                playlist = new Queue<QueueEntry>(playlist.Where(q => q.Guid != guid));
            }
        }

        private void MoveUpInQueue(string guid)
        {
            lock (playListLock)
            {
                var newQueue = new Queue<QueueEntry>();

                var itm = playlist.Where(q => q.Guid == guid).FirstOrDefault();
                if (itm != null)
                {
                    var lst = playlist.ToList();
                    int idx = lst.IndexOf(itm);
                    if (idx > 0)
                    {
                        var tmp = lst[idx - 1];
                        lst[idx - 1] = lst[idx];
                        lst[idx] = tmp;

                        playlist = new Queue<QueueEntry>(lst);
                    }
                }
            }
        }

        private void MoveDownInQueue(string guid)
        {
            lock (playListLock)
            {
                var newQueue = new Queue<QueueEntry>();

                var itm = playlist.Where(q => q.Guid == guid).FirstOrDefault();
                if (itm != null)
                {
                    var lst = playlist.ToList();
                    int idx = lst.IndexOf(itm);
                    if (idx < lst.Count - 1)
                    {
                        var tmp = lst[idx + 1];
                        lst[idx + 1] = lst[idx];
                        lst[idx] = tmp;
                        playlist = new Queue<QueueEntry>(lst);
                    }
                }
            }
        }

        private void Play(PlayTypeEnum playType, string source, string extraInfo)
        {
            if (IsPlaying)
            {
                playInfo.OmxPlayerProcess.StandardInput.Write('q');
                playInfo.OmxPlayerProcess.StandardInput.Flush();
                System.Threading.Thread.Yield();
                System.Threading.Thread.Sleep(1000);
                if (!playInfo.OmxPlayerProcess.HasExited)
                    playInfo.OmxPlayerProcess.Kill();
            }

            if (stopped)
            {
                Logging.Add(Name, "Making the raspberry pi the active source on TV", Logging.LoggingEnum.Info);
                cecManager.MakeActiveSource();

                PlayStateChangingHandler temp = PlayStateChanging;
                if (temp != null)
                    temp(this, false, true);
            }
            else
                Logging.Add(Name, "Not stopped, making the raspberry pi the active source is not needed", Logging.LoggingEnum.Info);

            if (playType == PlayTypeEnum.Youtube || playType == PlayTypeEnum.Livestream)
                PlayYoutubeDl(playType, source, extraInfo);
            else
                PlayFile(playType, source, extraInfo);
        }

        private void PlayFromQueue()
        {
            if (IsPlaying)
            {
                playInfo.OmxPlayerProcess.StandardInput.Write('q');
                playInfo.OmxPlayerProcess.StandardInput.Flush();
                System.Threading.Thread.Yield();
                System.Threading.Thread.Sleep(1000);
                if (!playInfo.OmxPlayerProcess.HasExited)
                    playInfo.OmxPlayerProcess.Kill();
            }

            QueueEntry entry = null;
            lock (playListLock)
            {
                if (playlist.Count > 0)
                    entry = playlist.Dequeue();
            }

            if (entry != null)
            {
                Logging.Add(Name, "Starting next item from the queue", Logging.LoggingEnum.Info);
                Play(entry.PlayType, entry.Source, entry.ExtraInfo);
            }
            else
            {
                // no more entries -> stop
                Logging.Add(Name, "No more items in the queue, stopping", Logging.LoggingEnum.Info);
                stopped = true;
                throw new Exception("No items in the queue");
            }
        }

        private void StopPlayback()
        {
            if (IsPlaying)
            {
                Logging.Add(Name, "Sending stop", Logging.LoggingEnum.Info);

                stopped = true;

                playInfo.OmxPlayerProcess.StandardInput.Write('q');
                playInfo.OmxPlayerProcess.StandardInput.Flush();
                playInfo.Paused = false;

                System.Threading.Thread.Sleep(1000);


                try
                {
                    if (IsPlaying)
                    {
                        Logging.Add(Name, "Still running, killing player", Logging.LoggingEnum.Info);
                        playInfo.OmxPlayerProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Add(Name, "Unable to kill player: " + ex.GetType().FullName + " - " + ex.Message, Logging.LoggingEnum.Error);
                }
            }
            else
                throw new Exception("Player was not playing");
        }

        private void Pause()
        {
            if (IsPlaying)
            {
                Logging.Add(Name, "Sending pause", Logging.LoggingEnum.Info);
                playInfo.OmxPlayerProcess.StandardInput.Write('p');
                playInfo.OmxPlayerProcess.StandardInput.Flush();
                playInfo.Paused = !playInfo.Paused;
            }
            else
                throw new Exception("Player was not playing");
        }

        private void VolumeUp()
        {
            if (IsPlaying)
            {
                Logging.Add(Name, "Sending volume up", Logging.LoggingEnum.Info);
                playInfo.OmxPlayerProcess.StandardInput.Write('+');
                playInfo.OmxPlayerProcess.StandardInput.Flush();
            }
            else
                throw new Exception("Player was not playing");
        }

        private void VolumeDown()
        {
            if (IsPlaying)
            {
                Logging.Add(Name, "Sending volume down", Logging.LoggingEnum.Info);
                playInfo.OmxPlayerProcess.StandardInput.Write('-');
                playInfo.OmxPlayerProcess.StandardInput.Flush();

            }
            else
                throw new Exception("Player was not playing");
        }


        private void Rewind(bool shortSkip)
        {
            if (IsPlaying)
            {
                // these are keys changed in the omxplayer before compiling
                if (shortSkip)
                {
                    Logging.Add(Name, "Rewinding 30sec", Logging.LoggingEnum.Info);
                    playInfo.OmxPlayerProcess.StandardInput.Write('4');
                }
                else
                {
                    Logging.Add(Name, "Rewinding 600sec", Logging.LoggingEnum.Info);
                    playInfo.OmxPlayerProcess.StandardInput.Write('5');
                }

                playInfo.OmxPlayerProcess.StandardInput.Write('4');
                playInfo.OmxPlayerProcess.StandardInput.Flush();
            }
            else
                throw new Exception("Player was not playing");
        }


        private void Forward(bool shortSkip)
        {
            if (IsPlaying)
            {
                // these are keys changed in the omxplayer before compiling
                if (shortSkip)
                {
                    Logging.Add(Name, "Forwarding 30sec", Logging.LoggingEnum.Info);
                    playInfo.OmxPlayerProcess.StandardInput.Write('6');
                }
                else
                {
                    Logging.Add(Name, "Forwarding 600sec", Logging.LoggingEnum.Info);
                    playInfo.OmxPlayerProcess.StandardInput.Write('8');
                }
                playInfo.OmxPlayerProcess.StandardInput.Flush();
            }
            else
                throw new Exception("Player was not playing");
        }


        private void ToggleSubtitles()
        {
            if (IsPlaying)
            {
                Logging.Add(Name, "Sending toggle subtitles", Logging.LoggingEnum.Info);


                playInfo.OmxPlayerProcess.StandardInput.Write('s');
                playInfo.OmxPlayerProcess.StandardInput.Flush();
            }
            else
                throw new Exception("Player was not playing");
        }

        private object Browse(string path)
        {
            string dir = System.IO.Path.Combine(rootDirectory, path);

            string[] files = System.IO.Directory.GetFiles(dir);
            string[] subdirectories = System.IO.Directory.GetDirectories(dir);

            List<Entry> entries = new List<Entry>();
            entries.AddRange(subdirectories.Select(f => new Entry() { IsDirectory = true, Name = System.IO.Path.GetFileName(f), Path = new Uri(rootDirectory).MakeRelativeUri(new Uri(f)).ToString() }));
            entries.AddRange(files
                                .Where(f => IsVideo(f) || IsAudio(f))
                                .Select(f => new Entry()
                                {
                                    IsDirectory = false,
                                    Name = System.IO.Path.GetFileName(f),
                                    Path = new Uri(rootDirectory).MakeRelativeUri(new Uri(f)).ToString(),
                                    Type = IsVideo(f) ? PlayTypeEnum.VideoFile : PlayTypeEnum.AudioFile
                                }));


            string parentPath;
            if (new System.IO.DirectoryInfo(rootDirectory).FullName == (new System.IO.DirectoryInfo(dir)).FullName)
                parentPath = "";
            else
                parentPath = new Uri(rootDirectory).MakeRelativeUri(new Uri(new System.IO.DirectoryInfo(dir).Parent.FullName)).ToString();

            if (parentPath == "player") // todo fix this properly
                parentPath = "";

            return new
            {
                Success = true,
                PreviousPath = parentPath,
                Entries = entries
            };
        }

        private object GetStatus()
        {
            if (playInfo != null)
                return new
                {
                    Status = IsPlaying ? "playing" : "stopped",
                    Filename = (playInfo.PlayType == PlayTypeEnum.AudioFile || playInfo.PlayType == PlayTypeEnum.VideoFile) ? System.IO.Path.GetFileName(playInfo.Source) : playInfo.Source,
                    ExtraInfo = playInfo.ExtraInfo,
                    PlayType = playInfo.PlayType,
                    Subtitles = playInfo.Subtitles,
                    Paused = playInfo.Paused,
                };
            else
                return new
                {
                    Status = IsPlaying ? "playing" : "stopped",
                    Filename = "",
                    Subtitles = "",
                    Paused = false,
                };
        }

        private object GetPlaylist()
        {
            lock (playListLock)
            {
                return playlist.Select(pl => new
                {
                    Guid = pl.Guid,
                    PlayType = pl.PlayType,
                    Source = (pl.PlayType == PlayTypeEnum.AudioFile || pl.PlayType == PlayTypeEnum.VideoFile) ? pl.Source : System.IO.Path.GetFileName(pl.Source),
                    ExtraInfo = pl.ExtraInfo
                }).ToArray();
            }
        }

        private object GetHtml(string url)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                client.Encoding = Encoding.UTF8;
                string html = client.DownloadString(url);

                return new
                {
                    Success = true,
                    Html = html
                };
            }
        }

        private void ReloadMounts()
        {
            Logging.Add(Name, "Reloading shares using script", Logging.LoggingEnum.Info);
            var sh = Process.Start(new ProcessStartInfo()
            {
                FileName = "/bin/sh",
                Arguments = reloadPathScriptPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            });

            string line = sh.StandardOutput.ReadLine();
            while (line != null)
            {

                if (!string.IsNullOrEmpty(line))
                    Logging.Add(Name, line, Logging.LoggingEnum.Info);
                System.Threading.Thread.Sleep(25);

                line = sh.StandardOutput.ReadLine();
            }
        }

        private void PlayYoutubeDl(PlayTypeEnum playType, string streamUrl, string extraInfo)
        {
            Logging.Add(Name, "Attempting to play stream " + streamUrl, Logging.LoggingEnum.Info);

            playInfo = new PlayInfo()
            {
                Source = streamUrl,
                Subtitles = "",
                PlayType = playType,
                ExtraInfo = extraInfo,
                Paused = false
            };

            //AddLog("Evaluating youtube url: " + "youtube-dl " + "-g '" + streamUrl + "'");
            //var youtubeDl = Process.Start(new ProcessStartInfo()
            //{
            //    FileName = "youtube-dl",
            //    Arguments = "-g '" + streamUrl + "'",
            //    UseShellExecute = false,
            //    RedirectStandardInput = true,
            //    RedirectStandardOutput = true
            //});

            //var outputStream = youtubeDl.StandardOutput;
            //var url = outputStream.ReadToEnd().Trim();

            //AddLog("Url resolved to '" + url + "'");

            //if (!string.IsNullOrEmpty(url))
            //{
            string args = "-t" + (int)playType + " '" + streamUrl + "'";
            Logging.Add(Name, "Starting playvido script: " + playVideoScript + " " + args, Logging.LoggingEnum.Info);
            StartOmxPlayer(args);
            //}
            //else
            //    return new Result() { Success = false, Message = "Error starting stream: stream could not be resolved" };
        }

        private void PlayFile(PlayTypeEnum playType, string file, string extraInfo)
        {
            Logging.Add(Name, "Attempting to play file " + file, Logging.LoggingEnum.Info);

            string filename = System.IO.Path.Combine(rootDirectory, file);

            string subtitles = extraInfo;
            //string subtitles = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), System.IO.Path.GetFileNameWithoutExtension(filename) + ".srt");
            //Logging.Add(Name, "Searching for subtitles .. " + subtitles, Logging.LoggingEnum.Info);
            if (!System.IO.File.Exists(subtitles))
            {
                Logging.Add(Name, "No subtitles found", Logging.LoggingEnum.Info);
                subtitles = "";
            }

            playInfo = new PlayInfo()
            {
                Source = file,
                Subtitles = subtitles,
                PlayType = playType,
                ExtraInfo = extraInfo,
                Paused = false
            };

            string args = "-t" + (int)playType + " '" + filename + "'" + (!string.IsNullOrEmpty(subtitles) ? " --subtitles '" + subtitles + "'" : "");
            Logging.Add(Name, "Starting playVideoScript: " + playVideoScript + " " + args, Logging.LoggingEnum.Info);
            StartOmxPlayer(args);
        }

        private void StartOmxPlayer(string args)
        {
            try
            {

                stopped = false;
                var omxPlayerProcess = Process.Start(new ProcessStartInfo()
                {
                    FileName = "/bin/sh",
                    Arguments = playVideoScript + " " + args,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,

                });
                omxPlayerProcess.EnableRaisingEvents = true;
                playInfo.OmxPlayerProcess = omxPlayerProcess;
                omxPlayerProcess.Exited += new EventHandler(omxPlayerProcess_Exited);

                Thread outputLogging = new System.Threading.Thread(() =>
                {
                    try
                    {
                        string line = omxPlayerProcess.StandardOutput.ReadLine();
                        while (line != null)
                        {
                            Logging.Add(Name, line, Logging.LoggingEnum.Info);
                            line = omxPlayerProcess.StandardOutput.ReadLine();
                            System.Threading.Thread.Sleep(25);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Add(Name, "Unable to read output of OMX Player: " + ex.GetType().FullName + " - " + ex.Message, Logging.LoggingEnum.Warning);
                    }
                });
                outputLogging.IsBackground = true;
                outputLogging.Start();


            }
            catch (Exception ex)
            {
                throw new Exception("Error starting process omxplayer: " + ex.GetType().FullName + " - " + ex.Message, ex);
            }
        }

        void omxPlayerProcess_Exited(object sender, EventArgs e)
        {
            Logging.Add(Name, "Omxplayer exited", Logging.LoggingEnum.Info);
            // remove handler
            ((Process)sender).Exited -= omxPlayerProcess_Exited;

            playInfo = null;

            if (!stopped)
            {
                try
                {
                    PlayFromQueue();
                }
                catch (Exception ex)
                {
                    // no more items in the queue
                }
            }

            if (stopped) // stopped
            {
                PlayStateChangingHandler temp = PlayStateChanging;
                if (temp != null)
                    temp(this, true, false);
            }
        }




        void cecManager_KeyPressed(string key, byte code)
        {
            if (IsPlaying)
            {
                if (key == "play")
                {
                    Logging.Add(Name, "Play button from remote pressed, toggle pause", Logging.LoggingEnum.Info);
                    Pause();
                }
                else if (key == "Fast forward")
                {
                    Logging.Add(Name, "Fast forward button from remote pressed, forwarding", Logging.LoggingEnum.Info);
                    Forward(false);
                }
                else if (key == "rewind")
                {
                    Logging.Add(Name, "Rewing button from remote pressed, rewinding", Logging.LoggingEnum.Info);
                    Rewind(false);
                }
            }
        }

        void cecManager_KeyReleased(string key, byte code)
        {
            if (IsPlaying)
            {
                if (key == "stop")
                {
                    Logging.Add(Name, "Stop button from remote pressed, stopping playback", Logging.LoggingEnum.Info);
                    StopPlayback();
                }
            }
        }

        private bool IsVideo(string path)
        {
            string ext = System.IO.Path.GetExtension(path).ToLower();
            return ext == ".mkv" || ext == ".avi" || ext == ".wmv" || ext == ".mp4" || ext == ".flv";
        }

        private bool IsAudio(string path)
        {
            string ext = System.IO.Path.GetExtension(path).ToLower();
            return ext == ".mp3";
        }

        public bool IsPlaying
        {
            get
            {
                if (playInfo == null)
                    return false;
                else
                    return playInfo.OmxPlayerProcess != null && !playInfo.OmxPlayerProcess.HasExited;
            }
        }

        public override void StopService()
        {
            if (IsPlaying)
                StopPlayback();

            cecManager.Stop();
        }

        public string Name { get { return "OMX Player"; } }

        void IPlayer.Play(string path, string subtitlePath)
        {
            if (IsPlaying)
                StopPlayback();

            Play(PlayTypeEnum.VideoFile, path, subtitlePath);
        }

        void IPlayer.Queue(string path, string subtitlePath)
        {
            AddToQueue(PlayTypeEnum.VideoFile, path, subtitlePath);
        }

        public event PlayStateChangingHandler PlayStateChanging;
    }


    class Entry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public PlayTypeEnum Type { get; set; }
    }

    public enum PlayTypeEnum
    {
        VideoFile = 0,
        AudioFile = 1,
        Youtube = 2,
        Livestream = 3
    }

    class PlayInfo
    {
        public Process OmxPlayerProcess { get; set; }
        public PlayTypeEnum PlayType { get; set; }
        public string Source { get; set; }
        public string ExtraInfo { get; set; }
        public string Subtitles { get; set; }


        public bool Paused { get; set; }
    }

}
