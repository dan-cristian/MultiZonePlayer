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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using DirectShowLib;
using MultiZonePlayer;

namespace MultiZonePlayer
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	/// 

         public class AuxPlayer : DCPlayer
         {

             private String inputDeviceName;
             private IBaseFilter inputFilter;
             

             public AuxPlayer()
                 : base()
             {

             }

             
             
             /*
     * Graph creation and destruction methods
     */

             public override void OpenClip(String inputDevice, ZoneGeneric zoneForm)
             {
                 try
                 {
                     // If no filename specified by command line, show file open dialog
                     this.inputDeviceName = inputDevice;
                     //m_outputDeviceList = new Hashtable();
                     //m_outputDeviceList.Add(zoneForm.GetZoneId(), outputDevice);
                     this.zoneForm = zoneForm;


                     // Reset status variables
                     this.currentState = ZoneState.NotStarted;

                     //this.currentVolume = VolumeFull;

                     // Start playing the media file
                     PlayMovieInWindow(/*deviceList, deviceIndex, form*/);
                 }
                 catch (Exception ex)
                 {
                     MLog.Log(null,"error openclip aux "+ ex.Message + " - " + lastErrorMesg);
                 }
             }

             protected override void PlayMovieInWindow()
             {
                 int hr = 0;

                 this.graphBuilder = (IGraphBuilder)new FilterGraph();

                 inputFilter = (IBaseFilter)Marshal.BindToMoniker(inputDeviceName);
                 DsError.ThrowExceptionForHR(hr);

                 hr = this.graphBuilder.AddFilter(inputFilter, "Input capture");
                 DsError.ThrowExceptionForHR(hr);

                 outputFilter = (IBaseFilter)Marshal.BindToMoniker(
					 zoneForm.GetClonedZones()[0].OutputDeviceAutoCompleted());

                 hr = this.graphBuilder.AddFilter(outputFilter, "Out Renderer");
                 DsError.ThrowExceptionForHR(hr);
                 int pinCount;
                 IPin[] pinList;
                 DShowUtility.GetFilterPins(inputFilter, out pinCount, out pinList);

                 hr = this.graphBuilder.Render(pinList[0]);
                 DsError.ThrowExceptionForHR(hr);

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
                 ////CheckVisibility();

                 // Have the graph signal event via window callbacks for performance
                 hr = this.mediaEventEx.SetNotifyWindow(this.Handle, WMGraphNotify, IntPtr.Zero);
                 DsError.ThrowExceptionForHR(hr);

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

                 // Complete window initialization
                 this.isFullScreen = false;
                 this.currentPlaybackRate = 1.0;

#if DEBUG
      //rot = new DsROTEntry(this.graphBuilder);
#endif

                 this.Focus();

                 // Run the graph to play the media file
                 hr = this.mediaControl.Run();
                 DsError.ThrowExceptionForHR(hr);

                 SetVolume(this.currentVolume);
                 this.currentState = ZoneState.Running;

             }

             public override String GetFileName()
             {
                 return "n/a, this is auxplayer";
             }
         }

}
