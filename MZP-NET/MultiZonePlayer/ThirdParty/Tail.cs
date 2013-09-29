// http://www.codeproject.com/KB/cs/wintail.aspx

using System;
using System.IO;
using System.Text;
using System.Threading;

namespace MultiZonePlayer
{
	public class Tail
	{
		string filename = "";
		//FileSystemWatcher fileSystemWatcher = null;
		long previousSeekPosition;
        private Thread m_thread;

		public delegate void MoreDataHandler(object sender, string newData);
		public event MoreDataHandler MoreData;

		public Tail(string filename)
		{
			this.filename = filename;
            m_thread = new Thread(() => RunThread());
            m_thread.Name = "Tail " + filename.Substring(Math.Max(0, filename.Length - 20));
            m_thread.Start();
		}

        /*
		public void Start()
		{
			FileInfo targetFile = new FileInfo(this.filename);

			previousSeekPosition = 0;

			fileSystemWatcher = new FileSystemWatcher();
			fileSystemWatcher.IncludeSubdirectories = false;
			fileSystemWatcher.Path = targetFile.DirectoryName;
			fileSystemWatcher.Filter = targetFile.Name;
			
			if (!targetFile.Exists)
			{
				fileSystemWatcher.Created += new FileSystemEventHandler(TargetFile_Created);
				fileSystemWatcher.EnableRaisingEvents = true;
			}
			else
			{
				TargetFile_Changed(null, null);
				StartMonitoring();
			}
		}

		public void Stop()
		{
			fileSystemWatcher.EnableRaisingEvents = false;
			fileSystemWatcher.Dispose();
		}*/
        /*
		public string ReadFullFile()
		{
			using (StreamReader streamReader =new StreamReader(this.filename))
			{
				return streamReader.ReadToEnd();
			}
		}
        */
        /*
		public void StartMonitoring()
		{
			fileSystemWatcher.Changed += new FileSystemEventHandler(TargetFile_Changed);
			fileSystemWatcher.EnableRaisingEvents = true;
		}*/

        private void RunThread()
        {
            MLog.Log(this, "running file watcher thread");
            try
            {
                while (MZPState.IsInitialised)
                {
                    Thread.Sleep(1000);
                    TargetFile_Changed(null, null);
                }
            }
            catch (Exception)
            {
                //MLog.Log(ex, this, "Tail thread got exception");
            }
            MLog.Log(this, "file watcher thread exit");
        }

        public void Stop()
        {
            m_thread.Interrupt();
        }
        /*
		public void TargetFile_Created(object source, FileSystemEventArgs e)
		{
			StartMonitoring();
		}
        */
		public void TargetFile_Changed(object source, FileSystemEventArgs e)
		{
            if (this.MoreData == null)
                return;

			lock (this)
			{
				//read from current seek position to end of file
                FileStream fs;
                try
                {
                    fs = new FileStream(this.filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch (Exception ex)
                {
                    MLog.Log(this, "Unable to open tail file: " + this.filename);
                    return;
                }

                int sizeIncrement = Convert.ToInt32(fs.Length - this.previousSeekPosition);
                if (sizeIncrement < 0)
                {
                    this.previousSeekPosition = 0;
                    sizeIncrement = Convert.ToInt32(fs.Length);
                }

                if (sizeIncrement > 0)
                {
                    byte[] bytesRead = new byte[sizeIncrement];

                    /*if (fs.Length > maxBytes)
                    {
                        this.previousSeekPosition = fs.Length - maxBytes;
                    }
                     * */
                    fs.Seek(this.previousSeekPosition, SeekOrigin.Begin);
                    int numBytes = fs.Read(bytesRead, 0, sizeIncrement);
                    fs.Close();
                    this.previousSeekPosition += numBytes;

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < numBytes; i++)
                    {
                        sb.Append((char)bytesRead[i]);
                    }

                    //call delegates with the string
                    this.MoreData(this, sb.ToString());
                }
                else
                    fs.Close();

			}
		}
	}
}
