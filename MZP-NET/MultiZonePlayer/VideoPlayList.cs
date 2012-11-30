using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;


namespace MultiZonePlayer
{
    

    public partial class VideoPlayList : Form
    {
        private VideoCollection m_playlist;
        //private List<VideoInfo> m_videoPlayList;
        //private ZoneVideoApp m_parentZone;

        public VideoPlayList()
        {
            InitializeComponent();
            //m_parentZone = parent;
        }

        private void VideoPlayList_Load(object sender, EventArgs e)
        {
            m_playlist = MediaLibrary.AllVideoFiles;
           // m_videoPlayList = new List<VideoInfo>();
            /*
            VideoItem videoInfo;

            for (int i = 0; i < IniFile.VIDEO_EXTENSION.Length; i++)
            {
                m_playlist.AddFiles(IniFile.PARAM_VIDEO_STORE_ROOT_PATH[1], 
                    "*."+IniFile.VIDEO_EXTENSION[i], System.IO.SearchOption.AllDirectories);
            }
            */

            foreach (VideoItem videoInfo in m_playlist.GetVideoCollection())
            {
                //filePath = m_playlist.GetAllFileList()[file].ToString();
                dgvVideos.Rows.Add(false, videoInfo.ImdbTitle, videoInfo.ImdbId, videoInfo.ImdbRating,videoInfo.ImdbYear, 
                    videoInfo.ImdbGenre, videoInfo.ImdbActors, videoInfo.ImdbDirector, videoInfo.Seen, videoInfo.Resolution,
                    videoInfo.ImdbDescription, videoInfo.SourceURL, videoInfo.ImdbImageURL);
            }

            dgvVideos.Sort(dgvVideos.Columns["Video_Name"], ListSortDirection.Ascending);
        }

        

        private void dgvVideos_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1) return;

            if (dgvVideos.Columns[e.ColumnIndex].Name == "Video_Name")
            {
                //m_parentZone.Play(dgvVideos.Rows[e.RowIndex].Cells["Video_Path"].Value.ToString());
            }

