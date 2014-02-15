using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using ExifLib;
using System.Threading;
using fastJSON;

namespace MultiZonePlayer {
	public abstract class PlaylistBase {
		public List<FileDetail> m_playlistFiles;

		public PlaylistBase() {
			m_playlistFiles = new List<FileDetail>();
		}

		public List<FileDetail> PlaylistFiles {
			get { return m_playlistFiles; }
		}

		public abstract void SaveUpdatedItems();
		public abstract int PendingSaveItemsCount();
		protected abstract void ProcessFile(FileInfo fi);

		public void AddFiles(String directory, String ext, SearchOption search, Boolean lookInCache) {
			if (Directory.Exists(directory)) {
				try {
					DirectoryInfo di = new DirectoryInfo(directory);
					FileInfo[] rgFiles = di.GetFiles(ext, search);
					int newfiles = 0, updatedfiles = 0;
					FileDetail file;
					MLog.Log(this, "Adding files ext=" + ext + " count=" + rgFiles.Length);

					//m_playlistFiles.Capacity = rgFiles.Length;
					//m_playlistItems.Capacity = rgFiles.Length;
					//m_artistMetaList.Capacity = rgFiles.Length / 10;

					foreach (FileInfo fi in rgFiles) {
						if (lookInCache) {
							file = m_playlistFiles.Find(x => x.FilePath == fi.FullName);
						}
						else {
							file = null;
						}
						//if new file is found on disk vs cached, load it
						if (file == null) {
							newfiles++;
							ProcessFile(fi);
							m_playlistFiles.Add(new FileDetail(fi.FullName, fi.LastWriteTime));
						}
						else {
							//if file was modified reload
							if (file.Modified != fi.LastWriteTime) {
								updatedfiles++;
								ProcessFile(fi);
								file.Modified = fi.LastWriteTime;
							}
						}
						//Application.DoEvents();
						if (MZPState.Instance == null) {
							MLog.Log(this, "Aborting loading media files");
							break;
						}
					}
					MLog.Log(this, "Adding files completed ext=" + ext + " new=" + newfiles + " updated=" + updatedfiles);
				}
				catch (Exception ex) {
					MLog.Log(ex, this, "Error adding files from " + directory + " err=" + ex.Message);
				}
			}
			else {
				MLog.Log(this, "Error, directory " + directory + " does not exist");
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
				//We Will use an Array ValueList as it doesn't has an initial length
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
			//We Will use an Array ValueList as it doesn't has an initial length
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
			//catch (Exception err) { MultiZonePlayer.MLog.Log(this, err.Message); }
		}

		public Hashtable GetAllFileList_()
		{
			return songList;
		}
	}
*/

	public class FileDetail {
		public String FilePath;
		public DateTime Modified;
		//public Boolean NeedsDiskReload = false;

		public FileDetail() {
		}

		public FileDetail(String filePath, DateTime modified) {
			FilePath = filePath;
			Modified = modified;
		}
	}

	public abstract class MediaItemBase {
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
		public DateTime Modified;
		public String Comment;
		public DateTime LibraryAddDate;
		public String Author = "";
		public String Copyright;

		public bool RequireSave {
			get { return m_requireSave; }
		}

		public abstract void SaveItem();

		public virtual bool RetrieveMediaItemValues() {
			FileInfo finfo = Utilities.GetFileInfo(SourceURL);
			Created = finfo.CreationTime;
			Modified = finfo.LastWriteTime;
			return true;
		}

		public virtual int UpdateRating(int step, int minvalue, int maxvalue) {
			Rating = Math.Max(minvalue, Rating + step);
			Rating = Math.Min(Rating, maxvalue);
			m_requireSave = true;
			return Rating;
		}

		public virtual void SetRating(int level) {
			Rating = level;
			m_requireSave = true;
		}

		public virtual void IncreasePlayCount() {
			PlayCount++;
			m_requireSave = true;
		}

		public virtual void SetPlayCount(int count) {
			PlayCount = count;
			m_requireSave = true;
		}
	}

	public class AudioItem : MediaItemBase {
		public String Genre = "";
		public String ContributingArtist = "";
		public String Year = "";
		public String Album = "";
		public Boolean IsFavorite = false;
		public LastFmMeta Meta;
		private TagLib.File m_tagFile = null;
		private Boolean SaveRatingInComment = false;
		private Boolean SavePlayCountInComment = false;

		public AudioItem() {
		}

		public AudioItem(String url, String folder, LastFmMeta meta) {
			SourceURL = url;
			SourceContainerURL = folder;
			Meta = meta;
		}

		public void SetLibraryAddedComment() {
			Comment = Comment + IniFile.MEDIA_TAG_LIBRARY_ID + DateTime.Now.ToString("yyyy-MM-dd");
			m_requireSave = true;
		}

		public void SetFavorite(bool isFavorite) {
			string currentIsFavorite;
			if (Comment.Contains(IniFile.MEDIA_TAG_FAVORITE)) {
				currentIsFavorite =
					Comment.Substring(Comment.IndexOf(IniFile.MEDIA_TAG_FAVORITE) + IniFile.MEDIA_TAG_FAVORITE.Length, 1);
				Comment = Comment.Replace(IniFile.MEDIA_TAG_FAVORITE + currentIsFavorite,
					IniFile.MEDIA_TAG_FAVORITE + (isFavorite ? "1" : "0"));
			}
			else {
				Comment = Comment + IniFile.MEDIA_TAG_FAVORITE + (isFavorite ? "1" : "0");
			}
			m_requireSave = true;
		}

		public virtual bool IsThatNew(int ageinweeks) {
			DateTime dt = DateTime.Now.Subtract(new TimeSpan(ageinweeks*7, 0, 0, 0));
			return LibraryAddDate.CompareTo(dt) > 0;
		}

