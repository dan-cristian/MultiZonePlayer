using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

//using MediaInfoLib;

namespace MultiZonePlayer
{

    abstract class PlaylistBase
    {
        protected List<String> m_playlistFiles;
        protected List<MediaItem> m_playlistItems;

        public virtual List<String> PlaylistFiles
        {
            get
            {
                return m_playlistFiles;
            }
        }

        public virtual List<MediaItem> PlaylistItems
        {
            get
            {
                return m_playlistItems;
            }
        }

        public void SaveUpdatedItems()
        {
            List<MediaItem> clone = m_playlistItems.ToList();
            foreach (MediaItem ai in clone)
            {
                if (ai.RequireSave)
                    ai.SaveItem();
            }
            
        }

        public int PendingSaveItemsCount()
        {
            return m_playlistItems.Count(m => m.RequireSave);
        }
        /*public void ClearAllLists()
        {
            m_playlistFiles.Clear();
            m_playlistItems.Clear();
        }*/
        /*
        public List<AudioItem> PlaylistItemsByPlayCount
        {
            get
            {
                var list = from value in m_playlistItems
                       where value.MediaType == "audio"
                       orderby value.PlayCount ascending
                       select value;

                return list.ToList<AudioItem>();
            }
        }
        /*
         
        /*
        public void Randomize()
        {
            m_playlistItems.Sort(delegate(AudioItem a1, AudioItem a2) { return a1.RandomId.CompareTo(a2.RandomId); });
        }

        public void Normalize()
        {
            m_playlistItems.Sort(delegate(AudioItem a1, AudioItem a2) { return a1.Index.CompareTo(a2.Index); });
        }
         */
    }

   
    class M3UPlayList : PlaylistBase
    {
        //public array of addresses to be accessed by open dialog and save dialog
        private string[] Final_Adresses = new string[0];
        private Hashtable songList = null;

        public void ReadM3U(String m3uFileName)
        {
            int count = 0;
            if (songList == null)
            {
                songList = new Hashtable();
            }
            else
            {
                songList.Clear();
            }


            //try
            //{
            //checkedListBox1.Items.Clear();
            /*if (Name_only.Checked)
            {
                //Read all bytes to an array To convert it to unicode ,so a Non-english characters will be shown properly 
                byte[] all_bytes = File.ReadAllBytes(openFileDialog1.FileName);

                // Perform the conversion from one encoding to the other.
                byte[] new_encoded = Encoding.Convert(Encoding.Default, Encoding.Unicode, all_bytes);

                //Convert the new encoded array (of bytes) to a normal string
                string uniString = Encoding.Unicode.GetString(new_encoded);

                //Split the string to array by lines
                string[] final_encoded = uniString.Split('\n');

                //String Array To Save all addresses vary with Some conditions in M3U PlayList
                //if the Songs in the Same Directory , the playList will Contain the name only
                //if the Songs in SubDirectory in the same directory of the playlist ,it will contain the directory name\song name
                //else it will contain the full address 
                //We Will use an Array List as it doesn't has an initial length
                System.Collections.ArrayList address = new System.Collections.ArrayList();

                //Read the M3u File from this array
                for (int x = 2; x < final_encoded.Length; x = x + 2)
                {
                    //the song is in other Directory
                    if (final_encoded[x].Contains(":"))
                    {
                        //removing the last character "\r" to get the exact path
                        address.Add(final_encoded[x].Remove(final_encoded[x].Length - 1));
                        checkedListBox1.Items.Add(final_encoded[x].Remove(0, final_encoded[x].LastIndexOf('\\') + 1));
                    }
                    //if (final_encoded[x].Contains("\\"))
                    else
                    {
                        string tmpadrs = openFileDialog1.FileName;
                        tmpadrs = tmpadrs.Remove(tmpadrs.LastIndexOf('\\'));
                        tmpadrs = tmpadrs + "\\" + final_encoded[x];
                        address.Add(tmpadrs);
                        checkedListBox1.Items.Add(final_encoded[x].Remove(0, final_encoded[x].LastIndexOf('\\') + 1));
                    }
                }
                //Converting the array list to a normal array of strings to the public array
                Final_Adresses = (string[])address.ToArray(typeof(string));
            }
             * */
            //if (Full.Checked)
            //{
            //Read all bytes to an array To convert it to unicode ,so a Non-english characters will be shown properly 
            byte[] all_bytes = File.ReadAllBytes(m3uFileName);

            // Perform the conversion from one encoding to the other.
            byte[] new_encoded = Encoding.Convert(Encoding.Default, Encoding.Unicode, all_bytes);

            //Convert the new encoded array (of bytes) to a normal string
            string uniString = Encoding.Unicode.GetString(new_encoded);

            //Split the string to array by lines
            string[] final_encoded = uniString.Split('\n');

            //String Array To Save all addresses vary with Some conditions in M3U PlayList
            //if the Songs in the Same Directory , the playList will Contain the name only
            //if the Songs in SubDirectory in the same directory of the playlist ,it will contain the directory name\song name
            //else it will contain the full address 
            //We Will use an Array List as it doesn't has an initial length
            //System.Collections.ArrayList address = new System.Collections.ArrayList();

            //Read the M3u File from this array
            for (int x = 2; x < final_encoded.Length; x++)
            {
                //the song is in other Directory
                if (!final_encoded[x].StartsWith("#") && !final_encoded[x].StartsWith("\r") && !(final_encoded[x].Length == 0))
                {
                    if (final_encoded[x].Contains(":") || final_encoded[x].StartsWith("\\"))
                    {
                        //removing the last 2 characters "\r" to get the exact path

                        songList.Add(count.ToString(), final_encoded[x].Remove(final_encoded[x].Length - 1));
                        count++;
                    }
                    else
                    {
                        string tmpadrs = (String)m3uFileName.Clone();
                        tmpadrs = tmpadrs.Remove(tmpadrs.LastIndexOf('\\'));
                        if (final_encoded[x].Length > 0)
                            tmpadrs = tmpadrs + "\\" + final_encoded[x].Remove(final_encoded[x].Length - 1);
                        else
                            tmpadrs = tmpadrs + "\\" + final_encoded[x];

                        songList.Add(count.ToString(), tmpadrs);
                        count++;
                    }
                }
            }
            //Converting the array list to a normal array of strings to the public array
            //Final_Adresses = (string[])address.ToArray(typeof(string));
            //}
            //}
            //catch (Exception err) { MessageBox.Show(err.Message); }
        }