            if (dgvVideos.Columns[e.ColumnIndex].Name == "Video_ImdbId")
            {
                webMain.Navigate(IniFile.VIDEO_INFO_IMDB_URL + dgvVideos.Rows[e.RowIndex].Cells["Video_ImdbId"].Value.ToString());
            }
        }

        private void dgvVideos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            if (e.ColumnIndex != 0)
                dgvVideos.Rows[e.RowIndex].Cells["Video_Changed"].Value = true;
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            try
            {
                VideoItem info;
                for (int i = 0; i < dgvVideos.RowCount; i++)
                {
                    if (dgvVideos.Rows[i].Cells["Video_Changed"].Value.ToString() == "True")
                    {
                        info = GetVideoInfo(i);
                        if (info != null)
                        {
                            info.SaveItem();
                            dgvVideos.Rows[i].Cells["Video_Changed"].Value = false;
                        }
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Save error " + ex.Message + ex.StackTrace);
            }
        }

        private VideoItem GetVideoInfo(int rowIndex)
        {
            if (dgvVideos.Rows[rowIndex].Cells["Video_Name"].Value == null)
                return null;

            VideoItem info = new VideoItem(dgvVideos.Rows[rowIndex].Cells["Video_Path"].Value.ToString());
            
            info.ImdbTitle = dgvVideos.Rows[rowIndex].Cells["Video_Name"].Value.ToString();
            info.ImdbId = dgvVideos.Rows[rowIndex].Cells["Video_ImdbId"].Value.ToString();
            info.ImdbYear = dgvVideos.Rows[rowIndex].Cells["Video_Year"].Value.ToString();
            info.ImdbDescription = dgvVideos.Rows[rowIndex].Cells["Video_Description"].Value.ToString();
            info.ImdbDirector = dgvVideos.Rows[rowIndex].Cells["Video_Director"].Value.ToString();
            info.Seen = dgvVideos.Rows[rowIndex].Cells["Video_Seen"].Value.ToString();
            info.Resolution = dgvVideos.Rows[rowIndex].Cells["Video_Resolution"].Value.ToString();
            info.ImdbGenre = dgvVideos.Rows[rowIndex].Cells["Video_Genre"].Value.ToString();
            info.ImdbActors = dgvVideos.Rows[rowIndex].Cells["Video_Actors"].Value.ToString();
            info.ImdbImageURL= dgvVideos.Rows[rowIndex].Cells["Video_ImageURL"].Value.ToString();
            info.ImdbRating = dgvVideos.Rows[rowIndex].Cells["Video_Rating"].Value.ToString();

            return info;
        }

        private void SetVideoInfo(VideoItem videoInfo, int rowIndex)
        {
            dgvVideos.Rows[rowIndex].Cells["Video_Name"].Value = videoInfo.ImdbTitle;
            dgvVideos.Rows[rowIndex].Cells["Video_ImdbId"].Value = videoInfo.ImdbId;
            dgvVideos.Rows[rowIndex].Cells["Video_Year"].Value = videoInfo.ImdbYear;
            dgvVideos.Rows[rowIndex].Cells["Video_Description"].Value = videoInfo.ImdbDescription;
            dgvVideos.Rows[rowIndex].Cells["Video_Genre"].Value = videoInfo.ImdbGenre;
            dgvVideos.Rows[rowIndex].Cells["Video_Actors"].Value = videoInfo.ImdbActors;
            dgvVideos.Rows[rowIndex].Cells["Video_Director"].Value = videoInfo.ImdbDirector;
            dgvVideos.Rows[rowIndex].Cells["Video_Seen"].Value = videoInfo.Seen;
            dgvVideos.Rows[rowIndex].Cells["Video_Resolution"].Value = videoInfo.Resolution;
            dgvVideos.Rows[rowIndex].Cells["Video_Rating"].Value = videoInfo.ImdbRating;
            dgvVideos.Rows[rowIndex].Cells["Video_ImageURL"].Value = videoInfo.ImdbImageURL;
        }

        private void VideoPlayList_SizeChanged(object sender, EventArgs e)
        {
            tableLayoutPanel1.Height = this.Height - (tableLayoutPanel1.Location.Y * 2);
            tableLayoutPanel1.Width = this.Width - (tableLayoutPanel1.Location.X * 2);
            /*dgvVideos.Height = dgvVideos.Parent.Height - (dgvVideos.Location.Y *2);
            dgvVideos.Width = dgvVideos.Parent.Width - (dgvVideos.Location.X * 2);
            webMain.Width = webMain.Parent.Width - (webMain.Location.X * 2);
            webMain.Height = webMain.Parent.Height - (webMain.Location.Y * 2);
             * */
           
        }

        private void VideoPlayList_FormClosed(object sender, FormClosedEventArgs e)
        {
            //m_parentZone.NotifyClose();
        }

        private void LoadSetImdbInfo(int rowIndex)
        {
            try
            {
                String imdbId = dgvVideos.Rows[rowIndex].Cells["Video_ImdbId"].Value.ToString();
                dgvVideos.CurrentCell = dgvVideos.Rows[rowIndex].Cells[0];
                if (imdbId.Length > 0)
                {
                    //VideoInfo videoInfo = m_videoPlayList.Find(delegate(VideoInfo info) { return info.Path == path; });
                    VideoItem videoInfo = IMDBParser.GetVideoInfo(imdbId, true, webMain);
                    SetVideoInfo(videoInfo, rowIndex);
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Error parsing IMDB " + ex.Message + ex.StackTrace);
            }

        }

        private void btGetImdb_Click(object sender, EventArgs e)
        {
            if (dgvVideos.CurrentRow == null) return;
            LoadSetImdbInfo(dgvVideos.CurrentRow.Index);
        }

        private void dgvVideos_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MLog.Log(null,e.Exception.Message);
        }

        private void btImdbAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvVideos.RowCount; i++)
            {
                LoadSetImdbInfo(i);
                txtStatus.Text = "do row " + i;
                Application.DoEvents();
            }
        }

        private void btFind_Click(object sender, EventArgs e)
        {
            String title;

            if (txTitle.Text == "")
                title = dgvVideos.Rows[dgvVideos.CurrentRow.Index].Cells["Video_Name"].Value.ToString();
            else
            {
                title = txTitle.SelectedText == "" ? txTitle.Text : txTitle.SelectedText;
            }

            String cleanTitle = IMDBParser.CleanTitleforImdbSearch(title);
            txTitle.Text = cleanTitle;
            webMain.Navigate(IniFile.VIDEO_INFO_IMDB_FIND_URL + cleanTitle);
            webMain.Focus();
        }

        private void webMain_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            txtURL.Text = webMain.Url.OriginalString;
        }

        private void btSetImdbId_Click(object sender, EventArgs e)
        {
            if (dgvVideos.CurrentRow == null) return;

            String imdbId = IMDBParser.GetImdbId(webMain.DocumentText);
            dgvVideos.Rows[dgvVideos.CurrentRow.Index].Cells["Video_ImdbId"].Value = imdbId;

            txTitle.Text = "";
            dgvVideos.Focus();

            GetVideoInfo(dgvVideos.CurrentRow.Index).SaveItem();
        }

        private void dgvVideos_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dgvVideos_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvVideos.CurrentRow == null) return;
            String imageURL = dgvVideos.Rows[e.RowIndex].Cells["Video_ImageURL"].Value.ToString();
            imgVideo.ImageLocation = imageURL;
        }

        private void dgvVideos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void btEpisode_Click(object sender, EventArgs e)
        {
            if (dgvVideos.CurrentRow == null) return;
            dgvVideos.Rows[dgvVideos.CurrentRow.Index].Cells["Video_Genre"].Value = "Episode";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvVideos.RowCount; i++)
            {
                if (dgvVideos.Rows[i].Cells["Video_Changed"].Value.ToString() == "True")
                {
                    LoadSetImdbInfo(i);
                    txtStatus.Text = "do row " + i;
                    Application.DoEvents();
                }
            }
        }

        private void btMissingImdb_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvVideos.RowCount; i++)
            {
                if (dgvVideos.Rows[i].Cells["Video_ImdbId"].Value!=null &&
                    (dgvVideos.Rows[i].Cells["Video_Rating"].Value.ToString() == "")
                    && (dgvVideos.Rows[i].Cells["Video_ImdbId"].Value.ToString() != ""))
                {
                    LoadSetImdbInfo(i);
                    txtStatus.Text = "do row " + i;
                    Application.DoEvents();
                }
            }
        }

        private void dgvVideos_Click(object sender, EventArgs e)
        {
            txTitle.Text = "";
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            VideoItem info;
            String del = "^";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dgvVideos.RowCount; i++)
            {
                info = GetVideoInfo(i);
                if (info.ImdbId != "")
                    sb.AppendLine(info.ImdbId + del + info.ImdbTitle + del + info.ImdbYear + del 
                        + info.ImdbRating + del + info.ImdbGenre+del+ info.ImdbActors + del 
                        + IniFile.VIDEO_INFO_IMDB_URL+info.ImdbId+del +info.ImdbDescription);
            }
            Clipboard.SetText(sb.ToString());
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            VideoItem item;
            item = IMDBParser.GetVideoInfo(txTestURL.Text, false, null);
            txTest.Clear();
            txTest.AppendText("ID [0133093]|" + item.ImdbId + "|\n");
            txTest.AppendText("Title [The Matrix (1999)]|" + item.ImdbTitle + "|\n");
            txTest.AppendText("Year [1999]|" + item.ImdbYear + "|\n");
            txTest.AppendText("Rating [8.7]|" + item.ImdbRating + "|\n");
            txTest.AppendText("Genre [Action Adventure Sci-Fi]|" + item.ImdbGenre + "|\n");
            txTest.AppendText("Actors []|" + item.ImdbActors + "|\n");
            txTest.AppendText("Director [Andy Wachowski]|" + item.ImdbDirector + "|\n");
            txTest.AppendText("Image URL[]|" + item.ImdbImageURL + "|\n");
            txTest.AppendText("Description[]|" + item.ImdbDescription + "|\n");
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            webMain.Navigate(txTestURL.Text);
        }

        
    }

}
