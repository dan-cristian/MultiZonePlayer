using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MultiZonePlayer
{
    public partial class VideoPlayMenu : Form
    {
        private ZoneVideoBase m_parentZone;
        private VideoCollection m_videoCollection;
        private List<String> m_logicCriteria;
        private List<String> m_criteria;
        private int m_currentZOrder = 1;
        private Control m_currentContainerControl = null;
        private Control m_currentControl = null;
        private Control m_defaultContainer = null;
        private Control m_defaultControl = null;

        public VideoPlayMenu(ZoneVideoBase parentForm)
        {
            InitializeComponent();
            m_parentZone = parentForm;
            m_criteria = new List<String>();
            m_logicCriteria = new List<String>();
            m_defaultContainer = flowGenreContainer;
            m_defaultControl = btLogicGenre;
            m_currentContainerControl = m_defaultContainer;
            m_currentControl = m_defaultControl;
            HighlightCurrentControl();
            m_logicCriteria.Add(btLogicGenre.Tag+"OR");
            m_logicCriteria.Add(btLogicYear.Tag + "OR");
            m_logicCriteria.Add(btLogicRating.Tag + "OR");
            m_logicCriteria.Add(btLogicActors.Tag + "OR");
            m_logicCriteria.Add(btLogicQuality.Tag + "OR");
            m_logicCriteria.Add(btLogicSeen.Tag + "OR");
            m_videoCollection = MediaLibrary.AllVideoFiles;//new VideoCollection(IniFile.PARAM_VIDEO_STORE_ROOT_PATH[1]);
        }

        public VideoCollection VideoCollection
        {
            get { return m_videoCollection; }
        }

        private void RefreshVideos()
        {
            dgvVideos.Hide();
            dgvVideos.Rows.Clear();
            DisplayVideoInfo(null);
            foreach (VideoItem videoInfo in m_videoCollection.GetVideoCollection())
            {
                if (MatchCriteria(videoInfo))
                    dgvVideos.Rows.Add(videoInfo.ImdbTitle, videoInfo.SourceURL);
            }

            dgvVideos.Sort(dgvVideos.Columns["Video_Name"], ListSortDirection.Ascending);
            if (dgvVideos.Rows.Count>0)
                dgvVideos.CurrentCell = dgvVideos.Rows[0].Cells[0];
            dgvVideos.Show();
            
            String search="";

            foreach (String s in m_logicCriteria)
            {
                search += s + " ";
            }

            foreach (String s in m_criteria)
            {
                search +=s + " ";
            }
            txtSearch.Text = search;

            if (IsARowSelected)
                DisplayRowVideoInfo(dgvVideos.CurrentRow.Index);
            txtSearch.Focus();//take focus away from Play button
        }

        

        private void UpdateCriteria(String criteria, bool criteriaAdded)
        {
            if (criteriaAdded)
            {
                if (criteria.Contains("Logic="))
                {
                    //AND operator always first
                    m_logicCriteria.Insert(0, criteria + "AND");
                    m_logicCriteria.Remove(criteria + "OR");
                }
                else
                    m_criteria.Add(criteria);
            }
            else
            {
                if (criteria.Contains("Logic="))
                {
                    m_logicCriteria.Add(criteria + "OR");
                    m_logicCriteria.Remove(criteria + "AND");
                }
                else
                    m_criteria.Remove(criteria);
            }
        }

        private bool MatchCriteria(VideoItem info)
        {
            String key, value, operand;//, logic;
            bool result=false;
            String[] operands = new String[] { ">=", "<=","=", ">", "<"};
            String logicCriteriaKey, logicCriteriaValue;

            foreach (String logicCriteria in m_logicCriteria)
            {
                logicCriteriaKey = logicCriteria.Substring(0, logicCriteria.IndexOf("="));
                logicCriteriaValue = logicCriteria.Substring(logicCriteria.IndexOf("=") + 1);
                foreach (String item in m_criteria)
                    #region iterate criterias
                {
                    operand = "";
                    foreach (String oper in operands)
                        if (item.Contains(oper)) {operand = oper;break;}

                    //search key and values e.g. Year=2011
                    key = item.Substring(0, item.IndexOf(operand));
                    value = item.Substring(item.IndexOf(operand) + operand.Length);

                    //for debug
                    //if (info.Name == "Pandorum (2009)")
                     //   MLog.Log(null,"debug");

                    if (logicCriteriaKey.Contains(key))
                    {
                            #region switchcriteria

                            switch (logicCriteriaValue + key + operand)
                            {
                                case "ANDYear=":
                                    if (info.ImdbYear.Equals(value)) result = true;
                                    else return false;
                                    break;
                                case "ORYear=":
                                    if (info.ImdbYear.Equals(value)) result = true;
                                    break;
                                case "ANDYear>=":
                                    if (info.ImdbYear.CompareTo(value) >= 0) result = (info.ImdbYear!="");
                                    else return false;
                                    break;
                                case "ORYear>=":
                                    if (info.ImdbYear.CompareTo(value) >= 0) result = (info.ImdbYear != "");
                                    break;
                                case "ANDYear<=":
                                    if (info.ImdbYear.CompareTo(value) <= 0) result = (info.ImdbYear != "");
                                    else return false;
                                    break;
                                case "ORYear<=":
                                    if (info.ImdbYear.CompareTo(value) <= 0) result = (info.ImdbYear != "");
                                    break;
                                case "ANDRating=":
                                    if (info.ImdbRating.Equals(value)) result = true;
                                    else return false;
                                    break;
                                case "ORRating=":
                                    if (info.ImdbRating.Equals(value)) result = true;
                                    break;
                                case "ANDRating>=":
                                    if (info.ImdbRating.CompareTo(value) >= 0) result = (info.ImdbRating != "");
                                    else return false;
                                    break;
                                case "ORRating>=":
                                    if (info.ImdbRating.CompareTo(value) >= 0) result = (info.ImdbRating != "");
                                    break;
                                case "ANDRating<=":
                                    if (info.ImdbRating.CompareTo(value) <= 0) result = (info.ImdbRating != "");
                                    else return false;
                                    break;
                                case "ORRating<=":
                                    if (info.ImdbRating.CompareTo(value) <= 0) result = (info.ImdbRating != "");
                                    break;
                                case "ANDGenre=":
                                    if (info.ImdbGenre.Contains(value)) result = true;
                                    else return false;
                                    break;
                                case "ORGenre=":
                                    if (info.ImdbGenre.Contains(value)) result = true;
                                    break;
                                case "ANDActor=":
                                    if (info.ImdbActors.Contains(value)) result = true;
                                    else return false;
                                    break;
                                case "ORActor=":
                                    if (info.ImdbActors.Contains(value)) result = true;
                                    break;
                                case "ANDSeen=":
                                    if (info.Seen.Equals(value)) result = true;
                                    else return false;
                                    break;
                                case "ORSeen=":
                                    if (info.Seen.Equals(value)) result = true;
                                    break;
                                case "ANDSeen>=":
                                    if (info.Seen.CompareTo(value) >= 0) result = (info.Seen != "");
                                    else return false;
                                    break;
                                case "ORSeen>=":
                                    if (info.Seen.CompareTo(value) >= 0) result = (info.Seen != "");
                                    break;
                                case "ANDSeen<=":
                                    if (info.Seen.CompareTo(value) <= 0) result = (info.Seen != "");
                                    else return false;
                                    break;
                                case "ORSeen<=":
                                    if (info.Seen.CompareTo(value) <= 0) result = (info.Seen != "");
                                    break;
                                case "ANDResolution=":
                                    if (info.Resolution.Equals(value)) result = true;
                                    else return false;
                                    break;
                                case "ORResolution=":
                                    if (info.Resolution.Equals(value)) result = true;
                                    break;
                            }
                            #endregion
                    }
                }
#endregion
                if ((logicCriteriaValue == "AND")&&(result == false))
                    return false;
            }
            return result;
        }

        public String GetCurrentMovie()
        {
            if (IsARowSelected)
                return dgvVideos.Rows[dgvVideos.CurrentRow.Index].Cells["Video_Path"].Value.ToString();
            else
                return "";
        }

        private bool IsARowSelected
        {
            get
            {
                return ((dgvVideos.CurrentRow != null) && (dgvVideos.CurrentRow.Index != -1));
            }
        }

        private void MoveRowSelection(int step)
        {
            MLog.Log(null,"Moving row selection step " + step);
            txtSearch.Focus();
            int i = dgvVideos.CurrentRow.Index;
            if (i != -1)
            {
                MLog.Log(null, "Current row selection before, index= " + i);
                i += step;
                if (i == -1) i = 0;
                if (i == dgvVideos.RowCount) i--;
                dgvVideos.CurrentCell = dgvVideos.Rows[i].Cells[0];
                MLog.Log(null, "Current row selection after, index= " + dgvVideos.CurrentRow.Index);
            }
        }

        public void VideoSelectionDown()
        {
            MoveRowSelection(1);
        }

        public void VideoSelectionUp()
        {
            MoveRowSelection(-1);
        }

        public void NextCriteriaContainer()
        {
            //m_currentZOrder++;
            Control container = FindContainer(this, "Z=", 1);
            if (container != null)
                m_currentContainerControl = container;
            UnhighlightCurrentControl();
            m_currentControl = null;
            MoveSelection(0);
            m_currentZOrder++;
        }

        public void PreviousCriteriaContainer()
        {
            //m_currentZOrder--;
            Control container = FindContainer(this, "Z=", -1);
            if (container != null)
                m_currentContainerControl = container;
            UnhighlightCurrentControl();
            m_currentControl = null;
            MoveSelection(0);
            m_currentZOrder--;
        }

        public void NextCriteriaSelection()
        {
            MoveSelection(1);
        }


        public void PreviousCriteriaSelection()
        {
            MoveSelection(-1);
        }

        public void ToggleSearchCriteria()
        {
            if (m_currentControl != null)
                SwapState(m_currentControl);
        }

        private void MoveSelection(int step)
        {
            MLog.Log(null,"Moving criteria selection with step " + step);
            Control container;
            UnhighlightCurrentControl();
            m_currentControl = GetNextControlInContainer(m_currentContainerControl, step);
            if (m_currentControl == null)
            {
                container = FindContainer(this, "Z=", step);
                if (container != null)
                    m_currentContainerControl = container;
                String tag = m_currentContainerControl.Tag.ToString();
                int index = tag.IndexOf("Z=");
                m_currentZOrder = Convert.ToInt16(tag.Substring("Z=".Length));
                m_currentControl = GetNextControlInContainer(m_currentContainerControl, step);
            }
            HighlightCurrentControl();
        }

        private void HighlightCurrentControl()
        {
            if (m_currentControl != null)
            {
                m_currentControl.BackColor = Color.Turquoise;
                //m_currentControl.Select();

            }
        }

        private void UnhighlightCurrentControl()
        {
            if (m_currentControl != null)
            {
                m_currentControl.BackColor = Color.Transparent;
            }
        }
        private Control GetNextControlInContainer(Control parentControl, int step)
        {
            foreach (Control ctrl in parentControl.Controls)
            {
                if (ctrl.GetType() == typeof(Button))
                {
                    if (m_currentControl == null)
                    {
                        if (step == -1)
                            return parentControl.Controls[parentControl.Controls.Count-1];
                        else
                            return ctrl;
                        
                    }
                    else
                    {
                        if (m_currentControl.TabIndex + step == ctrl.TabIndex)
                            return ctrl;
                    }
                }
            }
            return null;
        }

        //1 - find next, -1 find previous
        private Control FindContainer(Control parentControl, String tag, int step)
        {
            int index, order;
            String value;
            Control result=null;

            foreach (Control ctrl in parentControl.Controls)
            {
                if (ctrl.Tag != null)
                {
                    index=ctrl.Tag.ToString().IndexOf(tag);
                    if (index == 0)
                    {
                        value = ctrl.Tag.ToString().Substring(tag.Length);
                        if (Utilities.CanConvertToInt(value, out order))
                        {
                            if (order == (m_currentZOrder + step))
                            {
                                return ctrl;
                            }
                        }
                    }
                }
                if (ctrl.Controls.Count != 0)
                {
                    result = FindContainer(ctrl, tag, step);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        private void DisplayVideoInfo(VideoItem videoInfo)
        {
            if (videoInfo != null)
            {
                imgVideo.ImageLocation = videoInfo.ImdbImageURL;
                txtDescription.Text = videoInfo.ImdbDescription!=""?videoInfo.ImdbDescription:videoInfo.SourceURL;
                txtDirector.Text = videoInfo.ImdbDirector;
                txtGenre.Text = videoInfo.ImdbGenre;
                txtRating.Text = videoInfo.ImdbRating;
                txtYear.Text = videoInfo.ImdbYear;
            }
            else
            {
                imgVideo.ImageLocation = "";
                txtDescription.Text = "";
                txtDirector.Text = "";
                txtGenre.Text = "";
                txtRating.Text = "";
                txtYear.Text = "";
            }
        }

        private void DisplayRowVideoInfo(int rowIndex)
        {
            if (rowIndex <0 ) return;
            VideoItem videoInfo = m_videoCollection.GetVideoInfo(dgvVideos.Rows[rowIndex].Cells["Video_Path"].Value.ToString());
            DisplayVideoInfo(videoInfo);
        }

        private void SwapState(Control ctrl)
        {
            Button bt = (Button)ctrl;

            if (bt.FlatStyle == FlatStyle.Flat)
                bt.FlatStyle = FlatStyle.Standard;
            else
            {
                bt.FlatStyle = FlatStyle.Flat;
                bt.FlatAppearance.BorderColor = Color.Red;
            }
            if (bt.Tag != null)
            {
                String newTag=SetTagParameters(bt.Tag.ToString());
                UpdateCriteria(newTag, bt.FlatStyle == FlatStyle.Flat);
                RefreshVideos();
            }
        }

        private String SetTagParameters(String tag)
        {
            String result=tag;
            result = result.Replace("%thisyear", DateTime.Now.Year.ToString()+"/00/00");
            result = result.Replace("%lastyear", (DateTime.Now.Year-1).ToString()+"/00/00");
            return result;
        }

        private void GenerateDynamicComponents()
        {
            Button btn;
            int i = 1;
            foreach (String actor in m_videoCollection.GetActorsByAppearance(4))
            {
                btn = new Button();
                flowActorsContainer.Controls.Add(btn);
                btn.Parent = flowActorsContainer;
                btn.AutoSize = true;
                btn.Text = "&"+actor;
                btn.TabIndex = i;
                btn.Tag = "Actor=" + actor;
                btn.Click += new System.EventHandler(this.btSelection_Click);
                btn.FlatStyle = FlatStyle.Standard;
                btn.Show();
                i++;
            }

            i = 1;
            foreach (String genre in m_videoCollection.GetGenresByAppearance(1))
            {
                btn = new Button();
                flowGenreContainer.Controls.Add(btn);
                btn.Parent = flowGenreContainer;
                btn.AutoSize = true;
                btn.Text = "&" + genre;
                btn.TabIndex = i;
                btn.Tag = "Genre=" + genre;
                btn.Click += new System.EventHandler(this.btSelection_Click);
                btn.FlatStyle = FlatStyle.Standard;
                btn.Show();
                i++;
            }

        }

        private void btSelection_Click(object sender, EventArgs e)
        {
            SwapState((Control)sender);
        }

        private void VideoPlayMenu_Load(object sender, EventArgs e)
        {
            GenerateDynamicComponents();
            RefreshVideos();
        }

        private void dgvVideos_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            DisplayRowVideoInfo(e.RowIndex);
        }

        private void btPlay_Click(object sender, EventArgs e)
        {
            if (IsARowSelected) 
                m_parentZone.Play(dgvVideos.Rows[dgvVideos.CurrentRow.Index].Cells["Video_Path"].Value.ToString());
        }

        private void btSetup_Click(object sender, EventArgs e)
        {
            m_parentZone.InitSetup();
        }

        private void VideoPlayMenu_Shown(object sender, EventArgs e)
        {

        }

        private void VideoPlayMenu_ResizeEnd(object sender, EventArgs e)
        {
        }

        private void VideoPlayMenu_SizeChanged(object sender, EventArgs e)
        {
            
        }

        private void flowGenreNames_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
