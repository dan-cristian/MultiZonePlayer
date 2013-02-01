/*
 * Created by SharpDevelop.
 * User: dcristian
 * Date: 11.03.2011
 * Time: 18:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using DirectShowLib;
using MultiZonePlayer;
using System.Threading;

namespace MultiZonePlayer
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	/// 
	 public enum ZoneStateEnum
  {
    Stopped, 
    Paused, 
    Running, 
    Init
  };
		 public  enum MediaType
  {
    Audio,
    Video
  }
	 public class DCPlayer : System.Windows.Forms.Form
     {

         #region vars
         protected const int WMGraphNotify = 0x0400 + 13;

         protected IGraphBuilder graphBuilder = null;
         protected IMediaControl mediaControl = null;
         protected IMediaEventEx mediaEventEx = null;
         protected IVideoWindow videoWindow = null;
    protected IBasicAudio   basicAudio = null;
    protected IBasicVideo   basicVideo = null;
    protected IMediaSeeking mediaSeeking = null;
    protected IMediaPosition mediaPosition = null;
    protected IVideoFrameStep frameStep = null;
    //DC additions
    protected IBaseFilter outputFilter = null;
    protected IBaseFilter sourceFilter = null;
    protected IBaseFilter infiniteTeeFilter = null;
    //protected Hashtable m_outputDeviceList = null;
    //protected String outputDeviceName = "none";
    protected String lastErrorMesg;
    //protected IPin outputPin = null;
    //protected IPin inputPin = null;
    protected ZoneGeneric zoneForm = null;
    protected int beforeMuteVolume = Metadata.VolumeLevels.VolumeSilence;
    protected bool autoNext = true;
  //DC additions end
    protected string filename = string.Empty;
    protected bool isAudioOnly = false;
    protected bool isFullScreen = false;
    protected int currentVolume = Metadata.VolumeLevels.VolumeSilence;
    protected Metadata.ZoneState currentState = Metadata.ZoneState.NotStarted;
    protected double currentPlaybackRate = 1.0;

    protected IntPtr hDrain = IntPtr.Zero;

#if DEBUG
    protected DsROTEntry rot = null;
#endif

#endregion

    /// <summary>
    /// Variable nécessaire au concepteur.
    /// </summary>
    protected System.ComponentModel.Container components = null;
	
     public DCPlayer()
    {
 
      InitializeComponent();
    }
    
         // play single file then stop
     public DCPlayer(ZoneGeneric zoneForm, String outputDevice, String musicFile, int volume)
     {
         InitializeComponent();
         autoNext = false;
         this.currentVolume = volume;
         OpenClip(outputDevice, musicFile, zoneForm);
     }

    /// <summary>
    /// Nettoyage des ressources utilisées.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if (components != null) 
        {
          components.Dispose();
        }
      }
      base.Dispose( disposing );
    }


    #region Code généré par le Concepteur Windows Form
    /// <summary>
    /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
    /// le contenu de cette méthode avec l'éditeur de code.
    /// </summary>
    protected void InitializeComponent()
    {
      /*this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(240, 120);
      //this.Menu = this.mainMenu1;
      this.Name = "MainForm";
      this.Text = "PlayWnd Media Player";
      
       */ this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
    }
    #endregion

    /*
     * Graph creation and destruction methods
     */

    public virtual void OpenClip(String outputDevice, String musicFile, ZoneGeneric zoneForm)
    {
      try
      {
          if (outputDevice == "")
              MLog.Log(null,"no outputdevice set for "+musicFile);

          this.filename = musicFile;
          //m_outputDeviceList = new Hashtable();
          //m_outputDeviceList.Add(zoneForm.GetZoneId(), outputDevice);
          //this.outputDeviceName = outputDevice;
          this.zoneForm = zoneForm;
          if (this.filename == string.Empty)
        {

          //this.filename = GetClipFileName();
          if (this.filename == string.Empty || filename == null)
            return;
        }
        // Start playing the media file
        PlayMovieInWindow();
      }
      catch (Exception ex)
      {
      	MLog.Log(null,"OpenClip error filename=" +musicFile+", error =" + ex.Message + " - " + ex.StackTrace+ " - "+ lastErrorMesg);
      	CloseClip();
      }
    }
    

     
    protected virtual void PlayMovieInWindow()
    {
        // Reset status variables
        this.currentState = Metadata.ZoneState.NotStarted;
        ConnectAndRunGraph();
    }

    public void UpdateOutputDevices()
    {
        long pos = Position;
        CloseClip();
        //m_outputDeviceList.Add(p_zone.ZoneId, p_outputDeviceName);
        //SetVolume(p_zone.GetDefaultVolume());
        ConnectAndRunGraph();
        Position = pos;
    }

    
    protected void ConnectAndRunGraph()
    {
		try
		{
			int hr = 0;

			this.graphBuilder = (IGraphBuilder)new FilterGraph();

#if DEBUG
			rot = new DsROTEntry(this.graphBuilder);
#endif
			hr = this.graphBuilder.AddSourceFilter(filename, "Input Source", out sourceFilter);
			lastErrorMesg = filename;
			DsError.ThrowExceptionForHR(hr);

			infiniteTeeFilter = (IBaseFilter)Marshal.BindToMoniker(DShowUtility.InfinitePinTeeFilter);
			hr = this.graphBuilder.AddFilter(infiniteTeeFilter, "InfiniteTee");
			DsError.ThrowExceptionForHR(hr);

			int loop = 0;
			foreach (IZoneActivity device in zoneForm.GetClonedZones())
			{
				while (!MZPState.Instance.PowerControl.IsPowerOn(device.ZoneDetails.ZoneId) && loop < 50)
				{
					Thread.Sleep(100);
					loop++;
				}
				if (loop >= 50)
					MLog.Log(this, "Error waiting for power on on DCPlayer graph init, loop count exceeded");
				IBaseFilter outFilter = (IBaseFilter)Marshal.BindToMoniker(device.ZoneDetails.OutputDeviceAutoCompleted);
				hr = this.graphBuilder.AddFilter(outFilter, "Out Renderer device " + device);
				DsError.ThrowExceptionForHR(hr);
			}

			int pinCount, newPinCount;
			IPin[] pinList, newPinList;

			DShowUtility.GetFilterPins(sourceFilter, out pinCount, out pinList);
			hr = this.graphBuilder.Render(pinList[0]);

			DShowUtility.GetFilterPins(infiniteTeeFilter, out pinCount, out pinList);
			for (int i = 0; i < zoneForm.GetClonedZones().Count + 1; i++)
			{
				hr = this.graphBuilder.Render(pinList[i]);
				DShowUtility.GetFilterPins(infiniteTeeFilter, out newPinCount, out newPinList);
				if (newPinCount != pinCount)
				{
					pinCount = newPinCount;
					pinList = newPinList;
				}
			}

			// QueryInterface for DirectShow interfaces
			this.mediaControl = (IMediaControl)this.graphBuilder;
			this.mediaEventEx = (IMediaEventEx)this.graphBuilder;
			this.mediaSeeking = (IMediaSeeking)this.graphBuilder;
			this.mediaPosition = (IMediaPosition)this.graphBuilder;

			// Query for video interfaces, which may not be relevant for audio files
			this.videoWindow = this.graphBuilder as IVideoWindow;
			this.basicVideo = this.graphBuilder as IBasicVideo;

			// Query for audio interfaces, which may not be relevant for video-only files
			this.basicAudio = this.graphBuilder as IBasicAudio;

			// Is this an audio-only file (no video component)?
			//CheckVisibility();

			// Have the graph signal event via window callbacks for performance
			//hr = this.mediaEventEx.SetNotifyWindow(this.Handle, WMGraphNotify, IntPtr.Zero);
			//DsError.ThrowExceptionForHR(hr);

			/*
			if (!this.isAudioOnly)
			{
			// Setup the video window
			hr = this.videoWindow.put_Owner(this.Handle);
			DsError.ThrowExceptionForHR(hr);

			hr = this.videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
			DsError.ThrowExceptionForHR(hr);

			//hr = InitVideoWindow(1, 1);
			DsError.ThrowExceptionForHR(hr);

			GetFrameStepInterface();
			}
			else
			{
			// Initialize the default player size and enable playback menu items
        
			}
			*/

			// Complete window initialization
			//CheckSizeMenu(menuFileSizeNormal);
			this.isFullScreen = false;
			this.currentPlaybackRate = 1.0;

			// Run the graph to play the media file
			hr = this.mediaControl.Run();
			DsError.ThrowExceptionForHR(hr);

			SetVolume(this.currentVolume);
			this.currentState = Metadata.ZoneState.Running;
		}
		catch (Exception ex)
		{
			MLog.Log(ex, this, "Error init graph zone "+zoneForm.ZoneDetails.ZoneName+" output="+zoneForm.ZoneDetails.OutputDeviceAutoCompleted);
		}
    }

    

    public Metadata.ZoneState GetState()
    {
        return this.currentState;
    }

    public long Position
    {
        get
        {
            if (this.mediaSeeking != null)
            {
                int hr;
                long pos;
                hr = this.mediaSeeking.GetCurrentPosition(out pos);
                return pos;
            }
            else return 0;
        }

        set
        {
            int hr;
            DsLong pos = new DsLong(value);
            // Seek to the beginning
            hr = this.mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
        }
    }

    public int PositionPercent
    {
        get {
            if (this.mediaSeeking != null)
            {
                long stoppos, currentpos;
                int hr = this.mediaSeeking.GetStopPosition(out stoppos);
                currentpos = Position;

                if (stoppos > 0)
                    return Convert.ToInt16((currentpos / Convert.ToDouble(stoppos)) * 100);
            }
            return 0;
        }
    }
    public void CloseClip()
    {
        try
        {
      int hr = 0;

      // Stop media playback
      if(this.mediaControl != null)
        hr = this.mediaControl.Stop();

      // Clear global flags
      this.currentState = Metadata.ZoneState.NotStarted;
      this.isAudioOnly = true;
      this.isFullScreen = false;

      // Free DirectShow interfaces
      CloseInterfaces();

      // Clear file name to allow selection of new file with open dialog
      //this.filename = string.Empty;
        
      // No current media state
      //this.currentState = Metadata.ZoneState.NotInitialised;

      
        }
        catch (Exception ex)
        {
            MLog.Log(ex,"Error closing player");
        }
    }

    public virtual String GetFileName()
    {
        return filename;
    }

    #region Window Form Methods

    #endregion

    //
    // Some video renderers support stepping media frame by frame with the
    // IVideoFrameStep interface.  See the interface documentation for more
    // details on frame stepping.
    //
    protected bool GetFrameStepInterface()
    {
      int hr = 0;

      IVideoFrameStep frameStepTest = null;

      // Get the frame step interface, if supported
      frameStepTest = (IVideoFrameStep) this.graphBuilder;

      // Check if this decoder can step
      hr = frameStepTest.CanStep(0, null);
      if (hr == 0)
      {
        this.frameStep = frameStepTest;
        return true;
      }
      else
      {
        // BUG 1560263 found by husakm (thanks)...
        // Marshal.ReleaseComObject(frameStepTest);
        this.frameStep = null;
        return false;
      }
    }

    protected void CloseInterfaces()
    {
      int hr = 0;

      try
      {
        lock(this)
        {
          // Relinquish ownership (IMPORTANT!) after hiding video window
          if (!this.isAudioOnly)
          {
            hr = this.videoWindow.put_Visible(OABool.False);
            DsError.ThrowExceptionForHR(hr);
            hr = this.videoWindow.put_Owner(IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);
          }

          if (this.mediaEventEx != null)
          {
            hr = this.mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);
          }

#if DEBUG
          if (rot != null)
          {
              try
              {
                  rot.Dispose();
              }
              catch (Exception) { }
            rot = null;
          }
#endif
          // Release and zero DirectShow interfaces
          if (this.mediaEventEx != null)
            this.mediaEventEx = null;
          if (this.mediaSeeking != null) 
            this.mediaSeeking = null;
          if (this.mediaPosition != null) 
            this.mediaPosition = null;
          if (this.mediaControl != null) 
            this.mediaControl = null;
          if (this.basicAudio != null) 
            this.basicAudio = null;
          if (this.basicVideo != null) 
            this.basicVideo = null;
          if (this.videoWindow != null) 
            this.videoWindow = null;
          if (this.frameStep != null) 
            this.frameStep = null;
          if (this.graphBuilder != null) 
            Marshal.ReleaseComObject(this.graphBuilder); 
            this.graphBuilder = null;

          GC.Collect();
        }
      }
      catch
      {
      }
    }

    /*
     * Media Related methods
     */

    public void PauseClip()
    {
      if (this.mediaControl == null)
        return;

      // Toggle play/pause behavior
      if ((this.currentState == Metadata.ZoneState.Paused) || (this.currentState == Metadata.ZoneState.NotStarted))
      {
        if (this.mediaControl.Run() >= 0)
            this.currentState = Metadata.ZoneState.Running;
      }
      else
      {
        if (this.mediaControl.Pause() >= 0)
            this.currentState = Metadata.ZoneState.Paused;
      }
    }

    public void StopClip()
    {
      int hr = 0;
      DsLong pos = new DsLong(0);

      if ((this.mediaControl == null) || (this.mediaSeeking == null))
        return;

      // Stop and reset postion to beginning
      if ((this.currentState == Metadata.ZoneState.Paused) || (this.currentState == Metadata.ZoneState.Running))
      {
        hr = this.mediaControl.Stop();

        // Seek to the beginning
        hr = this.mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);

        // Display the first frame to indicate the reset condition
        hr = this.mediaControl.Pause();
      }
      this.currentState = Metadata.ZoneState.NotStarted;
    }

    public int ToggleMute()
    {
      int hr = 0;

      if ((this.graphBuilder == null) || (this.basicAudio == null))
        return 0;

      // Read current volume
      hr = this.basicAudio.get_Volume(out this.currentVolume);
      if (hr == -1) //E_NOTIMPL
      {
        // Fail quietly if this is a video-only media file
        return 0;
      }
      else if (hr < 0)
      {
        return hr;
      }

      if (this.currentVolume == Metadata.VolumeLevels.VolumeSilence)
      {
          this.currentVolume = this.beforeMuteVolume;
      }
      else
      {
          this.beforeMuteVolume = this.currentVolume;
          this.currentVolume = Metadata.VolumeLevels.VolumeSilence;
      }

      // Switch volume levels
      /*
        if (this.currentVolume == VolumeFull)
        this.currentVolume = VolumeSilence;
      else
        this.currentVolume = VolumeFull;
        */
      // Set new volume
      hr = this.basicAudio.put_Volume(this.currentVolume);
      return hr;
    }

    public int SetVolume(int volume)
    {
        currentVolume = volume;
        int hr = 0;

        if ((this.graphBuilder == null) || (this.basicAudio == null))
            return 0;

        if (volume > 0) return 0;

        // Set new volume
        hr = this.basicAudio.put_Volume(volume);
        DsError.ThrowExceptionForHR(hr);
        return hr;
    }

    public int ChangeVolume(int step)
    {
        int hr = 0;

        if ((this.graphBuilder == null) || (this.basicAudio == null))
            return 0;

        // Read current volume
        hr = this.basicAudio.get_Volume(out this.currentVolume);
        if (hr == -1) //E_NOTIMPL
        {
            // Fail quietly if this is a video-only media file
            return 0;
        }
        else if (hr < 0)
        {
            return hr;
        }

        if ((this.currentVolume + step) > Metadata.VolumeLevels.VolumeFull) return Metadata.VolumeLevels.VolumeFull;
        if ((this.currentVolume + step) < Metadata.VolumeLevels.VolumeSilence) return Metadata.VolumeLevels.VolumeSilence;

        // Set new volume
        hr = this.basicAudio.put_Volume(this.currentVolume + step);
        DsError.ThrowExceptionForHR(hr);

        hr = this.basicAudio.get_Volume(out this.currentVolume);
        return hr;
    }

    public int GetVolumeLevel()
    {
        return this.currentVolume;// +"/" + VolumeFull;
    }

    public int StepOneFrame()
    {
      int hr = 0;

      // If the Frame Stepping interface exists, use it to step one frame
      if (this.frameStep != null)
      {
        // The graph must be paused for frame stepping to work
          if (this.currentState != Metadata.ZoneState.Paused)
          PauseClip();

        // Step the requested number of frames, if supported
        hr = this.frameStep.Step(1, null);
      }

      return hr;
    }

    public int StepFrames(int nFramesToStep)
    {
      int hr = 0;

      // If the Frame Stepping interface exists, use it to step frames
      if (this.frameStep != null)
      {
        // The renderer may not support frame stepping for more than one
        // frame at a time, so check for support.  S_OK indicates that the
        // renderer can step nFramesToStep successfully.
        hr = this.frameStep.CanStep(nFramesToStep, null);
        if (hr == 0)
        {
          // The graph must be paused for frame stepping to work
            if (this.currentState != Metadata.ZoneState.Paused)
            PauseClip();

          // Step the requested number of frames, if supported
          hr = this.frameStep.Step(nFramesToStep, null);
        }
      }

      return hr;
    }

    public int ModifyRate(double dRateAdjust)
    {
      int hr = 0;
      double dRate;

      // If the IMediaPosition interface exists, use it to set rate
      if ((this.mediaPosition != null) && (dRateAdjust != 0.0))
      {
        hr = this.mediaPosition.get_Rate(out dRate);
        if (hr == 0)
        {
          // Add current rate to adjustment value
          double dNewRate = dRate + dRateAdjust;
          hr = this.mediaPosition.put_Rate(dNewRate);

          // Save global rate
          if (hr == 0)
          {
            this.currentPlaybackRate = dNewRate;
          }
        }
      }

      return hr;
    }

    protected int SetRate(double rate)
    {
      int hr = 0;

      // If the IMediaPosition interface exists, use it to set rate
      if (this.mediaPosition != null)
      {
        hr = this.mediaPosition.put_Rate(rate);
        if (hr >= 0)
        {
          this.currentPlaybackRate = rate;
        }
      }

      return hr;
    }

    protected void HandleGraphEvent()
    {
      int hr = 0;
      EventCode evCode;
      IntPtr evParam1, evParam2;

      // Make sure that we don't access the media event interface
      // after it has already been released.
      if (this.mediaEventEx == null)
        return;

      // Process all queued events
      while((this.mediaEventEx != null) && (this.mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0))
      {
        // Free memory associated with callback, since we're not using it
        hr = this.mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);

        // If this is the end of the clip, reset to beginning
        if(evCode == EventCode.Complete)
        {
            if (autoNext) zoneForm.EventNextAuto();
            else
            {
                CloseClip();
                Dispose();
                return;
            }
            /*
          DsLong pos = new DsLong(0);
          // Reset to first frame of movie
          hr = this.mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning, 
            null, AMSeekingSeekingFlags.NoPositioning);
             */ 
          if (hr < 0)
          {
            // Some custom filters (like the Windows CE MIDI filter)
            // may not implement seeking interfaces (IMediaSeeking)
            // to allow seeking to the start.  In that case, just stop
            // and restart for the same effect.  This should not be
            // necessary in most cases.
            hr = this.mediaControl.Stop();
            hr = this.mediaControl.Run();
          }
        }
      }
    }

    /*
     * WinForm Related methods
     */

    protected override void WndProc(ref Message m)
    {
      switch (m.Msg)
      {
        case WMGraphNotify :
        {
          HandleGraphEvent();
          break;
        }
      }

      // Pass this message to the video window for notification of system changes
      if (this.videoWindow != null)
        this.videoWindow.NotifyOwnerMessage(m.HWnd, m.Msg, m.WParam, m.LParam);

      base.WndProc (ref m);
    }

    protected void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      StopClip();
      CloseInterfaces();
    }

    public void Tick()
    {
        HandleGraphEvent();
    }

	}
	
}
