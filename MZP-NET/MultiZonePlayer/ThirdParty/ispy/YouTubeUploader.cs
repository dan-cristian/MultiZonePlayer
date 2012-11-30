using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.GData.Client;
using Google.GData.Client.ResumableUpload;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;

namespace iSpyApplication
{
    internal static class YouTubeUploader
    {
        private static Authenticator _youTubeAuthenticator;
        private static readonly Queue<UserState> UploadFiles = new Queue<UserState>(40);
        private static ResumableUploader _ru;
        private static GDataCredentials _credentials;
        private static bool _uploading;

        public static string AddUpload(int objectId, string filename, bool @public)
        {
            return AddUpload(objectId, filename, @public, "", "", false);
        }

        // modified by dcristian
        public static string AddUpload(int objectId, string filename, bool @public, string emailOnComplete,
                                       string message, bool isTimeLapse)
        {
            if (string.IsNullOrEmpty(MainForm.Conf.YouTubeUsername))
            {
                return LocRm.GetString("YouTubeAddSettings");
            }
            if (UploadFiles.SingleOrDefault(p => p.Filename == filename) != null)
                return LocRm.GetString("YouTubeMovieInQueue");

            if (UploadFiles.Count == 40)
                return LocRm.GetString("YouTubeQueueFull");

            int i = MainForm.Conf.UploadedVideos.IndexOf(filename);
            if (i != -1)
            {
                if (emailOnComplete != "")
                {
                    string cfg = MainForm.Conf.UploadedVideos.Substring(i);
                    string vid = cfg.Substring(cfg.IndexOf("|") + 1);
                    if (vid.IndexOf(",") != -1)
                        vid = vid.Substring(0, vid.IndexOf(","));
                    SendYouTubeMails(emailOnComplete, message, vid);
                    return LocRm.GetString("YouTubUploadedAlreadyNotificationsSent");
                }
                return LocRm.GetString("YouTubUploadedAlready");
            }
            if (_credentials == null)
            {
                _credentials = new GDataCredentials(MainForm.Conf.YouTubeUsername,
                                                   MainForm.Conf.YouTubePassword);
            }
            if (_credentials != null)
            {
                try
                {
                    _youTubeAuthenticator = new ClientLoginAuthenticator("iSpy", ServiceNames.YouTube, _credentials)
                                                {DeveloperKey = MainForm.Conf.YouTubeKey};
                }
                catch
                {
                    _youTubeAuthenticator = null;
                }
            }


            var us = new UserState(objectId, filename, emailOnComplete, message, @public);
            UploadFiles.Enqueue(us);
            if (!_uploading)
                Upload(isTimeLapse);
            return LocRm.GetString("YouTubeMovieAdded");
        }


        private static void OnDone(object sender, AsyncOperationCompletedEventArgs e)
        {
            if (e != null)
            {
                var u = e.UserState as UserState;

                if (u != null)
                {
                    if (e.Cancelled)
                    {
                        //MainForm.LogErrorToFile("YouTube upload cancelled");
                        Console.WriteLine("YouTube upload cancelled");
                    }
                    else if (e.Error != null)
                    {
                        //Console.WriteLine("YouTube upload failed: " + e.Error.Message);
                        MainForm.LogErrorToFile("YouTube upload failed: " + e.Error.Message);
                    }
                    else
                    {
                        ParseAndFinish(u, e.ResponseStream);
                    }
                }
            }
            _uploading = false;
            if (UploadFiles.Count > 0)
                Upload(false);
        }