        public Hashtable GetAllFileList_()
        {
            return songList;
        }
    }

    public abstract class MediaItem
    {
        public String Genre = "";
        public String Author = "";
        public String ContributingArtist = "";
        public String Year = "";
        public String Album = "";
        public String Title = "";
        public int PlayCount = 0;
        public int Rating = -1;
        public String MediaType = "";
        public String SourceURL = "";
        public String SourceContainerURL = "";
        private static Random rndSeed = new Random();
        private static int index = 0;
        public int RandomId = rndSeed.Next();
        public int Index = index++;
        protected bool m_requireSave = false;
        public DateTime Created;
        public String Comment;
        public DateTime LibraryAddDate;

        public abstract bool RetrieveMediaItemValues();
        
        public virtual int UpdateRating(int step, int minvalue, int maxvalue)
        {
            Rating = Math.Max(minvalue, Rating + step);
            Rating = Math.Min(Rating, maxvalue);
            m_requireSave = true;
            return Rating;
        }

        public virtual void SetRating(int level)
        {
            Rating = level;
            m_requireSave = true;
        }
        
        public virtual void IncreasePlayCount()
        {
            PlayCount++;
            m_requireSave = true;
        }

        public void SetLibraryAddedComment()
        {
            Comment = Comment + IniFile.MEDIA_TAG_LIBRARY_ID + DateTime.Now.ToString("yyyy-MM-dd");
            m_requireSave = true;
        }

        public virtual void SetPlayCount(int count)
        {
            PlayCount = count;
            m_requireSave = true;
        }

        public bool RequireSave
        {
            get { return m_requireSave; }
        }

        public abstract void SaveItem();

        public virtual bool IsThatNew(int ageinweeks)
        {
            DateTime dt = DateTime.Now.Subtract(new TimeSpan(ageinweeks*7,0,0,0));
            return LibraryAddDate.CompareTo(dt) > 0;
        }
    }

    class MusicItem:MediaItem
    {/*
        public const String keyRating = "rating";
        public const String keyGenre = "Genre";
        public const String keyPlayCount = "userplaycount";
        public const String keyAuthor = "Album/Performer";
        public const String keyFormat = "Format";
      * */
        private TagLib.File m_tagFile = null;

        public MusicItem(String url, String folder)
        {
            SourceURL = url;
            SourceContainerURL = folder;
        }

