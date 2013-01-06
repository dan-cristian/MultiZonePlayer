﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace MultiZonePlayer
{

	public abstract class PlaylistBase
	{
		protected List<String> m_playlistFiles;

		public PlaylistBase()
		{
			m_playlistFiles = new List<String>();
		}

		public List<String> PlaylistFiles
		{
			get	{return m_playlistFiles;}
		}
		public abstract void SaveUpdatedItems();
		public abstract int PendingSaveItemsCount();
		protected abstract void ProcessFile(FileInfo fi);

		public void AddFiles(String directory, String ext, SearchOption search)
		{
			try
			{
				DirectoryInfo di = new DirectoryInfo(directory);
				FileInfo[] rgFiles = di.GetFiles(ext, search);

				MLog.Log(this, "Adding files count=" + rgFiles.Length);

				//m_playlistFiles.Capacity = rgFiles.Length;
				//m_playlistItems.Capacity = rgFiles.Length;
				//m_artistMetaList.Capacity = rgFiles.Length / 10;

				foreach (FileInfo fi in rgFiles)
				{
					ProcessFile(fi);
					m_playlistFiles.Add(fi.FullName);
					//Application.DoEvents();
					if (MZPState.Instance == null)
					{
						MLog.Log(this, "Aborting loading files, program is exiting");
						break;
					}
				}
				MLog.Log(this, "Adding files completed");
			}
			catch (Exception ex)
			{
				MLog.Log(ex, null, "Unable to add files from " + directory + " err=" + ex.Message);
			}
		}
	}

	/*
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
			 */
	/*
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
*/
	

	public abstract class MediaItemBase
	{
		private static Random rndSeed = new Random();
		private static int index = 0;
		protected static int errorMetaCount = 0;

		public String MediaType = "";
		public String SourceURL = "";
		public String SourceContainerURL = "";
		public int RandomId = rndSeed.Next();
		public String Title = "";
		public int PlayCount = 0;
		public int Rating = -1;
		public int Index = index++;
		protected bool m_requireSave = false;
		public DateTime Created;
		public String Comment;
		public DateTime LibraryAddDate;
		public String Author = "";
		public abstract bool RetrieveMediaItemValues();
		public bool RequireSave
		{
			get { return m_requireSave; }
		}

		public abstract void SaveItem();

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

		public virtual void SetPlayCount(int count)
		{
			PlayCount = count;
			m_requireSave = true;
		}

	}

	public class AudioItem : MediaItemBase
	{
		public String Genre = "";
		public String ContributingArtist = "";
		public String Year = "";
		public String Album = "";
		public Boolean IsFavorite = false;
		public LastFmMeta Meta;
		private TagLib.File m_tagFile = null;

		public AudioItem(String url, String folder, LastFmMeta meta)
		{
			SourceURL = url;
			SourceContainerURL = folder;
			Meta = meta;
		}

		public void SetLibraryAddedComment()
		{
			Comment = Comment + IniFile.MEDIA_TAG_LIBRARY_ID + DateTime.Now.ToString("yyyy-MM-dd");
			m_requireSave = true;
		}

		public void SetFavorite(bool isFavorite)
		{
			string currentIsFavorite;
			if (Comment.Contains(IniFile.MEDIA_TAG_FAVORITE))
			{
				currentIsFavorite = Comment.Substring(Comment.IndexOf(IniFile.MEDIA_TAG_FAVORITE) + IniFile.MEDIA_TAG_FAVORITE.Length, 1);
				Comment = Comment.Replace(IniFile.MEDIA_TAG_FAVORITE + currentIsFavorite, IniFile.MEDIA_TAG_FAVORITE + (isFavorite ? "1" : "0"));
			}
			else
				Comment = Comment + IniFile.MEDIA_TAG_FAVORITE + (isFavorite ? "1" : "0");
			m_requireSave = true;
		}


		public virtual bool IsThatNew(int ageinweeks)
		{
			DateTime dt = DateTime.Now.Subtract(new TimeSpan(ageinweeks * 7, 0, 0, 0));
			return LibraryAddDate.CompareTo(dt) > 0;
		}

		public override bool RetrieveMediaItemValues()
		{
			int p_rating = 0, p_playcount = 0;
			bool result = false;
			try
			{
				TagLib.File tg = TagLib.File.Create(SourceURL as String);
				//for (int i = 0; i < IniFile.MUSIC_EXTENSION.Length; i++)
				//{
				//	if (tg.MimeType.Contains(IniFile.MUSIC_EXTENSION[i]))
				//	{
				Banshee.Streaming.StreamRatingTagger.GetRatingAndPlayCount(tg, ref p_rating, ref p_playcount);
				Title = tg.Tag.Title;
				if (Title == null) Title = SourceURL.Substring(Math.Max(0, SourceURL.Length - 35)).Replace("\\", "/");
				Title = Utilities.SanitiseInternationalAndTrim(Title);
				Genre = Utilities.ToTitleCase(tg.Tag.FirstGenre);
				Genre = Utilities.SanitiseInternationalAndTrim(Genre);
				if (Genre == null) Genre = "Not Set";
				PlayCount = p_playcount;
				Rating = p_rating;
				Author = Utilities.SanitiseInternationalAndTrim(Utilities.ToTitleCase(tg.Tag.FirstAlbumArtist));
				if ((Author == "") || Author.ToLower().Contains("various") || Author.ToLower().Contains("unknown"))
				{
					Author = Utilities.SanitiseInternationalAndTrim(Utilities.ToTitleCase(tg.Tag.FirstPerformer));
					if ((Author == "") || Author.ToLower().Contains("various") || Author.ToLower().Contains("unknown"))
					{
						Author = Utilities.SanitiseInternationalAndTrim(Utilities.ToTitleCase(tg.Tag.FirstComposer));
					}
					//MLog.Log(null, tg.ToString());
				}
				if (Author == "") Author = "Not Set";
				Album = Utilities.SanitiseInternationalAndTrim(tg.Tag.Album);
				Year = tg.Tag.Year.ToString();
				MediaType = "audio";
				Created = Utilities.GetFileInfo(SourceURL).CreationTime;
				Comment = tg.Tag.Comment == null ? "" : tg.Tag.Comment;

				if (Meta != null)
				{
					/*Author = Meta.ArtistName != null ? Meta.ArtistName : Author;
					Album = Meta.Album != null ? Meta.Album : Album;
					Genre = Meta.GenreTags != null && Meta.GenreTags.Length>0? Meta.GenreTags[0]: Genre;
					GenreTags = Meta.GenreTags;
					ArtistOrigin = Meta.ArtistOrigin != null ? Meta.ArtistOrigin : ArtistOrigin;
						*/
				}

				if (!Comment.Contains(IniFile.MEDIA_TAG_LIBRARY_ID))
				{
					//first added to library, reset some fields
					this.SetRating(0);
					this.SetPlayCount(0);
					this.SetLibraryAddedComment();
					this.SetFavorite(false);
					LibraryAddDate = Created;
				}
				else
				{
					String librarydate = Comment.Substring(Comment.IndexOf(IniFile.MEDIA_TAG_LIBRARY_ID)
						+ IniFile.MEDIA_TAG_LIBRARY_ID.Length, "yyyy-mm-dd".Length);
					LibraryAddDate = DateTime.Parse(librarydate);

					if (Comment.Contains(IniFile.MEDIA_TAG_FAVORITE))
						this.IsFavorite = Comment.Substring(Comment.IndexOf(IniFile.MEDIA_TAG_FAVORITE) + IniFile.MEDIA_TAG_FAVORITE.Length, 1) == "1";
					else
						this.SetFavorite(false);
				}
				result = true;
			}
			catch (Exception)
			{
				//MLog.Log(ex, "Unable to read tag for file "+ SourceURL);
				errorMetaCount++;
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
	
	public class MediaImageItem : MediaItemBase
	{
		public string Tags;
		public string CameraModel;

		public MediaImageItem(String url, String folder)
		{
			SourceURL = url;
			SourceContainerURL = folder;
		}

		public override void SaveItem()
		{
			throw new NotImplementedException();
		}

		public override bool RetrieveMediaItemValues()
		{
			bool result = false;
			Goheer.EXIF.EXIFextractor exif;
			try
			{
				exif = new Goheer.EXIF.EXIFextractor(SourceURL, "", "");
				this.Created = Convert.ToDateTime(exif["Date Time"]);
				this.CameraModel = exif["Equip Model"].ToString();
				this.Comment = exif["Image Description"].ToString();
				result = true;
			}
			catch (Exception)
			{
				errorMetaCount++;
			}

			return result;
		}

	}
	public class LastFmMeta
	{
		public string URL;
		public string MainGenre;
		public string ArtistName;
		public List<string> GenreTags;
		public string ArtistOrigin;
		public string Album;
		public List<string> SimilarArtists;

		public LastFmMeta()
		{
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", ArtistName, MainGenre);
		}
	}

	public class VideoItem : MediaItemBase
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
				MLog.Log(this, "Error no source url set for videoitem");
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

	/*
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
*/

	public class VideoCollection : PlaylistBase
	{
		private List<VideoItem> m_videoPlayList;
		private Dictionary<String, int> m_actorNames;
		private Dictionary<String, int> m_GenreList;

		public VideoCollection()
		{
			m_actorNames = new Dictionary<String, int>();
			m_GenreList = new Dictionary<String, int>();
			m_videoPlayList = new List<VideoItem>();
		}

		public List<VideoItem> GetVideoCollection()
		{
			return m_videoPlayList;
		}

		public ICollection GetActorNames()
		{
			return m_actorNames.Keys;
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

		public override void SaveUpdatedItems()
		{
			//throw new NotImplementedException();
		}

		public override int PendingSaveItemsCount()
		{
			return 0;
			//throw new NotImplementedException();
		}

		protected override void ProcessFile(FileInfo fi)
		{
			VideoItem vidInfo;
			String actors, actorName;
			string file = fi.FullName;
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

	public class MusicCollection : PlaylistBase
	{
		private List<LastFmMeta> m_artistMetaList;
		protected List<AudioItem> m_playlistItems;

		public List<LastFmMeta> ArtistMetaList
		{
			get { return m_artistMetaList; }
		}

		public MusicCollection()
			: base()
		{
			m_artistMetaList = new List<LastFmMeta>();
			m_playlistItems = new List<AudioItem>();
		}
		
		public List<AudioItem> PlaylistItems
		{
			get { return m_playlistItems; }
		}

		public override void SaveUpdatedItems()
		{
			if (m_playlistItems != null)
			{
				List<AudioItem> clone = m_playlistItems.ToList();
				foreach (AudioItem ai in clone)
				{
					if (ai.RequireSave) ai.SaveItem();
				}
			}
		}

		public override int PendingSaveItemsCount()
		{
			return m_playlistItems.Count(m => m.RequireSave);
		}

		protected override void ProcessFile(FileInfo fi)
		{
			DirectoryInfo dir;
			AudioItem item;
			LastFmMeta meta, metaDisk, metaDownload;
			String metaText;
			String metaFile;

			if (!m_playlistFiles.Contains(fi.FullName))
			{
				dir = fi.Directory;
				do
				{
					meta = m_artistMetaList.Find(x => x.URL == dir.FullName);
					if (meta == null)
					{
						metaFile = dir.FullName + IniFile.MEDIA_META_FILE_NAME;
						if (File.Exists(metaFile))
						{
							metaText = Utilities.ReadFile(metaFile);
							metaDisk = fastJSON.JSON.Instance.ToObject<LastFmMeta>(metaText);
							if (metaDisk.ArtistName == null)
								metaDisk.ArtistName = dir.Name;
							if (metaDisk.MainGenre == null)
							{
								metaDownload = LastFMService.GetArtistMeta(metaDisk.ArtistName);
								meta = metaDownload;
								meta.URL = dir.FullName;
								metaText = fastJSON.JSON.Instance.ToJSON(meta, false);
								System.IO.File.WriteAllText(metaFile, metaText);
							}
							else
								meta = metaDisk;
							m_artistMetaList.Add(meta);
							break;
						}
						if (dir.Parent != null)
							dir = dir.Parent;
						else
							break;
					}
					else
						break;
				}
				while (true);
				m_playlistFiles.Add(fi.FullName);
				item = new AudioItem(fi.FullName, fi.DirectoryName, meta);
				item.RetrieveMediaItemValues();
				m_playlistItems.Add(item);
			}
		}
	}

	public class PicturesCollection : PlaylistBase
	{
		protected List<MediaImageItem> m_playlistItems;
		private List<MediaImageItem> m_autoIterateItems;
		private DateTime m_autoIterateDate = DateTime.Now;
		private int m_currentPictureIndex;

		public PicturesCollection()
			: base()
		{
			m_playlistItems = new List<MediaImageItem>();
		}

		public override int PendingSaveItemsCount()
		{
			return 0;
			//throw new NotImplementedException();
		}

		public override void SaveUpdatedItems()
		{
			return;
			//throw new NotImplementedException();
		}

		protected override void ProcessFile(FileInfo fi)
		{
			//throw new NotImplementedException();
			MediaImageItem item;

			item = new MediaImageItem(fi.FullName, fi.DirectoryName);
			item.RetrieveMediaItemValues();
			m_playlistItems.Add(item);
		}

		public MediaImageItem CurrentIteratePicture
		{
			get
			{
				if (m_currentPictureIndex < m_playlistItems.Count)
					return m_playlistItems[m_currentPictureIndex];
				else
					return null;
			}
		}
		public string IteratePicture
		{
			get
			{
				DateTime currentDate = DateTime.Now;
				int tries= 0;
				if (m_autoIterateDate.ToString("yyyy-mm-dd") != DateTime.Now.ToString("yyyy-mm-dd"))
					m_autoIterateItems = null;

				if (m_autoIterateItems == null)
				{
					do
					{
						m_autoIterateItems = m_playlistItems.FindAll(x => x.Created.Day == currentDate.Day);
						m_autoIterateDate = DateTime.Now;
						m_currentPictureIndex = -1;
						if (m_autoIterateItems.Count == 0)
							currentDate = currentDate.Subtract(new TimeSpan(1,0,0,0));
						tries++;
					}
					while (m_autoIterateItems.Count==0 || tries < 60);
				}

				string picture;
				if (m_currentPictureIndex < m_playlistItems.Count)
					m_currentPictureIndex++;
				else
					m_currentPictureIndex = 0;

				if (m_playlistItems.Count == 0)
					picture = IniFile.PARAM_PICTURE_STORE_ROOT_PATH[1] + "notfound.jpg";
				picture = m_playlistItems[m_currentPictureIndex].SourceURL;
				return picture;
			}
		}
	}

	public class MediaLibrary
		{
			public static bool IsInitialised = false;
			private static MusicCollection m_musicFiles;
			private static VideoCollection m_videoFiles;
			private static PicturesCollection m_pictureFiles;

			public static void Initialise()
			{
				IsInitialised = false;
				m_musicFiles = new MusicCollection();
				m_videoFiles = new VideoCollection();
				m_pictureFiles = new PicturesCollection();

				MLog.Log(null, "Loading mediaplayer picture files from " + IniFile.PARAM_PICTURE_STORE_ROOT_PATH[1]);
				for (int i = 0; i < IniFile.PICTURE_EXTENSION.Length; i++)
				{
					m_pictureFiles.AddFiles(IniFile.PARAM_PICTURE_STORE_ROOT_PATH[1], "*." + IniFile.PICTURE_EXTENSION[i],
						System.IO.SearchOption.AllDirectories);
				}

				MLog.Log(null, "Loading mediaplayer music files from " + IniFile.PARAM_MUSIC_STORE_ROOT_PATH[1]);
				for (int i = 0; i < IniFile.MUSIC_EXTENSION.Length; i++)
				{
					m_musicFiles.AddFiles(IniFile.PARAM_MUSIC_STORE_ROOT_PATH[1], "*." + IniFile.MUSIC_EXTENSION[i],
						System.IO.SearchOption.AllDirectories);
				}

				MLog.Log(null, "Loading mediaplayer video files from " + IniFile.PARAM_VIDEO_STORE_ROOT_PATH[1]);
				for (int i = 0; i < IniFile.VIDEO_EXTENSION.Length; i++)
				{
					m_videoFiles.AddFiles(IniFile.PARAM_VIDEO_STORE_ROOT_PATH[1], "*." + IniFile.VIDEO_EXTENSION[i], 
						System.IO.SearchOption.AllDirectories);
				}
				
				

				//save new items found in library
				m_musicFiles.SaveUpdatedItems();
				m_videoFiles.SaveUpdatedItems();
				m_pictureFiles.SaveUpdatedItems();

				IsInitialised = true;
			}

			public static MusicCollection AllAudioFiles
			{
				get	{return m_musicFiles;}
			}

			public static VideoCollection AllVideoFiles
			{
				get	{return m_videoFiles;}
			}
			public static PicturesCollection AllPictureFiles
			{
				get { return m_pictureFiles; }
			}
			public static List<AudioItem> AudioFilesByGenre(Metadata.ValueList vals)
			{
				var items = m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item)
				{
					return vals.ContainsIndexValue(item.Genre);
				});
				//GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
				return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
			}

			public static List<AudioItem> AudioFilesByArtist(Metadata.ValueList vals)
			{
				var items = m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item)
				{
					return vals.ContainsIndexValue(item.Author);
				});
				//GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
				return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
			}

			public static List<AudioItem> AudioFilesByArtist(String artist)
			{
				var items = m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item)
				{
					return artist.Equals(item.Author);
				});
				return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
			}

			public static List<AudioItem> AudioFilesByAlbum(String album)
			{
				var items = m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item)
				{
					return album.Equals(item.Album);
				});
				return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
			}

			public static List<AudioItem> AudioFilesByGenre(String genre)
			{
				var items = m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item)
				{
					return genre.Equals(item.Genre);
				});
				return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
			}

			public static List<AudioItem> AudioFilesByFolder(String sourceContainerURL)
			{
				var items = m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item)
				{
					return sourceContainerURL.Equals(item.SourceContainerURL);
				});
				return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
			}

			public static List<AudioItem> AudioFilesByRating(int rating)
			{
				var items = m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item)
				{
					return rating.Equals(item.Rating);
				});
				return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
			}

			public static Metadata.ValueList MusicGenres
			{
				get
				{
					//var unique = m_musicFiles.PlaylistItems.GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
					var unique = m_musicFiles.PlaylistItems.Select(i => i.Genre).Distinct().ToList();
					Metadata.ValueList val = new Metadata.ValueList(Metadata.CommandSources.system);
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

					Metadata.ValueList val = new Metadata.ValueList(Metadata.CommandSources.system);
					val.SetIndexValues(unique.ToList());
					return val;
				}
			}

			public static List<AudioItem> GetMoodPlaylist(MoodMusic mood)
			{
				List<AudioItem> result = null;
				if (mood != null)
				{
					try
					{
						IEnumerable<AudioItem> query, res1, res2, res3, res4;
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
								if (res1 == null) res1 = new List<AudioItem>();
								value = val;
								query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Genre.ToLower().Contains(value.ToLower())).ToList();
								res1 = res1.Union(query).Distinct().ToList();
							}
						}
						if (res1 == null) res1 = new List<AudioItem>();

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
								if (res2 == null) res2 = new List<AudioItem>();
								value = val;
								query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Author.ToLower().Contains(value.ToLower())).ToList();
								res2 = res2.Union(query).Distinct().ToList();
							}
						}
						if (res2 == null) res2 = new List<AudioItem>();

						res3 = new List<AudioItem>();
						foreach (int val in mood.Ratings)
						{
							query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Rating.Equals(val)).ToList();
							res3 = res3.Union(query).Distinct().ToList();
						}

						res4 = new List<AudioItem>();
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

						if (result != null && mood.IsGroupByTop)
						{
							IEnumerable<AudioItem> list = new List<AudioItem>();
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