        //modified by dcristian
        private static void Upload(bool isTimeLapse)
        {
            try
            {
                _uploading = true;
                UserState us = UploadFiles.Dequeue();
                //Console.WriteLine("youtube: upload " + us.AbsoluteFilePath);
                _ru = null;
                _ru = new ResumableUploader(25);
                _ru.AsyncOperationCompleted += OnDone;

                var v = new Video
                            {
                                Title = "iSpy: " + us.CameraData.name + " timelapse="+isTimeLapse,
                                Description = MainForm.Website + " : " + us.CameraData.description
                            };
                if (us.CameraData == null)
                {
                    if (UploadFiles.Count > 0)
                        Upload(false);
                    return;
                }
                v.Keywords = us.CameraData.settings.youtube.tags;
                v.Tags.Add(new MediaCategory(us.CameraData.settings.youtube.category));
                v.Media.Categories.Add(new MediaCategory(us.CameraData.settings.youtube.category));
                v.Private = !us.Ispublic;
                v.Author = "webcam";
                if (us.EmailOnComplete != "")
                    v.Private = false;

                string contentType = MediaFileSource.GetContentTypeForFileName(us.AbsoluteFilePath);

                v.MediaSource = new MediaFileSource(us.AbsoluteFilePath, contentType);

                // add the upload uri to it
                var link =
                    new AtomLink("http://uploads.gdata.youtube.com/resumable/feeds/api/users/" +
                                 MainForm.Conf.YouTubeAccount + "/uploads") { Rel = ResumableUploader.CreateMediaRelation };
                v.YouTubeEntry.Links.Add(link);


                _ru.InsertAsync(_youTubeAuthenticator, v.YouTubeEntry, us);
            }
                //added by dcristian
            catch (Exception ex)
            {
                MainForm.LogMessageToFile("Exception youtube " + ex.Message + ex.StackTrace);
            }
        }

        private static void ParseAndFinish(UserState u, Stream s)
        {
            var ys = new YouTubeRequestSettings("iSpy", MainForm.Conf.YouTubeKey);
            var ytr = new YouTubeRequest(ys);
            Video v = ytr.ParseVideo(s);
            string msg = "YouTube video uploaded: <a href=\"http://www.youtube.com/watch?v=" + v.VideoId + "\">" +
                          v.VideoId + "</a>";
            if (u.Ispublic)
                msg += " (public)";
            else
                msg += " (private)";
            MainForm.LogMessageToFile(msg);
            if (u.EmailOnComplete != "" && u.Ispublic)
            {
                SendYouTubeMails(u.EmailOnComplete, u.Message, v.VideoId);
            }
            //check against most recent uploaded videos
            MainForm.Conf.UploadedVideos += "," + u.AbsoluteFilePath + "|" + v.VideoId;
            if (MainForm.Conf.UploadedVideos.Length > 10000)
                MainForm.Conf.UploadedVideos = "";
        }

        private static void SendYouTubeMails(string addresses, string message, string videoid)
        {
            string[] emails = addresses.Split('|');
            foreach (string email in emails)
            {
                string em = email.Trim();
                if (em.IsValidEmail())
                {
                    string body;
                    if (em != MainForm.EmailAddress)
                    {
                        body = LocRm.GetString("YouTubeShareMailBody").Replace("[USERNAME]",
                                                                                MainForm.Conf.WSUsername);
                        body = body.Replace("[EMAIL]", MainForm.EmailAddress);
                        body = body.Replace("[MESSAGE]", message);
                        body = body.Replace("[INFO]", videoid);
                        MainForm.WSW.SendContent(em,
                                                 LocRm.GetString("YouTubeShareMailSubject").Replace("[EMAIL]",
                                                                                                    MainForm.
                                                                                                        EmailAddress),
                                                 body);
                    }
                    else
                    {
                        body = LocRm.GetString("YouTubeUploadMailBody").Replace("[USERNAME]",
                                                                                 MainForm.Conf.WSUsername);
                        body = body.Replace("[INFO]", videoid);
                        MainForm.WSW.SendContent(em, LocRm.GetString("YouTubeUploadMailSubject"), body);
                    }
                }
            }
        }

        #region Nested type: UserState

        internal class UserState
        {
            private readonly int _objectid;
            public string EmailOnComplete;
            public bool Ispublic;
            public string Message;
            public string Filename;


            internal UserState(int objectId, string filename, string emailOnComplete, string message,
                               bool @public)
            {
                _objectid = objectId;
                CurrentPosition = 0;
                RetryCounter = 0;
                Filename = filename;
                EmailOnComplete = emailOnComplete;
                Message = message;
                Ispublic = @public;
            }

            internal string AbsoluteFilePath
            {
                get
                {
                    return MainForm.Conf.MediaDirectory + "video\\" + CameraData.directory + "\\" +
                           Filename;
                }
            }

            internal objectsCamera CameraData
            {
                get { return MainForm.Cameras.SingleOrDefault(p => p.id == _objectid); }
            }

            internal long CurrentPosition { get; set; }


            internal string Error { get; set; }

            internal int RetryCounter { get; set; }


            internal string HttpVerb { get; set; }

            internal Uri ResumeUri { get; set; }
        }

        #endregion
    }
}