        public override bool RetrieveMediaItemValues()
        {
            int p_rating = 0, p_playcount = 0;
            bool result = false;
            try
            {
                TagLib.File tg = TagLib.File.Create(SourceURL as String);
                for (int i = 0; i < IniFile.MUSIC_EXTENSION.Length; i++)
                {
                    if (tg.MimeType.Contains(IniFile.MUSIC_EXTENSION[i]))
                    {
                        Banshee.Streaming.StreamRatingTagger.GetRatingAndPlayCount(tg, ref p_rating, ref p_playcount);
                        Title = tg.Tag.Title;
                        if (Title== null) Title = SourceURL.Substring(Math.Max(0, SourceURL.Length - 35)).Replace("\\", "/");
                        Title = Utilities.SanitiseInternational(Title);
                        Genre = Utilities.ToTitleCase(tg.Tag.FirstGenre);
                        Genre = Utilities.SanitiseInternational(Genre);
                        if (Genre == null) Genre = "Not Set";
                        PlayCount = p_playcount;
                        Rating = p_rating;
                        Author = Utilities.SanitiseInternational(Utilities.ToTitleCase(tg.Tag.FirstAlbumArtist));
                        if ((Author == "") || Author.ToLower().Contains("various") || Author.ToLower().Contains("unknown"))
                        {
                            Author = Utilities.SanitiseInternational(Utilities.ToTitleCase(tg.Tag.FirstPerformer));
                            if ((Author == "") || Author.ToLower().Contains("various") || Author.ToLower().Contains("unknown"))
                            {
                                Author = Utilities.SanitiseInternational(Utilities.ToTitleCase(tg.Tag.FirstComposer));
                            }
                            //MLog.Log(null, tg.ToString());
                        }
                        if (Author == "") Author = "Not Set";
                        
                        MediaType = "audio";
                        Created = Utilities.GetFileInfo(SourceURL).CreationTime;
                        Comment = tg.Tag.Comment == null ? "" : tg.Tag.Comment;
                        if (!Comment.Contains(IniFile.MEDIA_TAG_LIBRARY_ID))
                        {
                            //first added to library, reset some fields
                            this.SetRating(0);
                            this.SetPlayCount(0);
                            this.SetLibraryAddedComment();
                            LibraryAddDate = Created;
                        }
                        else
                        {
                            String librarydate = Comment.Substring(Comment.IndexOf(IniFile.MEDIA_TAG_LIBRARY_ID)
                                +IniFile.MEDIA_TAG_LIBRARY_ID.Length, "yyyy-mm-dd".Length);
                            LibraryAddDate = DateTime.Parse(librarydate);
                        }
                        result = true;
                    }
                    
                }
                /*
                for (int i = 0; i < IniFile.VIDEO_EXTENSION.Length; i++)
                {
                    if (tg.MimeType.Contains(IniFile.VIDEO_EXTENSION[i]))
                    {
                        MediaType = "video";
                        result = true;
                    }
                }*/
            }
            catch (Exception )
            {
                //MLog.Log(ex, "Unable to read tag for file "+ SourceURL);
            }
            return result;
        }

        

        public override void SaveItem()
        {
            m_tagFile = TagLib.File.Create(SourceURL);
            Banshee.Streaming.StreamRatingTagger.StoreRatingAndPlayCount(Rating, PlayCount, m_tagFile);
            m_tagFile.Tag.Comment = Comment;

            try
            {
                m_tagFile.Save();
                m_requireSave = false;
            }
            catch (TagLib.CorruptFileException)
            {
                m_requireSave = false;
                MLog.Log(this, "Corrupt file, not saving, " + SourceURL);
            }
            catch (IOException)
            {
                m_requireSave = true;
                //MLog.Log(this, "Unable to save tag for " + SourceURL);
            }
        }


    }


    public class VideoItem:MediaItem
    {
        //public String SourceURL;
        public String ImdbYear = "";
        public String ImdbGenre = "";
        public String ImdbTitle;
        public String ImdbId = "";
        //public String Year = "";
        public String ImdbDescription = "";
        public String ImdbDirector = "";
        public String Seen = "";
        public String ImdbImageURL = "";
        //public String Genre = "";
        public String ImdbRating = "";
        public String Resolution = "";
        public String ImdbActors = "";
        //public bool HasChanged = false;

        public VideoItem()
        {
        }

        public VideoItem(String p_sourceURL)
        {
            SourceURL = p_sourceURL;
        }

