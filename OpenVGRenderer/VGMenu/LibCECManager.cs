using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using LoggingLib;

namespace VGMenu
{
    public class LibCECManager
    {

        private Process cecClientProcess;
        private string cecClientPath;
        public LibCECManager(string cecClientPath)
        {
            this.cecClientPath = cecClientPath;
        }

        public static string Name { get { return "Lib-CEC"; } }

        public void Start()
        {
            if (cecClientProcess != null && !cecClientProcess.HasExited)
                Stop();

            cecClientProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = cecClientPath,

                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            });

            Thread t = new Thread(() =>
            {
                try
                {
                    string line = cecClientProcess.StandardOutput.ReadLine();
                    while (line != null)
                    {
                        Logging.Add(Name, line, Logging.LoggingEnum.Info);
                        ProcessLine(line);

                        line = cecClientProcess.StandardOutput.ReadLine();
                        System.Threading.Thread.Sleep(25);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Add(Name, "Unable to read output of cec-client: " + ex.GetType().FullName + " - " + ex.Message + Environment.NewLine + ex.StackTrace, Logging.LoggingEnum.Error);
                    Stop();
                }

            });
            t.IsBackground = true;
            t.Start();
        }

        public void Stop()
        {
            if (cecClientProcess != null && !cecClientProcess.HasExited)
            {
                cecClientProcess.StandardInput.WriteLine("\x3");
                cecClientProcess.StandardInput.Flush();

                System.Threading.Thread.Sleep(5000);
                try
                {
                    if (!cecClientProcess.HasExited)
                    {
                        Logging.Add(Name,"cec-client didn't exit after sending ctrl+c, killing process", Logging.LoggingEnum.Warning);
                        cecClientProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Add(Name, "Unable to kill cec-client: " + ex.GetType().FullName + " - " + ex.Message, Logging.LoggingEnum.Error);
                }
            }
        }

        private void ProcessLine(string line)
        {
            if (line.StartsWith("DEBUG:"))
            {
                if (line.Contains("key pressed: "))
                {
                    string keypart = line.Substring(line.IndexOf("key pressed: ") + "key pressed: ".Length);
                    string key = keypart.Substring(0, keypart.LastIndexOf('(') - 1);
                    byte code = byte.Parse(keypart.Substring(keypart.LastIndexOf('(') + 1).Replace(")", ""), System.Globalization.NumberStyles.HexNumber);
                    OnKeyPressed(key, code);
                }
                else if (line.Contains("key auto-released: "))
                {
                    string keypart = line.Substring(line.IndexOf("key auto-released: ") + "key auto-released: ".Length);
                    string key = keypart.Substring(0, keypart.LastIndexOf('(') - 1);
                    byte code = byte.Parse(keypart.Substring(keypart.LastIndexOf('(') + 1).Replace(")", ""), System.Globalization.NumberStyles.HexNumber);
                    OnKeyReleased(key, code);
                }
            }
        }

        protected virtual void OnKeyPressed(string key, byte code)
        {
            Logging.Add(Name, "Key pressed: '" + key + "'",  Logging.LoggingEnum.Info);
            KeyHandler temp = KeyPressed;
            if (temp != null)
                temp(key, code);
        }

        protected virtual void OnKeyReleased(string key, byte code)
        {
            Logging.Add(Name, "Key released: '" + key + "'", Logging.LoggingEnum.Info);
            KeyHandler temp = KeyReleased;
            if (temp != null)
                temp(key, code);
        }

        public void MakeActiveSource()
        {
            if (cecClientProcess != null && !cecClientProcess.HasExited)
            {
                Logging.Add(Name, "Make current input active source on TV", Logging.LoggingEnum.Info);
                cecClientProcess.StandardInput.WriteLine("as");
                cecClientProcess.StandardInput.Flush();
            }
            else
                Logging.Add(Name, "CEC-Client is not available, can't make current input active source on TV", Logging.LoggingEnum.Warning);
        }

        public delegate void KeyHandler(string key, byte code);
        public event KeyHandler KeyPressed;
        public event KeyHandler KeyReleased;


    }
}
