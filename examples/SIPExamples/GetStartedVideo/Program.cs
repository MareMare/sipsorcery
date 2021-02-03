﻿//-----------------------------------------------------------------------------
// Filename: Program.cs
//
// Description: A getting started program to demonstrate how to use the
// SIPSorcery library to send and receive a video stream.
//
// This example uses a test pattern video source and has no audio. For a
// demo that is more like a video phone, and hence more complicated, 
// see the VideoPhoneCmdLine demo.
//
// Author(s):
// Aaron Clauson (aaron@sipsorcery.com)
//
// History:
// 21 Feb 2020	Aaron Clauson	Created, Dublin, Ireland.
// 02 Feb 2021  Aaron Clauson   Simplified by switching to video test pattern only.
//
// License: 
// BSD 3-Clause "New" or "Revised" License, see included LICENSE.md file.
//-----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;

namespace demo
{
    class Program
    {
        //private static string DESTINATION = "videotest@sipsorcery.cloud";
        //private static string DESTINATION = "aaron@192.168.0.50:6060";
        private static string DESTINATION = "101@192.168.0.48";
        private static int VIDEO_FRAME_WIDTH = 640;
        private static int VIDEO_FRAME_HEIGHT = 480;

        private static Microsoft.Extensions.Logging.ILogger Log = NullLogger.Instance;

        private static Form _form;
        private static PictureBox _remoteVideoPicBox;
        private static PictureBox _localVideoPicBox;
        private static bool _isFormActivated;

