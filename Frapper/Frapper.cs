using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Frapper
{
    public class FFMPEG
    {
        #region Properties
        private string _ffExe;
        public string ffExe
        {
            get
            {
                return _ffExe;
            }
            set
            {
                _ffExe = value;
            }
        }

        public event DataReceivedEventHandler ErrorDataReceived;

        public event DataReceivedEventHandler OutputDataReceived;

        public event EventHandler Exited;

        #endregion

        #region Constructors
        public FFMPEG()
        {
            Initialize();
        }
        public FFMPEG(string ffmpegExePath)
        {
            _ffExe = ffmpegExePath;
            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            //first make sure we have a value for the ffexe file setting
            if (string.IsNullOrEmpty(_ffExe))
            {
                object o = ConfigurationManager.AppSettings["ffmpeg:ExeLocation"];

                if (o == null)
                {
                    throw new Exception("Could not find the location of the ffmpeg exe file.  The path for ffmpeg.exe " +
                    "can be passed in via a constructor of the ffmpeg class (this class) or by setting in the app.config or web.config file.  " +
                    "in the appsettings section, the correct property name is: ffmpeg:ExeLocation");
                }
                else
                {
                    if (string.IsNullOrEmpty(o.ToString()))
                        throw new Exception("No value was found in the app setting for ffmpeg:ExeLocation");

                    _ffExe = o.ToString();
                }
            }

            if (!File.Exists(_ffExe))
                throw new Exception("Could not find a copy of ffmpeg.exe");
        }
        #endregion

        #region Run the process
        public void RunCommand(string Parameters)
        {
            //create a process info
            ProcessStartInfo oInfo = new ProcessStartInfo(this._ffExe, Parameters);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = true;
            oInfo.RedirectStandardError = true;

            //try the process
            try
            {
                //run the process
                Process proc = new Process { StartInfo = oInfo, EnableRaisingEvents = true };

                proc.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
                {
                    if (OutputDataReceived != null)
                        OutputDataReceived(sender, e);
                };
                proc.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
                {
                    if (ErrorDataReceived != null)
                        ErrorDataReceived(sender, e);
                };
                proc.Exited += (object sender, EventArgs e) =>
                {
                    if (Exited != null)
                        Exited(sender, e);
                };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.WaitForExit();

                proc.Close();
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }

}