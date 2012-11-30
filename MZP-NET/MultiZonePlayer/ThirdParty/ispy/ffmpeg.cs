using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace iSpyApplication
{
    public class Ffmpeg : IDisposable
    {
        public Ffmpeg(MainForm owner)
        {
            _owner = owner;
        }
        private FfmpegTask _currentFfmpegTask;
        private Process _ffmpegProcess;
        // Track whether Dispose has been called.
        private bool _disposed;
        private readonly MainForm _owner;

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        #endregion

        public void ProcessTask(object o)
        {
            var task = (FfmpegTask) o;
            _ffmpegProcess = new Process();
            try
            {
                _currentFfmpegTask = task;
                _ffmpegProcess.StartInfo.FileName = task.CommandLine;
                _ffmpegProcess.StartInfo.Arguments = task.Args;
                _ffmpegProcess.StartInfo.CreateNoWindow = true;
                _ffmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _ffmpegProcess.StartInfo.UseShellExecute = false;
                _ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                _ffmpegProcess.StartInfo.RedirectStandardError = true;

                _ffmpegProcess.Exited += FfmpegProcessExited;
                _ffmpegProcess.ErrorDataReceived += FfmpegProcessErrorDataReceived;//it's not error data, just reporting data
                _ffmpegProcess.OutputDataReceived += FfmpegProcessOutputDataReceived;
                _ffmpegProcess.EnableRaisingEvents = true;
                if (MainForm.Conf.LogFFMPEGCommands)
                    MainForm.LogMessageToFile(task.CommandLine + " " + task.Args);

                _ffmpegProcess.Start();
                _ffmpegProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                _ffmpegProcess.BeginOutputReadLine();
                _ffmpegProcess.BeginErrorReadLine();
                //Console.WriteLine("FFMPEG:: Waiting for exit...");
                _ffmpegProcess.WaitForExit();
                //Console.WriteLine("stopped waiting");

                if (_currentFfmpegTask.UploadYouTube)
                {
                    try
                    {
                        string fn = _currentFfmpegTask.OutFile;
                        fn = fn.Substring(fn.LastIndexOf(@"\"));
                        //modified by dcristian
                        //YouTubeUploader.AddUpload(_currentFfmpegTask.ObjectId, fn, _currentFfmpegTask.Public, "", "");
                        YouTubeUploader.AddUpload(_currentFfmpegTask.ObjectId, fn, _currentFfmpegTask.Public, "", "", task.IsTimelapse);
                    }
                    catch (Exception ex)
                    {
                        MainForm.LogExceptionToFile(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
            
            _ffmpegProcess.Dispose();
            _ffmpegProcess = null;
            MainForm.FfmpegTaskProcessing = false;
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _ffmpegProcess.Dispose();
                }
            }
            _disposed = true;
        }

        private void FfmpegProcessExited(object sender, EventArgs e)
        {
            if (_currentFfmpegTask.DeleteOnComplete)
            {
                try
                {
                    File.Delete(_currentFfmpegTask.Filename);
                }
                catch (Exception ex)
                {
                    if (MainForm.Conf.LogFFMPEGCommands)
                        MainForm.LogErrorToFile("FFMPEG Exit Error (deletefile): " + ex.Message);
                }
            }
            //string cmdLine = _currentFfmpegTask.Args;
            //string infile = _currentFfmpegTask.Filename;
            //string infileformat = infile.Substring(infile.LastIndexOf("."));

            //string outFile = infile.Substring(infile.LastIndexOf(@"\") + 1);

            //outFile = outFile.Substring(0, outFile.IndexOf("."));

            //string outFileFormat = cmdLine.Substring(cmdLine.LastIndexOf(outFile)).Replace(outFile, "");
            //outFileFormat = outFileFormat.Substring(0, outFileFormat.IndexOf('"'));

            //string outFileFull = infile.Replace(infileformat, outFileFormat);

            //string[] outfilepath = outFileFull.Split('\\');
            if (_currentFfmpegTask.OutFile == "")
                return;
            var fi = new FileInfo(_currentFfmpegTask.OutFile);
            switch (_currentFfmpegTask.ObjectTypeId)
            {
                case 1: //microphone
                    VolumeLevel vl = _owner.GetMicrophone(_currentFfmpegTask.ObjectId);
                    if (vl!=null)
                    {
                        if (vl.FileList.SingleOrDefault(p => _currentFfmpegTask.OutFile.EndsWith(@"\"+p.Filename)) == null)
                        {
                            string[] fnpath = _currentFfmpegTask.OutFile.Split('\\');
                            vl.FileList.Insert(0, new FilesFile
                                                      {
                                                          CreatedDateTicks = DateTime.Now.Ticks,
                                                          Filename = fnpath[fnpath.Length - 1],
                                                          MaxAlarm = _currentFfmpegTask.MaxAlert,
                                                          SizeBytes = fi.Length,
                                                          DurationSeconds = _currentFfmpegTask.DurationSeconds,
                                                          IsTimelapse = false,
                                                          AlertData = _currentFfmpegTask.AlertData,
                                                          TriggerLevel = _currentFfmpegTask.TriggerLevel
                                                      });
                        }
                    }

                    break;
                case 2:
                    CameraWindow cw = _owner.GetCameraWindow(_currentFfmpegTask.ObjectId);
                    if (cw!=null)
                    {
                        if (cw.FileList.SingleOrDefault(p => _currentFfmpegTask.OutFile.EndsWith(@"\"+p.Filename)) == null)
                        {
                            string[] fnpath = _currentFfmpegTask.OutFile.Split('\\');

                            cw.FileList.Insert(0, new FilesFile
                                                      {
                                                          CreatedDateTicks = DateTime.Now.Ticks,
                                                          Filename = fnpath[fnpath.Length - 1],
                                                          MaxAlarm = _currentFfmpegTask.MaxAlert,
                                                          SizeBytes = fi.Length,
                                                          DurationSeconds = _currentFfmpegTask.DurationSeconds,
                                                          IsTimelapse = _currentFfmpegTask.IsTimelapse,
                                                          AlertData = _currentFfmpegTask.AlertData,
                                                          TriggerLevel = _currentFfmpegTask.TriggerLevel
                                                      });
                        }
                    }
                    
                    break;


            }
            

            MainForm.FfmpegTaskProcessing = false;
            //Console.WriteLine("Exited:" + _currentFfmpegTask.CommandLine);
        }

        private void FfmpegProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (MainForm.Conf.LogFFMPEGCommands)
                    MainForm.LogMessageToFile("FFMPEG: " + e.Data);
                string[] data = e.Data.Split(',');
                if (data[0].ToLower().Trim().StartsWith("output"))
                {
                    string fn = data[2].Trim();
                    fn = fn.Substring(fn.IndexOf("'") + 1);
                    fn = fn.Substring(0, fn.IndexOf("'"));
                    _currentFfmpegTask.OutFile = fn;
                }
                if (data[0].ToLower().Trim().StartsWith("duration:"))
                {
                    string d = data[0].ToLower().Replace("duration:", "").Trim().Replace(".",":");
                    string[] hmsm = d.Split(':');
                    int iHours;
                    Int32.TryParse(hmsm[0].TrimStart('0'), out iHours);
                    int iMin;
                    Int32.TryParse(hmsm[1].TrimStart('0'), out iMin); 
                    int iSec;
                    Int32.TryParse(hmsm[2].TrimStart('0'), out iSec); 
                    
                    _currentFfmpegTask.DurationSeconds = iSec + (iMin*60) + (iHours*3600);


                }
            }
        }

        private static void FfmpegProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (MainForm.Conf.LogFFMPEGCommands)
                    MainForm.LogMessageToFile(e.Data);
            }
        }
    }


    public struct FfmpegTask
    {
        public string Args;
        public string CommandLine;
        public bool DeleteOnComplete;
        public string Filename;
        public int ObjectId;
        public int ObjectTypeId;
        public double MaxAlert;
        public bool Public;
        public bool UploadYouTube;
        public bool IsTimelapse;
        public string AlertData;
        public string OutFile;
        public int DurationSeconds;
        public double TriggerLevel;

        public FfmpegTask(string commandLine, string args, string filename, bool deleteOnComplete,
                          bool uploadYouTube, bool @public, int objectId, int objectTypeId, double maxAlert, string alertData, bool isTimelapse, double triggerLevel)
        {
            Public = @public;
            CommandLine = commandLine;
            Args = args;
            Filename = filename;
            DeleteOnComplete = deleteOnComplete;
            UploadYouTube = uploadYouTube;
            ObjectId = objectId;
            ObjectTypeId = objectTypeId;
            MaxAlert = maxAlert;
            OutFile = "";
            DurationSeconds = 0;
            AlertData = alertData;
            IsTimelapse = isTimelapse;
            TriggerLevel = triggerLevel;
        }
    }
}