		public override bool RetrieveMediaItemValues() {
			int p_rating = -1, p_playcount = -1;
			String parameter;
			bool result = false;
			try {
				result = base.RetrieveMediaItemValues();
				TagLib.File tg = TagLib.File.Create(SourceURL as String);
				//for (int i = 0; i < IniFile.MUSIC_EXTENSION.Length; i++)
				//{
				//	if (tg.MimeType.Contains(IniFile.MUSIC_EXTENSION[i]))
				//	{
				Comment = tg.Tag.Comment == null ? "" : tg.Tag.Comment;
				Banshee.Streaming.StreamRatingTagger.GetRatingAndPlayCount(tg, ref p_rating, ref p_playcount);

				if (p_rating == -1) {
					SaveRatingInComment = true;
					if (Comment.Contains(IniFile.MEDIA_TAG_RATING)) {
						parameter = Comment.Substring(IniFile.MEDIA_TAG_RATING, ";");
						if (!int.TryParse(parameter, out p_rating)) {
							p_rating = 0;
							MLog.Log(this, "Error reading rating value from comment=" + Comment + " on file=" + SourceURL);
						}
					}
					else {
						Comment += IniFile.MEDIA_TAG_RATING + "0;";
						this.SetRating(0);
					}
				}
				if (p_playcount == -1) {
					SavePlayCountInComment = true;
					if (Comment.Contains(IniFile.MEDIA_TAG_PLAYCOUNT)) {
						parameter = Comment.Substring(IniFile.MEDIA_TAG_PLAYCOUNT, ";");
						if (!int.TryParse(parameter, out p_playcount)) {
							p_playcount = 0;
							MLog.Log(this, "Error reading PlayCount value from comment=" + Comment + " on file=" + SourceURL);
						}
					}
					else {
						Comment += IniFile.MEDIA_TAG_PLAYCOUNT + "0;";
						this.SetPlayCount(0);
					}
				}

				Title = tg.Tag.Title;
				Title = Title ?? SourceURL.Substring(Math.Max(0, SourceURL.Length - 35)).Replace("\\", "/");
				Title = Utilities.SanitiseInternationalTrimUpper(Title);
				Genre = Utilities.SanitiseInternationalTrimUpper(tg.Tag.FirstGenre);
				if (Genre == null || Genre == "") {
					Genre = "Not Set";
				}
				PlayCount = p_playcount;
				Rating = p_rating;
				Author = Utilities.SanitiseInternationalTrimUpper(tg.Tag.FirstAlbumArtist);

				if ((Author == null) || (Author == "") || Author.ToLower().Contains("various") ||
				    Author.ToLower().Contains("unknown")) {
					Author = Utilities.SanitiseInternationalTrimUpper(tg.Tag.FirstPerformer);
					if ((Author == null) || (Author == "") || Author.ToLower().Contains("various") ||
					    Author.ToLower().Contains("unknown")) {
						Author = Utilities.SanitiseInternationalTrimUpper(tg.Tag.FirstComposer);
					}
				}

				if (Author == null || Author == "") {
					Author = "Not Set";
				}
				Album = Utilities.SanitiseInternationalTrimUpper(tg.Tag.Album);
				if (Album == null || Album == "") {
					Album = "Not Set";
				}
				Year = tg.Tag.Year.ToString();
				MediaType = "audio";

				if (Meta != null) {
					Author = Utilities.SanitiseInternationalTrimUpper(Meta.ArtistName) ?? Author;
					//Album = Meta.Album ?? Album;
					Genre = Utilities.SanitiseInternationalTrimUpper(Meta.MainGenre) ?? Genre;
				}

				if (!Comment.Contains(IniFile.MEDIA_TAG_LIBRARY_ID)) {
					//first added to library, reset some fields
					this.SetRating(0);
					this.SetPlayCount(0);
					this.SetLibraryAddedComment();
					this.SetFavorite(false);
					LibraryAddDate = Created;
				}
				else {
					String librarydate = Comment.Substring(Comment.IndexOf(IniFile.MEDIA_TAG_LIBRARY_ID)
					                                       + IniFile.MEDIA_TAG_LIBRARY_ID.Length, "yyyy-mm-dd".Length);
					LibraryAddDate = DateTime.Parse(librarydate);

					if (Comment.Contains(IniFile.MEDIA_TAG_FAVORITE)) {
						this.IsFavorite =
							Comment.Substring(Comment.IndexOf(IniFile.MEDIA_TAG_FAVORITE) + IniFile.MEDIA_TAG_FAVORITE.Length, 1) == "1";
					}
					else {
						this.SetFavorite(false);
					}
				}
				result = true;
			}
			catch (Exception) {
				//MLog.Log(ex, "Unable to read tag for file "+ SourceURL);
				errorMetaCount++;
			}
			return result;
		}

		public override void SaveItem() {
			String parameter;
			m_tagFile = TagLib.File.Create(SourceURL);
			Banshee.Streaming.StreamRatingTagger.StoreRatingAndPlayCount(Rating, PlayCount, m_tagFile);
			if (SaveRatingInComment) {
				parameter = Comment.Substring(IniFile.MEDIA_TAG_RATING, ";");
				Comment = Comment.Replace(IniFile.MEDIA_TAG_RATING + parameter, IniFile.MEDIA_TAG_RATING + Rating);
			}
			if (SavePlayCountInComment) {
				parameter = Comment.Substring(IniFile.MEDIA_TAG_PLAYCOUNT, ";");
				Comment = Comment.Replace(IniFile.MEDIA_TAG_PLAYCOUNT + parameter, IniFile.MEDIA_TAG_PLAYCOUNT + PlayCount);
			}
			m_tagFile.Tag.Comment = Comment;

			try {
				m_tagFile.Save();
				m_requireSave = false;
			}
			catch (TagLib.CorruptFileException) {
				m_requireSave = false;
				MLog.Log(this, "Corrupt file, not saving, " + SourceURL);
			}
			catch (IOException ex) {
				m_requireSave = true;
				MLog.Log(this, "Unable to save tag for " + SourceURL + " ex=" + ex.Message);
			}
		}
	}