        static async Task Main()
        {
            Console.WriteLine("SIPSorcery Getting Started Video Call Demo");
            Console.WriteLine("Press ctrl-c to exit.");

            Log = AddConsoleLogger();
            ManualResetEvent exitMRE = new ManualResetEvent(false);

            var sipTransport = new SIPTransport();
            //sipTransport.EnableTraceLogs();
            var userAgent = new SIPUserAgent(sipTransport, null, true);

            #region Set up a simple Windows Form with two picture boxes. 

            _form = new Form();
            _form.AutoSize = true;
            _form.BackgroundImageLayout = ImageLayout.Center;
            _localVideoPicBox = new PictureBox
            {
                Size = new Size(VIDEO_FRAME_WIDTH, VIDEO_FRAME_HEIGHT),
                Location = new Point(0, 0),
                Visible = true
            };
            _remoteVideoPicBox = new PictureBox
            {
                Size = new Size(VIDEO_FRAME_WIDTH, VIDEO_FRAME_HEIGHT),
                Location = new Point(0, VIDEO_FRAME_HEIGHT),
                Visible = true
            };
            _form.Controls.Add(_localVideoPicBox);
            _form.Controls.Add(_remoteVideoPicBox);

            #endregion

            Application.EnableVisualStyles();
            ThreadPool.QueueUserWorkItem(delegate { Application.Run(_form); });
            _form.FormClosing += (sender, e) => _isFormActivated = false;
            _form.Activated += (sender, e) => _isFormActivated = true;
            _form.FormClosed += (sender, e) => userAgent.Hangup();
            userAgent.OnCallHungup += (dialog) =>
            {
                if (_isFormActivated) { _form.Close(); }
            };

<<<<<<< HEAD
            ManualResetEvent formMre = new ManualResetEvent(false);
            _form.Activated += (object sender, EventArgs e) => formMre.Set();

            Console.WriteLine("Waiting for form activation.");
            formMre.WaitOne();

            _sipTransport.SIPTransportRequestReceived += OnSIPTransportRequestReceived;

            string executableDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

<<<<<<< HEAD
            var userAgent = new SIPUserAgent(_sipTransport, null);
<<<<<<< HEAD
            var windowsAudioEndPoint = new WindowsAudioEndPoint(new AudioEncoder(), -1, -1, true);
            windowsAudioEndPoint.RestrictCodecs(new List<AudioCodecsEnum> { AudioCodecsEnum.PCMU });
=======
            userAgent.OnCallHungup += (dialog) => exitMRE.Set();
            var windowsAudioEndPoint = new WindowsAudioEndPoint(new AudioEncoder());
            windowsAudioEndPoint.RestrictFormats(format => format.Codec == AudioCodecsEnum.PCMU);
<<<<<<< HEAD
>>>>>>> master
            var windowsVideoEndPoint = new WindowsVideoEndPoint();
=======
            //var windowsVideoEndPoint = new WindowsVideoEndPoint(new FFmpegVideoEncoder());
            var windowsVideoEndPoint = new WindowsVideoEndPoint(new VpxVideoEncoder());
            windowsVideoEndPoint.RestrictFormats(format => format.Codec == VideoCodecsEnum.VP8);

>>>>>>> master
            //windowsVideoEndPoint.OnVideoSourceError += (err) =>
            //{
            //    Log.LogError($"Video source error. {err}");
            //    if (userAgent.IsCallActive)
            //    {
            //        userAgent.Hangup();
            //    }
            //};
=======
            var userAgent = new SIPUserAgent(_sipTransport, null, true);
            userAgent.OnCallHungup += (dialog) => exitMRE.Set();
            var windowsAudioEndPoint = new WindowsAudioEndPoint(new AudioEncoder());
            windowsAudioEndPoint.RestrictFormats(format => format.Codec == AudioCodecsEnum.G722);
            var windowsVideoEndPoint = new WindowsVideoEndPoint(new VpxVideoEncoder());
>>>>>>> master

            // Fallback to a test pattern source if accessing the Windows webcam fails.
=======
            // Video sink and source to generate and consume VP8 video streams.
>>>>>>> master
            var testPattern = new VideoTestPatternSource(new VpxVideoEncoder());
            var vp8VideoSink = new VideoEncoderEndPoint();

<<<<<<< HEAD
            var mediaFileSource = new SIPSorceryMedia.FFmpeg.FFmpegFileSource(@"C:\Dev\sipsorcery\sipsorcery-core\examples\WebRTCExamples\WebRTCMp4Source\media\max_intro.mp4", true, new AudioEncoder());
            mediaFileSource.Initialise();
            mediaFileSource.RestrictCodecs(new List<VideoCodecsEnum> { VideoCodecsEnum.VP8 });
            mediaFileSource.RestrictCodecs(new List<AudioCodecsEnum> { AudioCodecsEnum.PCMU });

            MediaEndPoints mediaEndPoints = new MediaEndPoints
            {
                AudioSink = windowsAudioEndPoint,
                AudioSource = mediaFileSource,
                VideoSink = windowsVideoEndPoint,
                VideoSource = mediaFileSource,
=======
            // Add the video sink and source to the media session.
            MediaEndPoints mediaEndPoints = new MediaEndPoints
            {
                VideoSink = vp8VideoSink,
                VideoSource = testPattern,
>>>>>>> master
            };
            var voipMediaSession = new VoIPMediaSession(mediaEndPoints);
            voipMediaSession.AcceptRtpFromAny = true;

            #region Connect the video frames generate from the sink and source to the Windows form.

            testPattern.OnVideoSourceRawSample += (uint durationMilliseconds, int width, int height, byte[] sample, VideoPixelFormatsEnum pixelFormat) =>
            {
                if (_isFormActivated)
                {
                    _form?.BeginInvoke(new Action(() =>
                    {
                        if (_form.Handle != IntPtr.Zero)
                        {
                            unsafe
                            {
                                fixed (byte* s = sample)
                                {
                                    var bmpImage = new Bitmap(width, height, width * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, (IntPtr)s);
                                    _localVideoPicBox.Image = bmpImage;
                                }
                            }
                        }
                    }));
                }
            };

            vp8VideoSink.OnVideoSinkDecodedSample += (byte[] bmp, uint width, uint height, int stride, VideoPixelFormatsEnum pixelFormat) =>
            {
                if (_isFormActivated)
                {
                    _form?.BeginInvoke(new Action(() =>
                    {
                        if (_form.Handle != IntPtr.Zero)
                        {
                            unsafe
                            {
                                fixed (byte* s = bmp)
                                {
                                    var bmpImage = new Bitmap((int)width, (int)height, stride, PixelFormat.Format24bppRgb, (IntPtr)s);
                                    _remoteVideoPicBox.Image = bmpImage;
                                }
                            }
                        }
                    }));
<<<<<<< HEAD
                };
<<<<<<< HEAD

                //windowsAudioEndPoint.PauseAudio().Wait();
                //voipMediaSession.AudioExtrasSource.SetSource(AudioSourcesEnum.Music);
=======
>>>>>>> master
            }
            else
            {
                Log.LogWarning("Call attempt failed.");
                Console.WriteLine("Press ctrl-c to exit.");
            }

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                Log.LogInformation("Exiting...");
                exitMRE.Set();
=======
                }
>>>>>>> master
            };

            #endregion

            // Place the call.
            var callResult = await userAgent.Call(DESTINATION, null, null, voipMediaSession).ConfigureAwait(false);
            Console.WriteLine($"Call result {((callResult) ? "success" : "failure")}.");

            Console.WriteLine("Press any key to hangup and exit.");
            Console.ReadLine();

            if (userAgent.IsCallActive)
            {
                _isFormActivated = false;
                userAgent.Hangup();
                await Task.Delay(1000).ConfigureAwait(false);
            }

            sipTransport.Shutdown();
        }

        /// <summary>
        /// Adds a console logger. Can be omitted if internal SIPSorcery debug and warning messages are not required.
        /// </summary>
        private static Microsoft.Extensions.Logging.ILogger AddConsoleLogger()
        {
            var serilogLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();
            var factory = new SerilogLoggerFactory(serilogLogger);
            SIPSorcery.LogFactory.Set(factory);
            return factory.CreateLogger<Program>();
        }
    }
}
