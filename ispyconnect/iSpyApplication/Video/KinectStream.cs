using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using AForge.Video;
using iSpyApplication.Audio;
using iSpyApplication.Audio.streams;
using Microsoft.Kinect;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using iSpyApplication.Kinect;

namespace iSpyApplication.Video
{
    public class KinectStream : IVideoSource, IAudioSource
    {
        private readonly Pen _inferredBonePen = new Pen(Brushes.Gray, 1);
        private readonly Pen _trackedBonePen = new Pen(Brushes.Green, 2);
        private readonly Brush _trackedJointBrush = new SolidBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush _inferredJointBrush = Brushes.Yellow;
        internal static Pen TripWirePen = new Pen(Color.DarkOrange);
        private Skeleton[] _skeletons = new Skeleton[0];
        private const int JointThickness = 3;
        private KinectSensor _sensor;
        private readonly bool _skeleton, _tripwires;
        private DateTime _lastWarnedTripWire = DateTime.MinValue;
        //private readonly bool _bound;
        private ManualResetEvent _stopEvent;
        private DateTime _lastFrameTimeStamp = DateTime.Now;

        private const double MaxInterval = 1000d/15;
        
        private long _bytesReceived;
        private int _framesReceived;
        private string _uniqueKinectId;
        ////Depth Stuff
        //private short[] depthPixels;
        //private byte[] colorPixels;
        //private readonly bool _depth;

        public IAudioSource OutAudio;

        #region Audio
        private float _volume;
        private bool _listening;

        public int BytePacket = 400;

        private Stream _audioStream;
        private BufferedWaveProvider _waveProvider;
        private MeteringSampleProvider _meteringProvider;
        private SampleChannel _sampleChannel;

        public BufferedWaveProvider WaveOutProvider { get; set; }