	public class MediaImageItem : MediaItemBase {
		public List<String> Tags = new List<string>();
		public string CameraModel;
		public string Location;
		public string Subject;
		public List<String> Faces = new List<string>();
		public String FaceList = "";

		public MediaImageItem() {
		}

		public MediaImageItem(String url, String folder) {
			SourceURL = url;
			SourceContainerURL = folder;
		}

		public override void SaveItem() {
			throw new NotImplementedException();
		}

		public void AddFace(string face) {
			Faces.Add(face);
			FaceList = "";
			foreach (string item in Faces)
				FaceList += item + ";";
		}

		public String TagList {
			get {
				string result = "";
				foreach (string tag in Tags)
					result += tag + ";";
				return result;
			}
		}

		public override bool RetrieveMediaItemValues() {
			bool result = false;
			//Goheer.EXIF.EXIFextractor exif;
			//string datetime;
			try {
				/*
				exif = new Goheer.EXIF.EXIFextractor(SourceURL, "", "");
				this.CameraModel = exif["Equip Model"]!=null?exif["Equip Model"].ToString().Replace("\0", ""):"";
				this.Comment = exif["Image Description"]!=null?exif["Image Description"].ToString().Replace("\0", ""):"";
				datetime = exif["Date Time"]!=null?exif["Date Time"].ToString().Replace("\0", ""):"";
				if (datetime.Length >= "yyyy:dd:hh".Length)
				{
					datetime = datetime.Substring(0, "yyyy:dd:hh".Length).Replace(":", "/") + datetime.Substring("yyyy:dd:hh".Length);
				}
				*/
				result = base.RetrieveMediaItemValues();
				FileStream fs = new FileStream(SourceURL, FileMode.Open);
				/*
				
				BitmapSource src = BitmapFrame.Create(fs);
				BitmapMetadata bmp = (BitmapMetadata)src.Metadata;
				this.Created = Convert.ToDateTime(bmp.DateTaken);
				this.Title = bmp.Title ?? "";
				this.Rating = bmp.Rating;
				this.CameraModel = bmp.CameraModel ?? "";
				this.Comment = bmp.Comment ?? "";
				this.Tags = bmp.Keywords!=null ? bmp.Keywords.ToArray():new string[0];
				//this.Created = Convert.ToDateTime(datetime);
				this.Location = bmp.Location ?? "";
				this.Subject = bmp.Subject ?? "";
				*/
				int length = (int) Math.Min(16384, fs.Length);
				byte[] content = new byte[length];
				fs.Position = 0;
				fs.Read(content, 0, length);

				String fileContent = System.Text.Encoding.Default.GetString(content);
				fs.Close();
				int start = fileContent.IndexOf("<x:xmpmeta");
				int end = fileContent.IndexOf("</x:xmpmeta>") + "</x:xmpmeta>".Length;

				if (start > -1) {
					fileContent = fileContent.Substring(start, end - start);
					MediaImageItem media = this;
					XMPService.GetProperties(fileContent.Replace(':', '_'), ref media);
				}
				ExifReader reader = null;
				try {
					reader = new ExifReader(SourceURL);
					// To read a single field, use code like this:

					if (!reader.GetTagValue<DateTime>(ExifTags.DateTimeOriginal, out this.Created)) {
						reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out this.Created);
					}

					if (reader.GetTagValue<String>(ExifTags.Model, out this.CameraModel)) {
					}

					if (reader.GetTagValue<String>(ExifTags.ImageDescription, out this.Title)) {
					}

					if (reader.GetTagValue<String>(ExifTags.Artist, out this.Author)) {
					}

					if (reader.GetTagValue<String>(ExifTags.Copyright, out this.Copyright)) {
					}

					//if (reader.GetTagValue<String>(ExifTags.Software, out this.Tags[0]))
					//{
					//}

					// Parse through all available fields
					/*string props = "";
					foreach (ushort tagID in Enum.GetValues(typeof(ExifTags)))
					{
						object val;
						if (reader.GetTagValue(tagID, out val))
						{
							// Arrays don't render well without assistance.
							string renderedTag;
							if (val is Array)
							{
								var array = (Array)val;

								renderedTag = "";
								if (array.Length > 0)
								{

									foreach (object item in array)
										renderedTag += item + ",";

									renderedTag = renderedTag.Substring(0, renderedTag.Length - 1);
								}
							}
							else
								renderedTag = val.ToString();

							props += string.Format("{0}:{1}\r\n", Enum.GetName(typeof(ExifTags), tagID), renderedTag);
						}
					}

					// Remove the last carriage return
					props = props.Substring(0, props.Length - 2);
					 */
				}
				catch (Exception) {
					// Something didn't work!
					//MLog.Log(this, "Err read image exif file "+this.SourceURL + " er="+ex.Message);

					/*try
					{
						FileInfo fi = new FileInfo(this.SourceURL);
						if (this.Created.Year == 1)
						{
							this.Created = fi.CreationTime;
							this.Modified = fi.LastWriteTime;
						}
					}
					catch (Exception exx)
					{
						MLog.Log(exx, "Error read creationtime " + this.SourceURL);
					}
					*/
					if (reader != null) {
						reader.Dispose();
					}
				}

				result = true;
			}
			catch (Exception) {
				errorMetaCount++;
			}

			return result;
		}
	}

