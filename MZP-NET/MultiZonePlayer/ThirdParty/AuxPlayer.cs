/*
 * Created by SharpDevelop.
 * User: dcristian
 * Date: 11.03.2011
 * Time: 18:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace MultiZonePlayer {
	/// <summary>
	///     Description of Class1.
	/// </summary>
	public class AuxPlayer : DCPlayer {
		private String inputDeviceName;
		private IBaseFilter inputFilter;

		public AuxPlayer()
			: base() {
		}

		/*
     * Graph creation and destruction methods
     */

		public override void OpenClip(String inputDevice, ZoneGeneric zoneForm) {
			try {
				// If no fullfilepath specified by command line, show file open dialog
				inputDeviceName = inputDevice;
				//m_outputDeviceList = new Hashtable();
				//m_outputDeviceList.Add(zoneForm.GetZoneId(), outputDevice);
				this.zoneForm = zoneForm;

				// Reset status variables
				currentState = ZoneState.NotStarted;

				//this.currentVolume = VolumeFull;

				// Start playing the media file
				PlayMovieInWindow( /*deviceList, deviceIndex, form*/);
			}
			catch (Exception ex) {
				MLog.Log(null, "error openclip aux " + ex.Message + " - " + lastErrorMesg);
			}
		}

		protected override void PlayMovieInWindow() {
			int hr = 0;

			graphBuilder = (IGraphBuilder) new FilterGraph();

			inputFilter = (IBaseFilter) Marshal.BindToMoniker(inputDeviceName);
			DsError.ThrowExceptionForHR(hr);

			hr = graphBuilder.AddFilter(inputFilter, "Input capture");
			DsError.ThrowExceptionForHR(hr);

			outputFilter = (IBaseFilter) Marshal.BindToMoniker(
				zoneForm.GetClonedZones()[0].OutputDeviceAutoCompleted());

			hr = graphBuilder.AddFilter(outputFilter, "Out Renderer");
			DsError.ThrowExceptionForHR(hr);
			int pinCount;
			IPin[] pinList;
			DShowUtility.GetFilterPins(inputFilter, out pinCount, out pinList);

			hr = graphBuilder.Render(pinList[0]);
			DsError.ThrowExceptionForHR(hr);

			// QueryInterface for DirectShow interfaces
			mediaControl = (IMediaControl) graphBuilder;
			mediaEventEx = (IMediaEventEx) graphBuilder;
			mediaSeeking = (IMediaSeeking) graphBuilder;
			mediaPosition = (IMediaPosition) graphBuilder;

			// Query for video interfaces, which may not be relevant for audio files
			videoWindow = graphBuilder as IVideoWindow;
			basicVideo = graphBuilder as IBasicVideo;

			// Query for audio interfaces, which may not be relevant for video-only files
			basicAudio = graphBuilder as IBasicAudio;

			// Is this an audio-only file (no video component)?
			////CheckVisibility();

			// Have the graph signal event via window callbacks for performance
			hr = mediaEventEx.SetNotifyWindow(Handle, WMGraphNotify, IntPtr.Zero);
			DsError.ThrowExceptionForHR(hr);

			if (!isAudioOnly) {
				// Setup the video window
				hr = videoWindow.put_Owner(Handle);
				DsError.ThrowExceptionForHR(hr);

				hr = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
				DsError.ThrowExceptionForHR(hr);

				//hr = InitVideoWindow(1, 1);
				DsError.ThrowExceptionForHR(hr);

				GetFrameStepInterface();
			}
			else {
				// Initialize the default player size and enable playback menu items
			}

			// Complete window initialization
			isFullScreen = false;
			currentPlaybackRate = 1.0;

#if DEBUG
			//rot = new DsROTEntry(this.graphBuilder);
#endif

			Focus();

			// Run the graph to play the media file
			hr = mediaControl.Run();
			DsError.ThrowExceptionForHR(hr);

			SetVolume(currentVolume);
			currentState = ZoneState.Running;
		}

		public override String GetFileName() {
			return "n/a, this is auxplayer";
		}
	}
}