        public event DataAvailableEventHandler DataAvailable;
        public event LevelChangedEventHandler LevelChanged;
        public event AudioSourceErrorEventHandler AudioSourceError;
        public event AudioFinishedEventHandler AudioFinished;
        public event HasAudioStreamEventHandler HasAudioStream;
        /// <summary>
        /// Buffer used to hold audio data read from audio stream.
        /// </summary>
        private readonly byte[] _audioBuffer = new byte[50 * 16 * 2];

        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (_sampleChannel != null)
                {
                    _sampleChannel.Volume = value;
                }
            }
        }

        public bool Listening
        {
            get
            {
                if (IsRunning && _listening)
                    return true;
                return false;

            }
            set
            {
                if (value)
                {
                    WaveOutProvider = new BufferedWaveProvider(RecordingFormat) { DiscardOnBufferOverflow = true };
                }

                _listening = value;
            }
        }

        public WaveFormat RecordingFormat { get; set; }

        #endregion

        void MeteringProviderStreamVolume(object sender, StreamVolumeEventArgs e)
        {
            if (LevelChanged != null)
                LevelChanged(this, new LevelChangedEventArgs(e.MaxSampleValues));

        }

        public KinectStream()
        {
            
        }

        public KinectStream(string uniqueKinectId, bool skeleton, bool tripwires)
        {
            _uniqueKinectId = uniqueKinectId;
            _skeleton = skeleton;
            _tripwires = tripwires;
            //_depth = depth;

        }

        public int Tilt
        {
            get
            {
                if (_sensor != null)
                {
                    return _sensor.ElevationAngle;
                }
                return 0;
            }
            set
            {
                if (_sensor != null)
                {
                    if (value < _sensor.MaxElevationAngle && value > _sensor.MinElevationAngle)
                        _sensor.ElevationAngle = value;
                }
            }
        }

        #region IVideoSource Members

        public event NewFrameEventHandler NewFrame;


        public event VideoSourceErrorEventHandler VideoSourceError;


        public event PlayingFinishedEventHandler PlayingFinished;


        public long BytesReceived
        {
            get
            {
                long bytes = _bytesReceived;
                _bytesReceived = 0;
                return bytes;
            }
        }


        public virtual string Source
        {
            get { return _uniqueKinectId; }
            set { _uniqueKinectId = value; }
        }


        public int FramesReceived
        {
            get
            {
                int frames = _framesReceived;
                _framesReceived = 0;
                return frames;
            }
        }

        private bool _isrunning;
        public bool IsRunning
        {
            get { return _isrunning; }
        }

        public bool MousePointer;

        public void Start()
        {
            if (_sensor != null)
                Stop();

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected && _uniqueKinectId == potentialSensor.UniqueKinectId)
                {
                    _sensor = potentialSensor;
                    break;
                }
            }
            if (_sensor==null)
            {
                MainForm.LogMessageToFile("Sensor not found: "+_uniqueKinectId);
                _isrunning = false;
                return;
            }

            
            if (_skeleton)
            {
                _sensor.SkeletonStream.Enable();
                _sensor.SkeletonFrameReady += SensorSkeletonFrameReady;
            }

            //if (_depth)
            //{
            //    _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            //    _sensor.DepthFrameReady += SensorDepthFrameReady;
            //    // Allocate space to put the depth pixels we'll receive
            //    this.depthPixels = new short[_sensor.DepthStream.FramePixelDataLength];

            //    // Allocate space to put the color pixels we'll create
            //    this.colorPixels = new byte[_sensor.DepthStream.FramePixelDataLength * sizeof(int)];

            //    // This is the bitmap we'll display on-screen
            //    _colorBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            //}
            //else
            //{
                _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _sensor.ColorFrameReady += SensorColorFrameReady;
            //}
            
            // Turn on the skeleton stream to receive skeleton frames
            

            // Start the sensor
            try
            {
                _sensor.Start();
                _audioStream = _sensor.AudioSource.Start();

                RecordingFormat = new WaveFormat(16000, 16, 1);

                WaveOutProvider = new BufferedWaveProvider(RecordingFormat) { DiscardOnBufferOverflow = true };
                _waveProvider = new BufferedWaveProvider(RecordingFormat) { DiscardOnBufferOverflow = true };


                _sampleChannel = new SampleChannel(_waveProvider);
                _meteringProvider = new MeteringSampleProvider(_sampleChannel);
                _meteringProvider.StreamVolume += MeteringProviderStreamVolume;

                if (HasAudioStream != null)
                    HasAudioStream(this, EventArgs.Empty);

                _isrunning = true;

                _stopEvent = new ManualResetEvent(false);

                // create and start new thread
                var thread = new Thread(AudioThread) { Name = "kinect audio" };
                thread.Start();
            }
            catch (Exception ex)//IOException)
            {
                MainForm.LogExceptionToFile(ex);
                _sensor = null;
                _isrunning = false;
            }
        }

        private void AudioThread()
        {
            while (_stopEvent!=null && !_stopEvent.WaitOne(0, false))
            {
                var data = _audioStream.Read(_audioBuffer, 0, _audioBuffer.Length);
                if (DataAvailable != null)
                {
                    _waveProvider.AddSamples(_audioBuffer, 0, data);

                    if (Listening)
                    {
                        WaveOutProvider.AddSamples(_audioBuffer, 0, data);
                    }

                    //forces processing of volume level without piping it out
                    var sampleBuffer = new float[data];

                    _meteringProvider.Read(sampleBuffer, 0, data);
                    DataAvailable(this, new DataAvailableEventArgs((byte[])_audioBuffer.Clone()));
                }
            }
        }

        //void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        //{
        //    using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
        //    {
        //        if (depthFrame != null)
        //        {
        //            // Copy the pixel data from the image to a temporary array
        //            depthFrame.CopyPixelDataTo(this.depthPixels);

        //            // Convert the depth to RGB
        //            int colorPixelIndex = 0;
        //            for (int i = 0; i < this.depthPixels.Length; ++i)
        //            {
        //                // discard the portion of the depth that contains only the player index
        //                short depth = (short)(this.depthPixels[i] >> DepthImageFrame.PlayerIndexBitmaskWidth);

        //                // to convert to a byte we're looking at only the lower 8 bits
        //                // by discarding the most significant rather than least significant data
        //                // we're preserving detail, although the intensity will "wrap"
        //                // add 1 so that too far/unknown is mapped to black
        //                byte intensity = (byte)((depth + 1) & byte.MaxValue);

        //                // Write out blue byte
        //                colorPixels[colorPixelIndex++] = intensity;

        //                // Write out green byte
        //                colorPixels[colorPixelIndex++] = intensity;

        //                // Write out red byte                        
        //                colorPixels[colorPixelIndex++] = intensity;

        //                // We're outputting BGR, the last byte in the 32 bits is unused so skip it
        //                // If we were outputting BGRA, we would write alpha here.
        //                ++colorPixelIndex;
        //            }

        //            // Write the pixel data into our bitmap
        //            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
        //            Bitmap bmap = (Bitmap)tc.ConvertFrom(colorPixels);
        //            NewFrame(this, new NewFrameEventArgs(bmap));
        //            // release the image
        //            bmap.Dispose(); 
        //        }
        //    }
        //}

        void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            lock (_skeletons)
            {
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame != null)
                    {
                        _skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                        skeletonFrame.CopySkeletonDataTo(_skeletons);
                    }
                }
            }
        }

        void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            if ((DateTime.Now - _lastFrameTimeStamp).TotalMilliseconds >= MaxInterval)
            {
                _lastFrameTimeStamp = DateTime.Now;

                using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
                {
                    
                    Bitmap bmap = ImageToBitmap(imageFrame);
                    if (bmap != null)
                    {
                        using (Graphics g = Graphics.FromImage(bmap))
                        {
                            lock (_skeletons)
                            {
                                foreach (Skeleton skel in _skeletons)
                                {
                                    DrawBonesAndJoints(skel, g);
                                }
                            }
                            if (_tripwires)
                            {
                                foreach (var dl in TripWires)
                                {
                                    g.DrawLine(TripWirePen, dl.StartPoint, dl.EndPoint);
                                }
                            }
                        }
                        // notify client
                        NewFrame(this, new NewFrameEventArgs(bmap));
                        // release the image
                        bmap.Dispose();
                    }
                }
            }

        }

        void DrawBonesAndJoints(Skeleton skeleton, Graphics g)
        {
            // Render Torso
            DrawBone(skeleton, g, JointType.Head, JointType.ShoulderCenter);
            DrawBone(skeleton, g, JointType.ShoulderCenter, JointType.ShoulderLeft);
            DrawBone(skeleton, g, JointType.ShoulderCenter, JointType.ShoulderRight);
            DrawBone(skeleton, g, JointType.ShoulderCenter, JointType.Spine);
            DrawBone(skeleton, g, JointType.Spine, JointType.HipCenter);
            DrawBone(skeleton, g, JointType.HipCenter, JointType.HipLeft);
            DrawBone(skeleton, g, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            DrawBone(skeleton, g, JointType.ShoulderLeft, JointType.ElbowLeft);
            DrawBone(skeleton, g, JointType.ElbowLeft, JointType.WristLeft);
            DrawBone(skeleton, g, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            DrawBone(skeleton, g, JointType.ShoulderRight, JointType.ElbowRight);
            DrawBone(skeleton, g, JointType.ElbowRight, JointType.WristRight);
            DrawBone(skeleton, g, JointType.WristRight, JointType.HandRight);

            // Left Leg
            DrawBone(skeleton, g, JointType.HipLeft, JointType.KneeLeft);
            DrawBone(skeleton, g, JointType.KneeLeft, JointType.AnkleLeft);
            DrawBone(skeleton, g, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            DrawBone(skeleton, g, JointType.HipRight, JointType.KneeRight);
            DrawBone(skeleton, g, JointType.KneeRight, JointType.AnkleRight);
            DrawBone(skeleton, g, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            if (_skeleton)
            {
                foreach (Joint joint in skeleton.Joints)
                {
                    Brush drawBrush = null;

                    if (joint.TrackingState == JointTrackingState.Tracked)
                    {
                        drawBrush = _trackedJointBrush;
                    }
                    else if (joint.TrackingState == JointTrackingState.Inferred)
                    {
                        drawBrush = _inferredJointBrush;
                    }

                    if (drawBrush != null)
                    {
                        var p = SkeletonPointToScreen(joint.Position);
                        g.FillEllipse(drawBrush, p.X, p.Y, JointThickness, JointThickness);
                    }
                }
            }
        }

        private void DrawBone(Skeleton skeleton, Graphics g, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = _inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = _trackedBonePen;
            }

            Point p1 = SkeletonPointToScreen(joint0.Position);
            Point p2 = SkeletonPointToScreen(joint1.Position);

            if (_skeleton)
            {
                g.DrawLine(drawPen, p1, p2);
            }


            if (_tripwires && TripWire != null)
            {
                for (int i = 0; i < TripWires.Count; i++)
                {
                    var dl = TripWires[i];
                    if (joint1.Position.Z * 1000 >= dl.DepthMin && joint1.Position.Z * 1000 <= dl.DepthMax)
                    {
                        if (ProcessIntersection(p1, p2, TripWires[i]))
                        {
                            if ((DateTime.Now - _lastWarnedTripWire).TotalSeconds > 5)
                            {
                                TripWire(this, EventArgs.Empty);
                                _lastWarnedTripWire = DateTime.Now;
                            }
                            break;

                        }
                    }
                }
            }
        }

        public void InitTripWires(String cfg)
        {
            TripWires.Clear();
            if (!String.IsNullOrEmpty(cfg))
            {
                try
                {
                    var tw = cfg.Trim().Split(';');
                    for (int i = 0; i < tw.Length; i++)
                    {
                        var twe = tw[i].Split(',');
                        if (!String.IsNullOrEmpty(twe[0]))
                        {
                            var sp = new Point(Convert.ToInt32(twe[0]), Convert.ToInt32(twe[1]));
                            var ep = new Point(Convert.ToInt32(twe[2]), Convert.ToInt32(twe[3]));
                            int dmin = Convert.ToInt32(twe[4]);
                            int dmax = Convert.ToInt32(twe[5]);
                            TripWires.Add(new DepthLine(sp, ep, dmin, dmax));
                        }
                    }
                }
                catch (Exception)
                {
                    TripWires.Clear();
                }
            }
        }

        public event TripWireEventHandler TripWire;
        public delegate void TripWireEventHandler(object sender, EventArgs e);
        public List<DepthLine> TripWires = new List<DepthLine>();

        public void SignalToStop()
        {
            Stop();
        }


        public void WaitForStop()
        {
            Stop();
        }


        public void Stop()
        {
            try
            {
                _sensor.Stop();
            }
            catch (IOException)
            {
            }
            if (_stopEvent != null)
            {
                _stopEvent.Set();
                Thread.Sleep(500);
                _stopEvent.Close();
            }
            _stopEvent = null;
            _sensor = null;
            _isrunning = false;

        }

        #endregion

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.

            DepthImagePoint depthPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        static Bitmap ImageToBitmap(
                     ColorImageFrame image)
        {
            try
            {
                if (image != null)
                {
                    var pixeldata =
                        new byte[image.PixelDataLength];
                    
                    image.CopyPixelDataTo(pixeldata);

                    var bmap = new Bitmap(
                        image.Width,
                        image.Height,
                        PixelFormat.Format32bppRgb);
                    BitmapData bmapdata = bmap.LockBits(
                        new Rectangle(0, 0,
                                      image.Width, image.Height),
                        ImageLockMode.WriteOnly,
                        bmap.PixelFormat);
                    var ptr = bmapdata.Scan0;
                    Marshal.Copy(pixeldata, 0, ptr,
                                 image.PixelDataLength);
                    bmap.UnlockBits(bmapdata);


                    return bmap;
                }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
            return null;
        }

        private static bool ProcessIntersection(Point a, Point b, DepthLine dl)
        {
            var c = dl.StartPoint;
            var d = dl.EndPoint;

            float ua = (d.X - c.X) * (a.Y - c.Y) - (d.Y - c.Y) * (a.X - c.X);
            float ub = (b.X - a.X) * (a.Y - c.Y) - (b.Y - a.Y) * (a.X - c.X);
            float denominator = (d.Y - c.Y) * (b.X - a.X) - (d.X - c.X) * (b.Y - a.Y);

            //bool intersection, coincident;

            if (Math.Abs(denominator) <= 0.00001f)
            {
                if (Math.Abs(ua) <= 0.00001f && Math.Abs(ub) <= 0.00001f)
                {
                    return true;
                    //intersection = coincident = true;
                    //intersectionPoint = (A + B) / 2;
                }
            }
            else
            {
                ua /= denominator;
                ub /= denominator;

                if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
                {
                    return true;
                    //intersection = true;
                    //intersectionPoint.X = A.X + ua * (B.X - A.X);
                    //intersectionPoint.Y = A.Y + ua * (B.Y - A.Y);
                }
            }
            return false;
        }

        //Bitmap ImageToBitmap(
        //    DepthImageFrame Image)
        //{
        //    if (Image != null)
        //    {
        //        short[] pixeldata =
        //            new short[Image.PixelDataLength];
        //        Image.CopyPixelDataTo(pixeldata);

        //        Bitmap bmap = new Bitmap(
        //            Image.Width,
        //            Image.Height,
        //            PixelFormat.Format16bppGrayScale);

        //        BitmapData bmapdata = bmap.LockBits(
        //            new Rectangle(0, 0,
        //                          Image.Width, Image.Height),
        //            ImageLockMode.WriteOnly,
        //            bmap.PixelFormat);
        //        IntPtr ptr = bmapdata.Scan0;
        //        Marshal.Copy(pixeldata, 0, ptr,
        //                     Image.PixelDataLength);
        //        bmap.UnlockBits(bmapdata);
        //        return bmap;
        //    }
        //    return null;
        //}
    }
}