	public class LastFmMeta {
		public string URL = null;
		public string ArtistURL = null;
		public string MainGenre = null;
		public string ArtistName;
		public List<string> GenreTags;
		public string ArtistOrigin = null;
		public string Album;
		public List<string> SimilarArtists;
		public string ImageURL;
		public string ArtistSummary = null;
		public string YearFormed = null;

		public LastFmMeta() {
		}

		public override string ToString() {
			return String.Format("{0}, {1}", ArtistName, MainGenre);
		}
	}

	public class VideoItem : MediaItemBase {
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

		public VideoItem() {
		}

		public VideoItem(String p_sourceURL) {
			SourceURL = p_sourceURL;
		}

		public override bool RetrieveMediaItemValues() {
			if (SourceURL != "") {
				base.RetrieveMediaItemValues();
				String fileFolder;
				fileFolder = Directory.GetParent(SourceURL).FullName;
				String infoPath = fileFolder + IniFile.VIDEO_INFO_FILE;

				if (File.Exists(infoPath)) {
					ImdbId = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_IMDBID, infoPath);
					ImdbTitle = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_NAME, infoPath);
					ImdbYear = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_YEAR, infoPath);
					ImdbGenre = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_GENRE, infoPath);
					ImdbDirector = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_DIRECTOR, infoPath);
					Seen = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_SEEN, infoPath);
					ImdbActors = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_ACTORS, infoPath);
					ImdbImageURL = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_IMAGEURL, infoPath);
					ImdbDescription = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_DESCRIPTION,
						infoPath);
					ImdbRating = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_RATING, infoPath);
					//result.Resolution = Utilities.IniReadValue(IniFile.VIDEO_INFO_INI_SECTION, IniFile.VIDEO_INFO_INI_RESOLUTION, infoPath);
					switch (Path.GetExtension(SourceURL)) {
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

					if ((ImdbActors == "") && (ImdbDescription != "")) {
						ImdbActors = IMDBParser.GetImdbActorsFromDescription(ImdbDescription);
					}
				}
				else {
					ImdbTitle = SourceURL.Substring(SourceURL.LastIndexOf("\\") + 1);
				}

				return true;
			}
			else {
				MLog.Log(this, "Error no source url set for videoitem");
				return false;
			}
		}

		private static void SaveVideoInfo(VideoItem videoInfo) {
			if (videoInfo == null) {
				return;
			}

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

		public override void SaveItem() {
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
		public ValueList<MediaItem> GetMusicPlaylistByCriteria(String searchCriteria)
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

	public class VideoCollection : PlaylistBase {
		public List<VideoItem> m_videoPlayList;
		public Dictionary<String, int> m_actorNames;
		public Dictionary<String, int> m_GenreList;

		public VideoCollection() {
			m_actorNames = new Dictionary<String, int>();
			m_GenreList = new Dictionary<String, int>();
			m_videoPlayList = new List<VideoItem>();
		}

		public List<VideoItem> GetVideoCollection() {
			return m_videoPlayList;
		}

		public ICollection GetActorNames() {
			return m_actorNames.Keys;
		}

		public IOrderedEnumerable<String> GetActorsByAppearance(int minimuAppeareanceCount) {
			var actors = from key in m_actorNames.Keys
				where m_actorNames[key] >= minimuAppeareanceCount
				orderby m_actorNames[key] + key descending
				select key;
			return actors;
		}

		public IOrderedEnumerable<String> GetGenresByAppearance(int minimuAppeareanceCount) {
			var genres = from key in m_GenreList.Keys
				where m_GenreList[key] >= minimuAppeareanceCount
				orderby m_GenreList[key] + key descending
				select key;
			return genres;
		}

		public VideoItem GetVideoInfo(String filePath) {
			return m_videoPlayList.Find(delegate(VideoItem info) { return info.SourceURL == filePath; });
		}

		public override void SaveUpdatedItems() {
			//throw new NotImplementedException();
		}

		public override int PendingSaveItemsCount() {
			return 0;
			//throw new NotImplementedException();
		}

		protected override void ProcessFile(FileInfo fi) {
			VideoItem vidInfo;
			FileDetail fileInCache;
			fileInCache = m_playlistFiles.Find(x => x.FilePath == fi.FullName);
			if (fileInCache == null) {
				vidInfo = new VideoItem(fi.FullName);
				vidInfo.RetrieveMediaItemValues();
				ParseImdbData(vidInfo);
				m_videoPlayList.Add(vidInfo);
			}
			else {
				vidInfo = m_videoPlayList.Find(x => x.SourceURL == fi.FullName);
				if (vidInfo != null) {
					vidInfo.RetrieveMediaItemValues();
					ParseImdbData(vidInfo);
				}
				else {
					MLog.Log(this, "Unexpected Missing Video file in memory, file=" + fi.FullName);
				}
			}
		}

		private void ParseImdbData(VideoItem vidInfo) {
			String actors, actorName;
			//parse actors
			actors = vidInfo.ImdbActors;
			int lastIndex = 0;
			for (int i = 0; i < actors.Length; i++) {
				if ((actors[i] == ',') || (i == actors.Length - 1)) {
					actorName = actors.Substring(lastIndex, i - lastIndex);
					if (!m_actorNames.Keys.Contains(actorName)) {
						m_actorNames.Add(actorName, 1);
					}
					else {
						m_actorNames[actorName] = m_actorNames[actorName] + 1;
					}
					lastIndex = i + 1;
				}
			}

			//parse genres
			List<Object> genres;
			genres = Utilities.ParseStringForValues(vidInfo.ImdbGenre, ' ', typeof (String));
			String genre;
			foreach (Object obj in genres) {
				genre = (String) obj;
				if (!m_GenreList.Keys.Contains(genre)) {
					m_GenreList.Add(genre, 1);
				}
				else {
					m_GenreList[genre] = m_GenreList[genre] + 1;
				}
			}
		}
	}

	public class MusicCollection : PlaylistBase {
		public List<LastFmMeta> m_artistMetaList;
		public List<AudioItem> m_playlistItems;

		public List<LastFmMeta> ArtistMetaList {
			get { return m_artistMetaList; }
		}

		public MusicCollection()
			: base() {
			m_artistMetaList = new List<LastFmMeta>();
			m_playlistItems = new List<AudioItem>();
			//m_playlistItems = new List<MediaItemBase>();
		}

		public List<AudioItem> PlaylistItems {
			get {
				return m_playlistItems;
				//return new List<AudioItem>(m_playlistItems.Cast<AudioItem>());
			}
		}

		public override void SaveUpdatedItems() {
			if (m_playlistItems != null) {
				List<AudioItem> clone = m_playlistItems.ToList();
				foreach (MediaItemBase ai in clone) {
					if (ai.RequireSave) {
						ai.SaveItem();
					}
				}
			}
		}

		public override int PendingSaveItemsCount() {
			return m_playlistItems.Count(m => m.RequireSave);
		}

		protected override void ProcessFile(FileInfo fi) {
			DirectoryInfo dir;
			AudioItem item;
			LastFmMeta meta, metaDisk, metaDownload;
			String metaText;
			String metaFile;
			FileDetail fileInCache;

			fileInCache = m_playlistFiles.Find(x => x.FilePath == fi.FullName);
			if (fileInCache == null) {
				dir = fi.Directory;
				do {
					meta = m_artistMetaList.Find(x => x.URL == dir.FullName);
					if (meta == null) {
						metaFile = dir.FullName + IniFile.MEDIA_META_FILE_NAME;
						if (File.Exists(metaFile)) {
							metaText = Utilities.ReadFile(metaFile);
							metaDisk = fastJSON.JSON.Instance.ToObject<LastFmMeta>(metaText);
							if (metaDisk.ArtistName == null) {
								metaDisk.ArtistName = dir.Name;
							}
							if (metaDisk.MainGenre == null || IniFile.PARAM_LASTFM_FORCE_META_UPDATE[1] == "1") {
								metaDownload = LastFMService.GetArtistMeta(metaDisk.ArtistName);
								meta = metaDownload;
								meta.URL = dir.FullName;
								fastJSON.JSONParameters param = new fastJSON.JSONParameters();
								param.UseExtensions = false;
								metaText = fastJSON.JSON.Instance.ToJSON(meta, param);
								System.IO.File.WriteAllText(metaFile, metaText);
							}
							else {
								meta = metaDisk;
							}
							m_artistMetaList.Add(meta);
							break;
						}
						if (dir.Parent != null) {
							dir = dir.Parent;
						}
						else {
							break;
						}
					}
					else {
						break;
					}
				} while (true);
				m_playlistFiles.Add(new FileDetail(fi.FullName, fi.LastWriteTime));
				item = new AudioItem(fi.FullName, fi.DirectoryName, meta);
				item.RetrieveMediaItemValues();
				m_playlistItems.Add(item);
			}
			else {
				item = m_playlistItems.Find(x => x.SourceURL == fi.FullName);
				if (item != null) {
					item.RetrieveMediaItemValues();
				}
				else {
					MLog.Log(this, "Unexpected Missing Audio file in memory, file=" + fi.FullName);
				}
			}
		}
	}

	public class PicturesCollection : PlaylistBase {
		public List<MediaImageItem> m_playlistItems;
		private List<MediaImageItem> m_autoIterateItems;
		private DateTime m_autoIterateDate = DateTime.Now;
		private DateTime m_lastPictureRetrieved = DateTime.Now;
		private int m_currentPictureIndex;
		private int m_intervalIterateSecs;

		public PicturesCollection()
			: base() {
			m_playlistItems = new List<MediaImageItem>();
		}

		public override int PendingSaveItemsCount() {
			return 0;
			//throw new NotImplementedException();
		}

		public override void SaveUpdatedItems() {
			return;
			//throw new NotImplementedException();
		}

		protected override void ProcessFile(FileInfo fi) {
			//throw new NotImplementedException();
			MediaImageItem item;
			FileDetail fileInCache;
			fileInCache = m_playlistFiles.Find(x => x.FilePath == fi.FullName);

			if (fileInCache == null) {
				item = new MediaImageItem(fi.FullName, fi.DirectoryName);
				item.RetrieveMediaItemValues();
				m_playlistItems.Add(item);
			}
			else {
				item = m_playlistItems.Find(x => x.SourceURL == fi.FullName);
				if (item != null) {
					item.RetrieveMediaItemValues();
				}
				else {
					MLog.Log(this, "Unexpected Missing Picture file in memory, file=" + fi.FullName);
				}
			}
		}

		public MediaImageItem CurrentIteratePicture {
			get {
				//NextPicture();
				if (m_autoIterateItems != null && m_autoIterateItems.Count > 0) {
					return m_autoIterateItems[m_currentPictureIndex];
				}
				else {
					return null;
				}
			}
		}

		public void ForceNextPicture() {
			if (m_currentPictureIndex + 1 < m_autoIterateItems.Count) {
				m_currentPictureIndex++;
			}
			else {
				m_currentPictureIndex = 0;
			}
		}

		public void ForcePreviousPicture() {
			if (m_currentPictureIndex == 0) {
				m_currentPictureIndex = m_autoIterateItems.Count;
			}
			else {
				m_currentPictureIndex--;
			}
		}

		private void NextPicture() {
			if (m_autoIterateItems != null && m_autoIterateItems.Count > 0) {
				if (DateTime.Now.Subtract(m_lastPictureRetrieved).TotalSeconds >= m_intervalIterateSecs) {
					if (m_currentPictureIndex + 1 < m_autoIterateItems.Count) {
						m_currentPictureIndex++;
					}
					else {
						m_currentPictureIndex = 0;
					}
					m_lastPictureRetrieved = DateTime.Now;
				}
			}
		}

		public string IteratePicture(int items, int intervalSecs, string face) {
			DateTime currentDate = DateTime.Now;
			List<MediaImageItem> imagesFound;
			int tries = 0;
			if (m_autoIterateDate.Day != DateTime.Now.Day) {
				m_autoIterateItems = null;
			}
			string compareDate;
			TimeSpan day = new TimeSpan(1, 0, 0, 0);
			m_intervalIterateSecs = intervalSecs;

			if (m_autoIterateItems == null || m_autoIterateItems.Count == 0) {
				do {
					compareDate = currentDate.ToString("dd/MM");

					imagesFound = m_playlistItems.FindAll(x =>
						(x.Created.ToString("dd/MM") == compareDate)
						&& !x.Tags.Contains(IniFile.PARAM_PICTURE_TAG_IGNORE[1]))
						.ToList();

					if (imagesFound.Count > 0 && face != null) {
						imagesFound = imagesFound.FindAll(delegate(MediaImageItem item) {
							if (item.FaceList == "" || item.FaceList == null) {
								return false;
							}
							string[] faceAtoms = face.ToLower().Split(';');
							foreach (string el in faceAtoms) {
								if (item.FaceList.ToLower().Contains(el)) {
									return true;
								}
							}
							return false;
						}).ToList();
					}

					if (m_autoIterateItems != null) {
						m_autoIterateItems = m_autoIterateItems.Concat(imagesFound).
							OrderByDescending(y => y.Created.ToString("MM/dd")).Take(items).ToList();
					}
					else if (imagesFound.Count > 0) {
						m_autoIterateItems = imagesFound;
					}

					m_autoIterateDate = DateTime.Now;
					m_currentPictureIndex = -1;
					if (m_autoIterateItems == null || m_autoIterateItems.Count < items) {
						currentDate = currentDate.Subtract(day);
					}
					tries++;
				} while ((m_autoIterateItems == null || m_autoIterateItems.Count < items) && tries < 366);
			}

			NextPicture();

			string picture;

			if (m_autoIterateItems == null || m_autoIterateItems.Count == 0 || m_currentPictureIndex == -1) {
				picture = IniFile.PARAM_PICTURE_STORE_ROOT_PATH[1] + "\\notfound.jpg";
			}
			else {
				picture = m_autoIterateItems[m_currentPictureIndex].SourceURL;
			}
			return picture;
		}
	}

	public class MediaLibrary {
		private static bool m_isMusicInitialised = false;
		private static bool m_isPicturesInitialised = false;
		private static bool m_isVideosInitialised = false;

		private static MusicCollection m_musicFiles;
		private static VideoCollection m_videoFiles;
		private static PicturesCollection m_pictureFiles;

		public static Boolean HasMusicCache = false;
		public static Boolean HasVideoCache = false;
		public static Boolean HasPictureCache = false;

		public static bool IsInitialised {
			get { return m_isMusicInitialised && m_isPicturesInitialised && m_isVideosInitialised; }
		}

		public static void InitialiseLibrary() {
			MLog.Log(null, "MediaLibrary Init started");

			m_isMusicInitialised = false;
			if (Utilities.ExistFileRelativeToAppPath(IniFile.MEDIA_MUSIC_STORAGE_FILE)) {
				HasMusicCache = true;
				MLog.Log("Music Library loading from cache");
				String json = Utilities.ReadFileRelativeToAppPath(IniFile.MEDIA_MUSIC_STORAGE_FILE);
				MusicCollection jsonMusic = JSON.Instance.ToObject<MusicCollection>(json);
				m_musicFiles = jsonMusic;
				m_isMusicInitialised = true;
				MLog.Log("Music Library cache loading done");
			}
			else {
				m_musicFiles = new MusicCollection();
			}
			InitialiseMusic();
			Application.DoEvents();

			m_isPicturesInitialised = false;
			if (Utilities.ExistFileRelativeToAppPath(IniFile.MEDIA_PICTURE_STORAGE_FILE)) {
				HasPictureCache = true;
				MLog.Log("Pictures Library loading from cache");
				String json = Utilities.ReadFileRelativeToAppPath(IniFile.MEDIA_PICTURE_STORAGE_FILE);
				PicturesCollection jsonPict = JSON.Instance.ToObject<PicturesCollection>(json);
				m_pictureFiles = jsonPict;
				MLog.Log("Pictures Library loading cache done");
				m_isPicturesInitialised = true;
			}
			else {
				m_pictureFiles = new PicturesCollection();
			}
			InitialisePictures();
			Application.DoEvents();

			m_isVideosInitialised = false;
			if (Utilities.ExistFileRelativeToAppPath(IniFile.MEDIA_VIDEO_STORAGE_FILE)) {
				HasVideoCache = true;
				MLog.Log("Video Library loading from cache");
				String json = Utilities.ReadFileRelativeToAppPath(IniFile.MEDIA_VIDEO_STORAGE_FILE);
				VideoCollection jsonVideo = JSON.Instance.ToObject<VideoCollection>(json);
				m_videoFiles = jsonVideo;
				MLog.Log("Video Library loading cache done");
				m_isVideosInitialised = true;
			}
			else {
				m_videoFiles = new VideoCollection();
			}
			InitialiseVideos();
			MLog.Log(null, "MediaLibrary Init ended, async process running");
		}

		public static void InitialiseMusic() {
			Thread thmu = new Thread(() => MediaLibrary.InitialiseMusicWorker());
			thmu.Name = "MediaLibrary Music";
			thmu.Start();
		}

		public static void InitialisePictures() {
			Thread thpi = new Thread(() => MediaLibrary.InitialisePicturesWorker());
			thpi.Name = "MediaLibrary Pictures";
			thpi.Start();
		}

		public static void InitialiseVideos() {
			Thread thmo = new Thread(() => MediaLibrary.InitialiseVideosWorker());
			thmo.Name = "MediaLibrary Movies";
			thmo.Start();
		}

		private static void InitialiseMusicWorker() {
			MLog.Log(null, "Music Library refresh started");

			MLog.Log(null, "Checking mediaplayer music files from " + IniFile.PARAM_MUSIC_STORE_ROOT_PATH[1]);
			for (int i = 0; i < IniFile.MUSIC_EXTENSION.Length; i++) {
				m_musicFiles.AddFiles(IniFile.PARAM_MUSIC_STORE_ROOT_PATH[1], "*." + IniFile.MUSIC_EXTENSION[i],
					System.IO.SearchOption.AllDirectories, HasMusicCache);
			}
			//save new items found in library
			m_musicFiles.SaveUpdatedItems();
			if (IniFile.PARAM_LASTFM_FORCE_META_UPDATE[1] == "1") {
				IniFile.PARAM_LASTFM_FORCE_META_UPDATE[1] = "0, last refresh at " +
				                                            DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT);
			}
			if (MZPState.Instance != null) {
				m_isMusicInitialised = true;
			}
			MLog.Log(null, "Music Library refresh done");
		}

		private static void InitialisePicturesWorker() {
			MLog.Log(null, "Checking mediaplayer picture files from " + IniFile.PARAM_PICTURE_STORE_ROOT_PATH[1]);
			for (int i = 0; i < IniFile.PICTURE_EXTENSION.Length; i++) {
				m_pictureFiles.AddFiles(IniFile.PARAM_PICTURE_STORE_ROOT_PATH[1], "*." + IniFile.PICTURE_EXTENSION[i],
					System.IO.SearchOption.AllDirectories, HasPictureCache);
			}
			//save new items found in library
			m_pictureFiles.SaveUpdatedItems();
			if (MZPState.Instance != null) {
				m_isPicturesInitialised = true;
			}
		}

		private static void InitialiseVideosWorker() {
			MLog.Log(null, "Loading mediaplayer video files from " + IniFile.PARAM_VIDEO_STORE_ROOT_PATH[1]);
			for (int i = 0; i < IniFile.VIDEO_EXTENSION.Length; i++) {
				m_videoFiles.AddFiles(IniFile.PARAM_VIDEO_STORE_ROOT_PATH[1], "*." + IniFile.VIDEO_EXTENSION[i],
					System.IO.SearchOption.AllDirectories, HasVideoCache);
			}
			//save new items found in library
			m_videoFiles.SaveUpdatedItems();
			if (MZPState.Instance != null) {
				m_isVideosInitialised = true;
			}
		}

		public static void SaveLibraryToIni() {
			String json;
			try {
				if (m_isMusicInitialised) {
					MLog.Log("Saving music library");
					fastJSON.JSONParameters param = new fastJSON.JSONParameters();
					param.UseExtensions = false;
					param.UseEscapedUnicode = false;
					param.SerializeNullValues = false;
					json = fastJSON.JSON.Instance.ToJSON(m_musicFiles, param);
					Utilities.WriteTextFileRelToAppPath(IniFile.MEDIA_MUSIC_STORAGE_FILE, json);
					MLog.Log("Saving music library done");
				}
				if (m_isVideosInitialised) {
					MLog.Log("Saving video library");
					fastJSON.JSONParameters param = new fastJSON.JSONParameters();
					param.UseExtensions = false;
					param.UseEscapedUnicode = false;
					param.SerializeNullValues = false;
					json = fastJSON.JSON.Instance.ToJSON(m_videoFiles, param);
					Utilities.WriteTextFileRelToAppPath(IniFile.MEDIA_VIDEO_STORAGE_FILE, json);
					MLog.Log("Saving video library done");
				}
				if (m_isPicturesInitialised) {
					MLog.Log("Saving picture library");
					fastJSON.JSONParameters param = new fastJSON.JSONParameters();
					param.UseExtensions = false;
					param.UseEscapedUnicode = false;
					param.SerializeNullValues = false;
					json = fastJSON.JSON.Instance.ToJSON(m_pictureFiles, param);
					Utilities.WriteTextFileRelToAppPath(IniFile.MEDIA_PICTURE_STORAGE_FILE, json);
					MLog.Log("Saving picture library done");
				}
			}
			catch (Exception ex) {
				MLog.Log(ex, "Cannot save media library");
			}
		}

		public static MusicCollection AllAudioFiles {
			get { return m_musicFiles; }
		}

		public static VideoCollection AllVideoFiles {
			get { return m_videoFiles; }
		}

		public static PicturesCollection AllPictureFiles {
			get { return m_pictureFiles; }
		}

		public static List<AudioItem> AudioFilesByGenre(ValueList vals) {
			var items =
				m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item) { return vals.ContainsIndexValue(item.Genre, false); });
			//GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
			return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
		}

		public static List<AudioItem> AudioFilesByGenre(string genre) {
			genre = genre.ToLower();
			var items =
				m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item) { return item.Genre.ToLower().Contains(genre); });
			//GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
			return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
		}

		public static List<AudioItem> AudioFilesByArtist(ValueList vals) {
			var items =
				m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item) { return vals.ContainsIndexValue(item.Author, false); });
			//GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
			return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
		}

		public static List<AudioItem> AudioFilesByArtist(String artist) {
			artist = artist.ToLower();
			var items =
				m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item) { return item.Author.ToLower().Contains(artist); });
			return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
		}

		public static List<AudioItem> AudioFilesByAlbum(String album) {
			album = album.ToLower();
			var items =
				m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item) { return item.Album.ToLower().Contains(album); });
			return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
		}

		public static List<AudioItem> AudioFilesByFolder(String sourceContainerURL) {
			var items =
				m_musicFiles.PlaylistItems.FindAll(
					delegate(AudioItem item) { return sourceContainerURL.Equals(item.SourceContainerURL); });
			return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
		}

		public static List<AudioItem> AudioFilesByRating(int rating) {
			var items = m_musicFiles.PlaylistItems.FindAll(delegate(AudioItem item) { return rating.Equals(item.Rating); });
			return items.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
		}

		public static ValueList MusicGenres {
			get {
				//var unique = m_musicFiles.PlaylistItems.GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
				var unique = m_musicFiles.PlaylistItems.Select(i => i.Genre).Distinct().ToList();
				ValueList val = new ValueList(CommandSources.system);
				unique.Sort();
				val.SetIndexValues(unique.ToList());
				return val;
			}
		}

		public static ValueList MusicArtists {
			get {
				//var unique = m_musicFiles.PlaylistItems.GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
				var unique = m_musicFiles.PlaylistItems.Select(i => i.Author).Distinct().ToList();
				unique.Sort();

				ValueList val = new ValueList(CommandSources.system);
				val.SetIndexValues(unique.ToList());
				return val;
			}
		}

		public static ValueList MovieList {
			get {
				//var unique = m_musicFiles.PlaylistItems.GroupBy(i => i.Genre).Where(i => i.Count() == 1).Select(i => i.Key);
				var unique = m_videoFiles.GetVideoCollection().Select(i => i.Title).Distinct().ToList();
				unique.Sort();

				ValueList val = new ValueList(CommandSources.system);
				val.SetIndexValues(unique.ToList());
				return val;
			}
		}

		public static List<AudioItem> GetMoodPlaylist(MoodMusic mood) {
			List<AudioItem> result = null;
			if (mood != null) {
				try {
					IEnumerable<AudioItem> query, res1, res2, res3, res4;
					bool exclude;
					String value;

					res1 = null;
					foreach (String val in mood.Genres) {
						exclude = val.Contains('!');
						if (exclude) {
							if (res1 == null) {
								res1 = MediaLibrary.AllAudioFiles.PlaylistItems;
							}
							value = val.Replace("!", "");
							res1 = res1.Where(i => !i.Genre.ToLower().Contains(value.ToLower())).ToList();
							//res1 = res1.Intersect(query).Distinct().ToList();
						}
						else {
							if (res1 == null) {
								res1 = new List<AudioItem>();
							}
							value = val;
							query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Genre.ToLower().Contains(value.ToLower())).ToList();
							res1 = res1.Union(query).Distinct().ToList();
						}
					}
					if (res1 == null) {
						res1 = new List<AudioItem>();
					}

					res2 = null;
					foreach (String val in mood.Authors) {
						exclude = val.Contains('!');
						if (exclude) {
							if (res2 == null) {
								res2 = MediaLibrary.AllAudioFiles.PlaylistItems;
							}
							value = val.Replace("!", "");
							res2 = res2.Where(i => !i.Author.ToLower().Contains(value.ToLower())).ToList();
						}
						else {
							if (res2 == null) {
								res2 = new List<AudioItem>();
							}
							value = val;
							query =
								MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Author.ToLower().Contains(value.ToLower())).ToList();
							res2 = res2.Union(query).Distinct().ToList();
						}
					}
					if (res2 == null) {
						res2 = new List<AudioItem>();
					}

					res3 = new List<AudioItem>();
					foreach (int val in mood.Ratings) {
						query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Rating.Equals(val)).ToList();
						res3 = res3.Union(query).Distinct().ToList();
					}

					res4 = new List<AudioItem>();
					foreach (int val in mood.AgeInWeeks) {
						query = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.IsThatNew(val)).ToList();
						res4 = res4.Union(query).Distinct().ToList();
					}

					switch (mood.LogicalSearchOperator) {
						case MoodMusic.LogicalSearchOperatorEnum.Intersect:
							if (res1.Count() + res2.Count() + res3.Count() + res4.Count() != 0) {
								if (res1.Count() == 0) {
									res1 = MediaLibrary.AllAudioFiles.PlaylistItems;
								}
								if (res2.Count() == 0) {
									res2 = MediaLibrary.AllAudioFiles.PlaylistItems;
								}
								if (res3.Count() == 0) {
									res3 = MediaLibrary.AllAudioFiles.PlaylistItems;
								}
								if (res4.Count() == 0) {
									res4 = MediaLibrary.AllAudioFiles.PlaylistItems;
								}
								result = res1.Intersect(res2).Intersect(res3).Intersect(res4).ToList();
							}
							else {
								MLog.Log(null, "No songs matching criteria mood=" + mood.Name);
							}
							break;
						case MoodMusic.LogicalSearchOperatorEnum.Union:
							result = res1.Union(res2).Union(res3).Union(res4).ToList();
							break;
					}

					if (result != null && mood.IsGroupByTop) {
						IEnumerable<AudioItem> list = new List<AudioItem>();
						var items = result.OrderBy(y => y.PlayCount).ThenBy(z => z.RandomId).GroupBy(x => x.Author).Distinct();
						foreach (var item in items) {
							list = list.Union(item.Take(3));
						}
						result = list.ToList();
					}
				}
				catch (Exception ex) {
					MLog.Log(ex, "Error get mood=" + mood);
				}
			}
			else {
				MLog.Log(null, "NULL mood unexpected");
			}
			return result;
		}
	}
}