        public override bool RetrieveMediaItemValues()
        {
            if (SourceURL != "")
            {
                String fileFolder;
                fileFolder = Directory.GetParent(SourceURL).FullName;
                String infoPath = fileFolder + IniFile.VIDEO_INFO_FILE;

                if (File.Exists(infoPath))
                {
                    ImdbId = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_IMDBID, infoPath);
                    ImdbTitle = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_NAME, infoPath);
                    ImdbYear = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_YEAR, infoPath);
                    ImdbGenre = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_GENRE, infoPath);
                    ImdbDirector = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_DIRECTOR, infoPath);
                    Seen = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_SEEN, infoPath);
                    ImdbActors = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_ACTORS, infoPath);
                    ImdbImageURL = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_IMAGEURL, infoPath);
                    ImdbDescription = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_DESCRIPTION, infoPath);
                    ImdbRating = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_RATING, infoPath);
                    //result.Resolution = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_RESOLUTION, infoPath);
                    switch (Path.GetExtension(SourceURL))
                    {
                        case ".mkv":
                            Resolution = "1080p";
                            break;
                        case ".avi":
                            Resolution = "DVD";
                            break;
                        default:
                            Resolution = "";
                            break;
                    }

                    if ((ImdbActors == "") && (ImdbDescription != ""))
                    {
                        ImdbActors = IMDBParser.GetImdbActorsFromDescription(ImdbDescription);
                    }
                }
                else
                {
                    ImdbTitle = SourceURL.Substring(SourceURL.LastIndexOf("\\") + 1);
                }

                return true;
            }
            else
            {
                MLog.Log(this,"Error no source url set for videoitem");
                return false;
            }
        }

       

        private static void SaveVideoInfo(VideoItem videoInfo)
        {
            if (videoInfo == null) return;

            String fileFolder = Directory.GetParent(videoInfo.SourceURL).FullName;
            String infoPath = fileFolder + IniFile.VIDEO_INFO_FILE;

            String content = "[" + IniFile.VIDEO_INFO_INI_SECTION + "]" + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_NAME + "=" + videoInfo.ImdbTitle + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_IMDBID + "=" + videoInfo.ImdbId + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_YEAR + "=" + videoInfo.ImdbYear + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_GENRE + "=" + videoInfo.ImdbGenre + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_DIRECTOR + "=" + videoInfo.ImdbDirector + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_SEEN + "=" + videoInfo.Seen + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_ACTORS + "=" + videoInfo.ImdbActors + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_IMAGEURL + "=" + videoInfo.ImdbImageURL + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_RATING + "=" + videoInfo.ImdbRating + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_RESOLUTION + "=" + videoInfo.Resolution + Environment.NewLine;
            content += IniFile.VIDEO_INFO_INI_DESCRIPTION + "=" + videoInfo.ImdbDescription + Environment.NewLine;
            File.WriteAllText(infoPath, content);
            //videoInfo.HasChanged = false;
        }

        public override void SaveItem()
        {
            SaveVideoInfo(this);
        }
    }

    class DirectoryPlaylist : PlaylistBase
    {
        public DirectoryPlaylist()
        {
        }

        //ext in format *.ext
        public DirectoryPlaylist(String directory, String ext, SearchOption search)
        {
            AddFiles(directory, ext, search);
        }

        public void AddFiles(String directory, String ext, SearchOption search)
        {
            try
            {
                if (m_playlistFiles == null) m_playlistFiles = new List<String>();
                if (m_playlistItems == null) m_playlistItems = new List<MediaItem>();

                DirectoryInfo di = new DirectoryInfo(directory);
                FileInfo[] rgFiles = di.GetFiles(ext, search);
                MusicItem item;
                MLog.Log(this, "Adding files count="+rgFiles.Length);

                foreach (FileInfo fi in rgFiles)
                {
                    if (!m_playlistFiles.Contains(fi.FullName))
                    {
                        m_playlistFiles.Add(fi.FullName);
                        item = new MusicItem(fi.FullName,fi.DirectoryName);
                        item.RetrieveMediaItemValues();
                        m_playlistItems.Add(item);
                    }
                    Application.DoEvents();
                    if (MZPState.Instance == null)
                    {
                        MLog.Log(this, "Abording loading files, program is exiting");
                        break;
                    }
                }
                MLog.Log(this, "Adding files completed");
            }
            catch (Exception ex)
            {
                MLog.Log(null, "Unable to add files from " + directory + " err=" + ex.Message);
            }
        }


    }

    class WindowsMediaItem:MediaItem
    {
        public const String keyGenre = "wm/genre";
        public const String keyAuthor = "author";
        public const String keyYear = "wm/year";
        public const String keyAlbum = "wm/albumtitle";
        public const String keyTitle = "title";
        public const String keyPlayCount = "userplaycount";
        public const String keyRating = "userrating";
        public const String keyPlayCountAfternoon = "userplaycountafternoon";
        public const String keyPlayCountEvening = "userplaycountevening";
        public const String keyPlayCountMorning = "userplaycountmorning";
        public const String keyPlayCountNight = "userplaycountnight";
        public const String keyPlayCountWeekday = "userplaycountweekday";
        public const String keyPlayCountWeekend = "userplaycountweekend";
        public const String keySourceURL = "sourceurl";
        public const String keyAcquisitionDate = "acquisitiontimeyearmonthday";
        public const String keyProviderStyle = "wm/providerstyle";
        public const String keyTrackingId = "trackingid";
        public const String keyMediaType = "mediatype";


        public String PlayCountAfternoon = "";
        public String PlayCountEvening = "";
        public String PlayCountMorning = "";
        public String PlayCountNight = "";
        public String PlayCountWeekday = "";
        public String PlayCountWeekend = "";
        
        public String AcquisitionDate = "";
        public String ProviderStyle = "";
        public String TrackingId = "";
        
        private WMPLib.IWMPMedia m_WMPMedia = null;

        public WindowsMediaItem(WMPLib.IWMPMedia md)
        {
            m_WMPMedia = md;
        }

        public override bool RetrieveMediaItemValues()
        {
            String value;
            bool result=true;
            
            SourceURL = m_WMPMedia.sourceURL;

            value = m_WMPMedia.getItemInfo(keyAcquisitionDate).ToLower();
            if (value != null) AcquisitionDate = value;
            value = m_WMPMedia.getItemInfo(keyAlbum).ToLower();
            if (value != null) Album = value;
            value = m_WMPMedia.getItemInfo(keyAuthor).ToLower();
            if (value != null) Author = value;
            value = m_WMPMedia.getItemInfo(keyGenre).ToLower();
            if (value != null) Genre = value;
            value = m_WMPMedia.getItemInfo(keyMediaType).ToLower();
            if (value != null) MediaType = value;
            value = m_WMPMedia.getItemInfo(keyPlayCount).ToLower();
            if (value != null) PlayCount = Convert.ToInt32(value);
            value = m_WMPMedia.getItemInfo(keyTrackingId).ToLower();
            if (value != null) TrackingId = value;

            
            return result;
        }

        public override int UpdateRating(int step, int minvalue, int maxvalue)
        {
            int val;
            String sval = m_WMPMedia.getItemInfo(WindowsMediaItem.keyRating);
            MLog.Log(this, "WinMediaItemValue before=" + sval + " for item=" + m_WMPMedia.sourceURL + " key=" + WindowsMediaItem.keyRating);
            if (sval == "") sval = "0";
            val = Math.Max(minvalue, Convert.ToInt16(sval) + step);
            val = Math.Min(val, maxvalue);
            m_WMPMedia.setItemInfo(WindowsMediaItem.keyRating, val.ToString());
            MLog.Log(this, "WinMediaItemValue after=" + m_WMPMedia.getItemInfo(WindowsMediaItem.keyRating));
            m_requireSave = false;
            return val;
        }

        public override void IncreasePlayCount()
        {
            int val;
            String sval = m_WMPMedia.getItemInfo(WindowsMediaItem.keyPlayCount);
            if (sval == "") sval = "0";
            val = Math.Max(0, Convert.ToInt16(sval) + 1);
            m_WMPMedia.setItemInfo(WindowsMediaItem.keyPlayCount, val.ToString());
            m_requireSave = false;
        }

        public override void SaveItem()
        {
        }
    }

    class WindowsMediaPlayList : PlaylistBase
    {
        private AxWMPLib.AxWindowsMediaPlayer m_wmp;
        private Dictionary<String, MediaItem> m_fileList;

        public WindowsMediaPlayList()
        {
            //wmp.openPlayer(
            m_wmp = new AxWMPLib.AxWindowsMediaPlayer();
            m_wmp.CreateControl();
            //m_wmp.settings.enableErrorDialogs = false;
            RetrieveAllFileList();
        }

        

        public Hashtable GetMusicPlaylistByName(String playlistName)
        {
            Hashtable result = new Hashtable();
            WMPLib.IWMPPlaylistArray pla;
            WMPLib.IWMPPlaylist pl;
            WMPLib.IWMPMedia med;
            MediaItem inf;

            pla = m_wmp.playlistCollection.getByName(playlistName);

            //duplicate playlist names are allowed
            for (int i = 0; i < pla.count; i++)
            {
                pl = pla.Item(i);
                //rtxt.AppendText("Playname=" + pl.name + " count=" + pl.count + "\n");
                for (int j = 0; j < pl.count; j++)
                {
                    med = pl.get_Item(j);
                    //inf = new WindowsMediaItem();
                    //inf.GetAMediaItem(med);

                    inf = new MusicItem(med.sourceURL, med.sourceURL);
                    inf.RetrieveMediaItemValues();

                    
                    result.Add(i.ToString()+j, inf);
                }
            }

            return result;
        }

        private void RetrieveAllFileList()
        {
            MLog.Log(null,"Retrieving all file list");
            WMPLib.IWMPPlaylist pl=null;
            WMPLib.IWMPPlaylistArray pla=null;
            WMPLib.IWMPMedia med=null;
            MediaItem inf;
            m_fileList = new Dictionary<string, MediaItem>();

            try
            {
                pla = m_wmp.playlistCollection.getAll();
                MLog.Log(null,"Loading playlists count=" + pla.count);
                for (int i = 0; i < pla.count; i++)
                {
                    try
                    {
                        pl = pla.Item(i);
                        MLog.Log(null,"Loading playlist filecount=" + pl.count + " name=" + pl.name);
                        for (int j = 0; j < pl.count; j++)
                        {
                            try
                            {
                                med = pl.get_Item(j);
                                //inf = new WindowsMediaItem();
                                //inf.GetAMediaItem(med);
                                inf = new MusicItem(med.sourceURL, med.sourceURL);
                                if (inf.RetrieveMediaItemValues())
                                {
                                    if (!m_fileList.ContainsKey(inf.SourceURL))
                                        m_fileList.Add(inf.SourceURL, inf);
                                }
                            }
                            catch (Exception )
                            {
                                //MLog.Log(null,"Err retrieving files index=" + j 
                                  //  + " Err=" + xe.Message + " stack=" + xe.StackTrace);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        MLog.Log(null,"Err retrieving playlist files index="+i
                            +" Err=" + e.Message + " stack="+ e.StackTrace);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Err retrieving all wmp files " + ex.Message + ex.StackTrace);
            }
            MLog.Log(null,"Retrieving all file list completed, loaded filecount="+m_fileList.Count);
        }

        /*public IEnumerable<String> GetMusicPlaylistByCriteria(String genre)
        {
            var result = from value in RetrieveAllFileList().Values
                         where value.Genre == genre && value.MediaType == "audio"
                         orderby value.Rating descending
                         select value.SourceURL;

            return result;
        }*/

        // Attribute1=value;Attr2=value;...
        public List<MediaItem> GetMusicPlaylistByCriteria(String searchCriteria)
        {
            MLog.Log(null,"Get files by criteria=" + searchCriteria);
            Hashtable attribs = Utilities.ParseStringForKeyAndValue(searchCriteria, ';');

            String genre = attribs["genre"] as String;
            String year = attribs["year"] as String;
            String author = attribs["author"] as String;
            String rating = attribs["rating"] as String;
            String album = attribs["album"] as String;
            String ageinweeks = attribs["ageinweeks"] as String;

            var songs = from value in m_fileList.Values
                        where ((genre == null) || value.Genre.Contains(genre))
                           && (year == null || value.Year.Contains(year))
                           && ((author == null) || value.Author.Contains(author))
                           && ((rating == null) || value.Rating.Equals(rating))
                           && ((album == null) || value.Album.Contains(album))
                           && ((ageinweeks == null) || value.IsThatNew(Convert.ToInt16(ageinweeks)))
                           && value.MediaType == "audio"
                        orderby value.Rating descending
                        select value;//.SourceURL;
            /*int i = 0;
            foreach (String s in songs)
            {
                if (!result.Contains(s))
                {
                    result.Add(i.ToString(),s);
                    i++;
                }
            }
             * */
            MLog.Log(null,"Get files by criteria returns count=" + songs.Count());
            return songs.ToList<MediaItem>();
        }

        public void MediaItemValueChange(String sourceURL, String keyname, int step, int minvalue, int maxvalue)
        {
            try
            {
                MediaItem med = m_fileList[sourceURL];
                if (med != null)
                {
                    if (keyname==WindowsMediaItem.keyRating)
                        med.UpdateRating(step, minvalue, maxvalue);
                    if (keyname == WindowsMediaItem.keyPlayCount)
                        med.IncreasePlayCount();
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Unable to increase MediaItemValueIncrease for " + sourceURL 
                    + " key=" + keyname + "err="+ex.Message);
            }
        }

        public MediaItem GetAudioItem(String sourceURL)
        {
            return m_fileList[sourceURL];
        }
    }


    public class VideoCollection
    {
        private DirectoryPlaylist m_playlist;
        private List<VideoItem> m_videoPlayList;
        private Dictionary<String, int> m_actorNames;
        private Dictionary<String, int> m_GenreList;

     
        public VideoCollection(String path)
        {
            m_actorNames = new Dictionary<String, int>();
            m_GenreList = new Dictionary<String, int>();
            LoadVideos(path);
        }

        public List<string> PlaylistFiles
        {
            get
            {
                //if (m_musicFiles == null)
                //    InitMusic();
                return m_playlist.PlaylistFiles;
            }
        }
        public List<VideoItem> GetVideoCollection()
        {
            return m_videoPlayList;
        }

        public ICollection GetActorNames()
        {
            return m_actorNames.Keys;
        }

        private void LoadVideos(String path)
        {
            m_playlist = new DirectoryPlaylist();
            //m_playlist.ClearAllLists();
            m_videoPlayList = new List<VideoItem>();

            for (int i = 0; i < IniFile.VIDEO_EXTENSION.Length; i++)
            {
                m_playlist.AddFiles(path, "*." + IniFile.VIDEO_EXTENSION[i], System.IO.SearchOption.AllDirectories);
            }

            VideoItem vidInfo;
            String actors, actorName;

            foreach (String file in m_playlist.PlaylistFiles)
            {
                //filePath = m_playlist.GetAllFileList()[file].ToString();
                vidInfo = new VideoItem(file);
                vidInfo.RetrieveMediaItemValues();
                m_videoPlayList.Add(vidInfo);
                //parse actors
                actors = vidInfo.ImdbActors;
                int lastIndex = 0;
                for (int i = 0; i < actors.Length; i++)
                {
                    if ((actors[i] == ',') || (i == actors.Length - 1))
                    {
                        actorName = actors.Substring(lastIndex, i - lastIndex);
                        if (!m_actorNames.Keys.Contains(actorName))
                            m_actorNames.Add(actorName, 1);
                        else
                            m_actorNames[actorName] = m_actorNames[actorName] + 1;
                        lastIndex = i + 1;
                    }
                }

                //parse genres
                List<Object> genres;
                genres = Utilities.ParseStringForValues(vidInfo.ImdbGenre, ' ', typeof(String));
                String genre;
                foreach (Object obj in genres)
                {
                    genre = (String)obj;
                    if (!m_GenreList.Keys.Contains(genre))
                        m_GenreList.Add(genre, 1);
                    else
                        m_GenreList[genre] = m_GenreList[genre] + 1;
                }
            }
        }

        public IOrderedEnumerable<String> GetActorsByAppearance(int minimuAppeareanceCount)
        {
            var actors = from key in m_actorNames.Keys
                         where m_actorNames[key] >= minimuAppeareanceCount
                         orderby m_actorNames[key] + key descending
                         select key;
            return actors;
        }

        public IOrderedEnumerable<String> GetGenresByAppearance(int minimuAppeareanceCount)
        {
            var genres = from key in m_GenreList.Keys
                         where m_GenreList[key] >= minimuAppeareanceCount
                         orderby m_GenreList[key] + key descending
                         select key;
            return genres;
        }

        public VideoItem GetVideoInfo(String filePath)
        {
            return m_videoPlayList.Find(delegate(VideoItem info) { return info.SourceURL == filePath; });
        }
    }

    class MediaLibrary
    {
        private static DirectoryPlaylist m_musicFiles;
        private static VideoCollection m_videoFiles;

        public static void Initialise()
        {
                InitMusic();
                InitVideo();
                SaveUpdatedItems();//save new items found in library
        }

        public static bool IsInitialised
        {
            get
            {
                return (m_musicFiles != null) && (m_videoFiles != null);
            }
        }
        public static DirectoryPlaylist AllAudioFiles
        {
            get
            {
                //if (m_musicFiles == null)
                //    InitMusic();
                return m_musicFiles;
            }
        }

        public static VideoCollection AllVideoFiles
        {
            get
            {
                //if (m_videoFiles == null)
                //  InitVideo();
                return m_videoFiles;
            }
        }

        public static List<MediaItem> AudioFilesByGenre(Metadata.ValueList vals)
        {
            var items = m_musicFiles.PlaylistItems.FindAll(delegate(MediaItem item)
                { 
                    return vals.ContainsIndexValue(item.Genre);
                });
                //GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
            return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
        }

        public static List<MediaItem> AudioFilesByArtist(Metadata.ValueList vals)
        {
            var items = m_musicFiles.PlaylistItems.FindAll(delegate(MediaItem item)
            {
                return vals.ContainsIndexValue(item.Author);
            });
            //GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
            return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
        }

        public static List<MediaItem> AudioFilesByArtist(String artist)
        {
            var items = m_musicFiles.PlaylistItems.FindAll(delegate(MediaItem item)
            {
                return artist.Equals(item.Author);
            });
            return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
        }

        public static List<MediaItem> AudioFilesByAlbum(String album)
        {
            var items = m_musicFiles.PlaylistItems.FindAll(delegate(MediaItem item)
            {
                return album.Equals(item.Album);
            });
            return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
        }

        public static List<MediaItem> AudioFilesByGenre(String genre)
        {
            var items = m_musicFiles.PlaylistItems.FindAll(delegate(MediaItem item)
            {
                return genre.Equals(item.Genre);
            });
            return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
        }

        public static List<MediaItem> AudioFilesByFolder(String sourceContainerURL)
        {
            var items = m_musicFiles.PlaylistItems.FindAll(delegate(MediaItem item)
            {
                return sourceContainerURL.Equals(item.SourceContainerURL);
            });
            return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
        }

        public static List<MediaItem> AudioFilesByRating(int rating)
        {
            var items = m_musicFiles.PlaylistItems.FindAll(delegate(MediaItem item)
            {
                return rating.Equals(item.Rating);
            });
            return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
        }

        
        public static void SaveUpdatedItems()
        {
            m_musicFiles.SaveUpdatedItems();
            
        }

        private static void InitMusic()
        {
            MLog.Log(null, "Loading mediaplayer music files from " + IniFile.PARAM_MUSIC_STORE_ROOT_PATH[1]);
            m_musicFiles = new DirectoryPlaylist();
            
            for (int i = 0; i < IniFile.MUSIC_EXTENSION.Length; i++)
            {
                Application.DoEvents();
                m_musicFiles.AddFiles(IniFile.PARAM_MUSIC_STORE_ROOT_PATH[1], "*."+IniFile.MUSIC_EXTENSION[i], 
                    System.IO.SearchOption.AllDirectories);
            }
        }

        private static void InitVideo()
        {
            MLog.Log(null, "Loading mediaplayer video files from " + IniFile.PARAM_VIDEO_STORE_ROOT_PATH[1]);
            m_videoFiles = new VideoCollection(IniFile.PARAM_VIDEO_STORE_ROOT_PATH[1]);
        }

        public static Metadata.ValueList MusicGenres
        {
            get
            {
                //var unique = m_musicFiles.PlaylistItems.GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
                var unique = m_musicFiles.PlaylistItems.Select(i => i.Genre).Distinct().ToList();
                Metadata.ValueList val = new Metadata.ValueList(Metadata.CommandSources.Internal);
                unique.Sort();
                val.SetIndexValues(unique.ToList());
                return val;
            }
        }

        public static Metadata.ValueList MusicArtists
        {
            get
            {
                //var unique = m_musicFiles.PlaylistItems.GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
                var unique = m_musicFiles.PlaylistItems.Select(i => i.Author).Distinct().ToList();
                unique.Sort();

                Metadata.ValueList val = new Metadata.ValueList(Metadata.CommandSources.Internal);
                val.SetIndexValues(unique.ToList());
                return val;
            }
        }

        public static List<MediaItem> GetMoodPlaylist(MoodMusic mood)
        {
            List<MediaItem> result = null;
            if (mood != null)
            {
                try
                {
                    IEnumerable<MediaItem> query, res1, res2, res3, res4;
                    bool exclude;
                    String value;

                    res1 = null;
                    foreach (String val in mood.Genres)
                    {
                        exclude = val.Contains('!');
                        if (exclude)
                        {
                            if (res1 == null) res1 = MediaLibrary.AllAudioFiles.PlaylistItems;
                            value = val.Replace("!", "");
                            res1 = res1.Where(i => !i.Genre.ToLower().Contains(value.ToLower())).ToList();
                            //res1 = res1.Intersect(query).Distinct().ToList();
                        }
                        else
                        {
                            if (res1 == null) res1 = new List<MediaItem>();
                            value = val;
                            query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Genre.ToLower().Contains(value.ToLower())).ToList();
                            res1 = res1.Union(query).Distinct().ToList();
                        }
                    }
                    if (res1 == null) res1 = new List<MediaItem>();

                    res2 = null;
                    foreach (String val in mood.Authors)
                    {
                        exclude = val.Contains('!');
                        if (exclude)
                        {
                            if (res2 == null) res2 = MediaLibrary.AllAudioFiles.PlaylistItems;
                            value = val.Replace("!", "");
                            res2 = res2.Where(i => !i.Author.ToLower().Contains(value.ToLower())).ToList();

                        }
                        else
                        {
                            if (res2 == null) res2 = new List<MediaItem>();
                            value = val;
                            query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Author.ToLower().Contains(value.ToLower())).ToList();
                            res2 = res2.Union(query).Distinct().ToList();
                        }
                    }
                    if (res2 == null) res2 = new List<MediaItem>();

                    res3 = new List<MediaItem>();
                    foreach (int val in mood.Ratings)
                    {
                        query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Rating.Equals(val)).ToList();
                        res3 = res3.Union(query).Distinct().ToList();
                    }

                    res4 = new List<MediaItem>();
                    foreach (int val in mood.AgeInWeeks)
                    {
                        query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.IsThatNew(val)).ToList();
                        res4 = res4.Union(query).Distinct().ToList();
                    }

                    switch (mood.LogicalSearchOperator)
                    {
                        case MoodMusic.LogicalSearchOperatorEnum.Intersect:
                            if (res1.Count() + res2.Count() + res3.Count() + res4.Count() != 0)
                            {
                                if (res1.Count() == 0) res1 = MediaLibrary.AllAudioFiles.PlaylistItems;
                                if (res2.Count() == 0) res2 = MediaLibrary.AllAudioFiles.PlaylistItems;
                                if (res3.Count() == 0) res3 = MediaLibrary.AllAudioFiles.PlaylistItems;
                                if (res4.Count() == 0) res4 = MediaLibrary.AllAudioFiles.PlaylistItems;
                                result = res1.Intersect(res2).Intersect(res3).Intersect(res4).ToList();
                            }
                            else
                                MLog.Log(null, "No songs matching criteria mood=" + mood.Name);
                            break;
                        case MoodMusic.LogicalSearchOperatorEnum.Union:
                            result = res1.Union(res2).Union(res3).Union(res4).ToList();
                            break;
                        
                    }

                    if (result!=null && mood.IsGroupByTop)
                    {
                        IEnumerable<MediaItem> list = new List<MediaItem>();
                        var items = result.OrderBy(y => y.PlayCount).ThenBy(z => z.RandomId).GroupBy(x => x.Author).Distinct();
                        foreach (var item in items)
                        {
                            list = list.Union(item.Take(3));
                        }
                        result = list.ToList();
                    }
                    
                }
                catch (Exception ex)
                {
                    MLog.Log(ex, "Error get mood=" + mood);
                }
            }
            else
                MLog.Log(null, "NULL mood unexpected");
            return result;
        }
    }
}
