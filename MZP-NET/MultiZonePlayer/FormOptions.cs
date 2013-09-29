using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;

using DirectShowLib;

namespace MultiZonePlayer
{
    public partial class FormOptions : Form, IPlayEvent
    {
        private String m_lastControlDevice;

        public FormOptions()
        {
            InitializeComponent();
            PopulateOutputDevices();
            PopulateInputDevices();
            PopulateControlDevices();
            //todo
            //populatezones,users
            
            DisplayZones();
            GUIShowInputs();
            GUILoadUsers();
            GUILoadPlaylists();
            GUILoadParams();
            GUILoadControls();
        }

        public int GetZoneId()
        {
            return -1;
        }

        private void FormOptions_Load(object sender, EventArgs e)
        {
            

        }
        public void m_KeyPressed(KeyDetail kd)
        {
            
            m_lastControlDevice = kd.Device;
            txtLastControlDevice.Text = m_lastControlDevice;

            DataGridViewCell cell;
            String val;
            for (int i = 0; i < dgvControl.RowCount; i++)
            {
                cell = dgvControl.Rows[i].Cells[Control_Identify.Name] as DataGridViewCell;
                val = cell.Value as String ?? "";

                if (val.ToString().Equals("Auto"))
                {
                    dgvControl.Rows[i].Cells[Control_DeviceName.Name].Value = m_lastControlDevice;
                    cell.Value = "Manual";
                    break;
                }
            }
        }

        

        private void FormOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void PopulateOutputDevices()
        {
            foreach (String deviceName in DShowUtility.SystemDeviceNameList)
            {
                ((DataGridViewComboBoxColumn)dgvZones.Columns[OutputDevice.Name]).Items.Add(deviceName);
            }
			((DataGridViewComboBoxColumn)dgvZones.Columns[OutputDevice.Name]).Items.Add("");
			((DataGridViewComboBoxColumn)dgvZones.Columns[OutputDevice.Name]).Items.Add(IniFile.DEFAULT_AUTO_DEV_NAME);
        }

        private void PopulateInputDevices()
        {
            foreach (String deviceName in MZPState.Instance.SystemInputDeviceNames.Values)
            {
                ((DataGridViewComboBoxColumn)dgvInputs.Columns[InputDeviceName.Name]).Items.Add(deviceName);
            }
        }

        private void PopulateControlDevices()
        {
            foreach (ControlDevice dev in MZPState.Instance.SystemAvailableControlDevices)
            {
                ((DataGridViewComboBoxColumn)dgvControl.Columns[Control_DeviceName.Name]).Items.Add(dev.DeviceName);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                //IniFile.IniBeginSave();
                GUISaveZones();
                GUISaveInputs();
                GUISaveUsers();
                GUISavePlaylists();
                GUISaveParams();
                GUISaveControls();
                //IniFile.IniCompleteSave();
            }
            catch (Exception ex)
            { MLog.Log(ex, this, "Error save ini"); }

            //MZPState.Initialise();
        }

        private void DisplayZones()
        {
            foreach (ZoneDetails zone in MZPState.Instance.ZoneDetails)
            {
                dgvZones.Rows.Add(zone.ZoneId, zone.ZoneName, zone.OutputKeywords, zone.OutputDeviceUserSelected, 
                    zone.PowerIndex, zone.DefaultVolumePercent, zone.CameraId, zone.AlarmZoneId, zone.AlarmAreaId, zone.ParentZoneId,
					zone.ClosureOpenCloseRelay==null?ClosureOpenCloseRelay.EnumRelayType.Undefined.ToString():
					zone.ClosureOpenCloseRelay.RelayType.ToString(), 
					zone.ClosureIdList, zone.PowerOnDelay, zone.NearbyZonesIdList, zone.TemperatureDeviceId, zone.PowerType,
					zone.Type.ToString());

                if (zone.DisplayType!="")
                {
                    dgvDisplay.Rows.Add(zone.ZoneId, zone.DisplayType, zone.DisplayConnection);
                }
            }
        }

        public void EventNextAuto()
        {
            //nothing to do here, not needed
        }

        private void GUISaveZones()
        {
            Object zoneId, zoneName;///, outputDevice, outputKeywords, cameraId, alarmZoneId; Object powerControlIndexDK, defaultVolume;

            for (int r = 0; r < dgvZones.Rows.Count; r++)
            {
                zoneId = dgvZones.Rows[r].Cells[ZoneId.Name].Value;
                zoneName = dgvZones.Rows[r].Cells[ZoneName.Name].Value;
                
               
                if (zoneId != null && zoneName != null)
                {
                    ZoneDetails zone = MZPState.Instance.GetZoneById(Convert.ToInt16(zoneId));
                    
                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_ZONES, zoneId.ToString(), "");
                    if (zone == null)
                    {
                        zone = new ZoneDetails();
                        MZPState.Instance.ZoneDetails.Add(zone);
                    }
					if (zone.ClosureOpenCloseRelay == null) 
						zone.ClosureOpenCloseRelay = new ClosureOpenCloseRelay(false);

                    zone.ZoneId = Convert.ToInt16(dgvZones.Rows[r].Cells[ZoneId.Name].Value.ToString());
                    zone.ParentZoneId = Convert.ToInt16(dgvZones.Rows[r].Cells[Zones_ParentZoneId.Name].Value ?? "-1");
                    zone.ZoneName = dgvZones.Rows[r].Cells[ZoneName.Name].Value.ToString();
                    zone.OutputKeywords = (dgvZones.Rows[r].Cells[Zones_OutputKeywords.Name].Value ?? "").ToString();
                    zone.OutputDeviceUserSelected = (dgvZones.Rows[r].Cells[OutputDevice.Name].Value ?? "").ToString();
                    zone.PowerIndex = Convert.ToInt16(dgvZones.Rows[r].Cells[Zones_PowerIndexDK.Name].Value ?? "-1");
                    zone.DefaultVolumePercent = Convert.ToInt16(dgvZones.Rows[r].Cells[Zones_DefaultVolume.Name].Value ?? "0");
                    zone.CameraId = (dgvZones.Rows[r].Cells[Zones_CameraId.Name].Value ?? "").ToString();
                    zone.AlarmZoneId = Convert.ToInt16(dgvZones.Rows[r].Cells[Zones_AlarmZoneId.Name].Value ?? "-1");
                    zone.AlarmAreaId = Convert.ToInt16(dgvZones.Rows[r].Cells[Zones_AlarmAreadId.Name].Value ?? "-1");
					zone.ClosureOpenCloseRelay.RelayType = (ClosureOpenCloseRelay.EnumRelayType)Enum.Parse(typeof(ClosureOpenCloseRelay.EnumRelayType), 
						(dgvZones.Rows[r].Cells[Zones_ClosureRelayType.Name].Value ?? "Undefined").ToString());
					zone.ClosureIdList = (dgvZones.Rows[r].Cells[Zones_ClosureIdList.Name].Value ?? "").ToString();
					zone.PowerOnDelay = Convert.ToInt16(dgvZones.Rows[r].Cells[Zones_PowerOnDelay.Name].Value ?? "0");
					zone.NearbyZonesIdList= (dgvZones.Rows[r].Cells[Zones_NearbyZoneIdList.Name].Value ?? "").ToString();
					zone.TemperatureDeviceId = (dgvZones.Rows[r].Cells[Zones_TempDeviceId.Name].Value ?? "").ToString();
					zone.PowerType = (dgvZones.Rows[r].Cells[Zones_PowerType.Name].Value ?? "").ToString();
					zone.Type = (ZoneType)Enum.Parse(typeof(ZoneType),(dgvZones.Rows[r].Cells[Zones_Type.Name].Value ?? "Undefined").ToString());
					zone.SaveStateToIni();
                }
            }

            for (int r=0; r< dgvDisplay.Rows.Count;r++)
            {
                zoneId = dgvDisplay.Rows[r].Cells[Display_ZoneID.Name].Value;
                if (zoneId != null)
                {
                    ZoneDetails zone = MZPState.Instance.GetZoneById(Convert.ToInt16(zoneId));
                    zone.DisplayConnection = dgvDisplay.Rows[r].Cells[Display_ConnectionName.Name].Value.ToString();
                    zone.DisplayType = dgvDisplay.Rows[r].Cells[Display_Type.Name].Value.ToString();
                    zone.SaveStateToIni();
                }
            }
        }

        private void GUISaveControls()
        {
            Object zoneId;
            Object controlDeviceName;
            List<ControlDevice> list = new List<ControlDevice>();

            for (int r = 0; r < dgvControl.Rows.Count; r++)
            {
                zoneId = dgvControl.Rows[r].Cells[Control_ZoneId.Name].Value;
                controlDeviceName = dgvControl.Rows[r].Cells[Control_DeviceName.Name].Value ?? "";
                if (controlDeviceName.ToString() != "")
                    list.Add(new ControlDevice(Convert.ToInt16(zoneId), controlDeviceName.ToString(), "not used"));
            }
            ControlDevice.SaveToIni(list);
			MZPState.Instance.LoadSystemAndUserControls();
        }

        private void GUILoadControls()
        {
            foreach (ControlDevice ctrl in MZPState.Instance.IniControlDevices)
            {
                    dgvControl.Rows.Add(ctrl.ZoneId, ctrl.DeviceName, ctrl.DisplayName);
            }
        }

        private void GUISavePlaylists()
        {
            Object objUserId;
            Object objPlayListPath;
            Hashtable hIndex= new Hashtable();
            String userId;
                int index;

            for (int r = 0; r < dgvMusic.Rows.Count; r++)
            {
                objPlayListPath = dgvMusic.Rows[r].Cells[Playlist_FullPath.Name].Value;
                objUserId = dgvMusic.Rows[r].Cells[Playlist_UserId.Name].Value;

                if (objPlayListPath != null)
                {
                    if (objUserId == null)
                        userId = "0";
                    else
                        userId = objUserId.ToString();

                    if (hIndex[userId] == null) hIndex.Add(userId, "0");
                    index = Convert.ToInt32(hIndex[userId]);

                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_PLAYLIST + userId, index.ToString(), objPlayListPath.ToString());

                    index++;
                    hIndex[userId] = index.ToString();
                }
            }

            List<MoodMusic> moodlist = new List<MoodMusic>();
            Object name, authors, genres, ratings, ageinweeks, logical, group,topcount,random,numericcode;
            for (int r = 0; r < dgvMusicMood.Rows.Count; r++)
            {
                name = dgvMusicMood.Rows[r].Cells[Music_MoodName.Name].Value??"";
                authors = dgvMusicMood.Rows[r].Cells[Music_MoodAuthors.Name].Value ?? "";
                genres = dgvMusicMood.Rows[r].Cells[Music_MoodGenres.Name].Value ?? "";
                ratings = dgvMusicMood.Rows[r].Cells[Music_MoodRatings.Name].Value ?? "";
                ageinweeks = dgvMusicMood.Rows[r].Cells[Music_AgeInWeeks.Name].Value ?? "";
                logical = dgvMusicMood.Rows[r].Cells[MusicMood_LogicalOperation.Name].Value ?? "";
                group = dgvMusicMood.Rows[r].Cells[MusicMood_GroupByTop.Name].Value ?? "";
                topcount = dgvMusicMood.Rows[r].Cells[MusicMood_GroupByTopCount.Name].Value ?? "";
                random = dgvMusicMood.Rows[r].Cells[MusicMood_Random.Name].Value ?? "";
                numericcode = dgvMusicMood.Rows[r].Cells[MusicMood_NumericCode.Name].Value ?? "";
                if (!name.ToString().Equals(""))
                {
                    moodlist.Add(new MoodMusic(r, name.ToString(), genres.ToString(), authors.ToString(), ratings.ToString(), ageinweeks.ToString(), 
                        logical.ToString(), group.ToString(), topcount.ToString(), random.ToString(), numericcode.ToString()));
                }
            }
            MoodMusic.SaveToIni(moodlist);

            List<MusicScheduleEntry> schedulelist = new List<MusicScheduleEntry>();
            Object zoneid, starttime, endtime, weekday, mood;
            for (int r = 0; r < dgvMusicSchedule.Rows.Count; r++)
            {
                zoneid = dgvMusicSchedule.Rows[r].Cells[Music_ScheduleZoneId.Name].Value ?? "";
                starttime = dgvMusicSchedule.Rows[r].Cells[Music_ScheduleStartTime.Name].Value ?? "";
                endtime = dgvMusicSchedule.Rows[r].Cells[Music_ScheduleEndTime.Name].Value ?? "";
                weekday = dgvMusicSchedule.Rows[r].Cells[Music_ScheduleWeekDay.Name].Value ?? "";
                mood = dgvMusicSchedule.Rows[r].Cells[Music_ScheduleMood.Name].Value ?? "";
                if (!zoneid.ToString().Equals(""))
                {
                    schedulelist.Add(new MusicScheduleEntry(Convert.ToInt16(zoneid.ToString()), starttime.ToString(), endtime.ToString(), weekday.ToString(), mood.ToString()));
                }
            }
            MusicScheduleEntry.SaveToIni(schedulelist);
        }

        private void GUISaveInputs()
        {
            Object inputId;
            Object inputName;
            Object inputType;
            Object inZoneId;
            Object inputDevice;

            for (int r = 0; r < dgvInputs.Rows.Count; r++)
            {
                inputId = dgvInputs.Rows[r].Cells[InputId.Name].Value;
                inputName = dgvInputs.Rows[r].Cells[InputName.Name].Value;
                inputType = dgvInputs.Rows[r].Cells[InputTypeId_Inputs.Name].Value;
                inZoneId = dgvInputs.Rows[r].Cells[InZoneId.Name].Value;
                inputDevice = dgvInputs.Rows[r].Cells[IniFile.INI_SECTION_INPUTDEVICENAME].Value;

                if (inputId != null && inputType != null && inputDevice != null)
                {
                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_INPUTDEVICE + inputId.ToString(), "InputName", inputName.ToString());
                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_INPUTDEVICE + inputId.ToString(), "InputTypeId_Inputs", inputType.ToString());
                    if (inZoneId != null)
                    {
                        IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_INPUTS, inZoneId.ToString(), inputDevice.ToString());
                        IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_INPUTDEVICE + inputId.ToString(), "InZoneId", inZoneId.ToString());
                    }
                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_INPUTDEVICE + inputId.ToString(), "InputDeviceName", inputDevice.ToString());
                }
            }
        }

        private void GUIShowInputs()
        {
            InputEntry entry;
            foreach (int index in MZPState.Instance.ZoneInputDeviceEntries.Keys)
            {
                entry = MZPState.Instance.ZoneInputDeviceEntries[index] as InputEntry;
                dgvInputs.Rows.Add(index, entry.inputName, entry.inputType, entry.inputInZoneId, entry.inputDeviceName);
            }
        }

        private void GUISaveUsers()
        {
            String userId;
            String userName;
            String userCode;

            for (int r = 0; r < dgvUsers.Rows.Count; r++)
            {
                userId = (String)dgvUsers.Rows[r].Cells[UserId.Name].Value;
                userName = (String)dgvUsers.Rows[r].Cells[UserName.Name].Value;
                userCode = (String)dgvUsers.Rows[r].Cells[UserCode.Name].Value;

                if (userId != null && userCode != null)
                {
                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_USERS + userId.ToString(), "UserId", userId);
                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_USERS + userId.ToString(), "UserName", userName);
                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_USERS + userId.ToString(), "UserCode", userCode);
                }
            }
        }

        private void GUILoadUsers()
        {
            Users usr;
            IDictionaryEnumerator enumerator = MZPState.Instance.iniUserList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                usr = enumerator.Value as Users;
                dgvUsers.Rows.Add(usr.Id, usr.Name, usr.Code);
            }
        }

        private void GUILoadPlaylists()
        {
            String value;
            bool loop;
            int r;

            for (int i = 0; i < MZPState.Instance.iniUserList.Count; i++)
            {
                r = 0;
                loop = true;
                
                while (loop)
                {
                    value = IniFile.IniReadValue(IniFile.INI_SECTION_PLAYLIST + i.ToString(), r.ToString());
                    if (value != "")
                    {
                        dgvMusic.Rows.Add(i.ToString(), value);
                    }
                    else loop = false;
                    r++;
                }
            }

            foreach (MoodMusic mood in MZPState.Instance.MoodMusicList)
            {
                dgvMusicMood.Rows.Add(mood.Name, mood.GenreList(), mood.AuthorList(), mood.RatingList(), mood.AgeInWeeksList(), 
                    mood.LogicalSearchOperator.ToString(), mood.IsGroupByTop, mood.TopCount, mood.IsRandom, mood.NumericCode);
            }

            foreach (MusicScheduleEntry entry in MZPState.Instance.MusicScheduleList)
            {
                dgvMusicSchedule.Rows.Add(entry.ZoneId.ToString(), entry.StartTime, entry.EndTime, entry.WeekDays, entry.Mood);
            }
        }

        private void GUISaveParams()
        {
            String pname;
            String pvalue;

            for (int r = 0; r < dgvParams.Rows.Count; r++)
            {
                pname = (String)dgvParams.Rows[r].Cells[ParamName.Name].Value;
                pvalue = (String)dgvParams.Rows[r].Cells[ParamValue.Name].Value;
				string[] param;
				//apply params live
				for (int i = 0; i < IniFile.PARAMS.Length; i++)
				{
					param = IniFile.PARAMS[i] as String[];
					if (param[0] == pname)
						param[1] = pvalue;
				}
				//then save
                if (pname != null)
                {
                    IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_PARAMS, pname,pvalue);
                }
            }
        }

        private void GUILoadParams()
        {
            String pname, pvalue, pdesc;
            String[] param;

            for (int r = 0; r < IniFile.PARAMS.Length; r++)
            {
                param = IniFile.PARAMS[r] as String[];
                pname = param[0] as String;
				if (param.Length >= 3)
					pdesc = param[2] as String;
				else pdesc = "";
                pvalue = IniFile.IniReadValue(IniFile.INI_SECTION_PARAMS, pname);
                if (pvalue == "")
                    pvalue = param[1];//default value
                dgvParams.Rows.Add(pname,pvalue,pdesc);
            }
        }

        

        private void dgvInputs_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dgvInputs.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.Yellow;
        }

        private void dgvZones_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dgvZones.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.Yellow;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void dgvMusic_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            String fullPath;

            if (dgvMusic.Columns[e.ColumnIndex].HeaderText.Equals(Playlist_OpenPlaylist.HeaderText))// "Add Playlist")
            {
                openFileDialog1.DefaultExt= IniFile.DEFAULT_EXT_PLAYLIST;
                openFileDialog1.Filter = "Playlist | *" + IniFile.DEFAULT_EXT_PLAYLIST; 
                openFileDialog1.FileName = (String)dgvMusic.Rows[e.RowIndex].Cells[1].Value;
                openFileDialog1.ShowDialog();
                fullPath = openFileDialog1.FileName;
                dgvMusic.Rows[e.RowIndex].Cells[1].Value = fullPath;
            }

            if (dgvMusic.Columns[e.ColumnIndex].HeaderText.Equals(Playlist_OpenFolder.HeaderText))// == "Add Directory")
            {
                folderBrowserDialog1.SelectedPath = (String)dgvMusic.Rows[e.RowIndex].Cells[1].Value;
                folderBrowserDialog1.ShowDialog();
                fullPath = folderBrowserDialog1.SelectedPath;
                dgvMusic.Rows[e.RowIndex].Cells[1].Value = fullPath;
            }
        }

        private void dgvControl_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dgvControl.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.Yellow;
        }

        private void dgvZones_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvZones.Columns[e.ColumnIndex].HeaderText == "Test")
            {
                TestOutputDevice(dgvZones.Rows[e.RowIndex].Cells[OutputDevice.Name].Value.ToString(), 
                    dgvZones.Rows[e.RowIndex].Cells[Zones_OutputKeywords.Name].Value.ToString(),
                    Convert.ToInt16(dgvZones.Rows[e.RowIndex].Cells[Zones_DefaultVolume.Name].Value.ToString()));
            }
        }

        private void dgvControl_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void TestOutputDevice(String outDevice, String keywords, int defaultVolume)
        {
            outDevice = ZoneDetails.GetOutputDeviceNameAutocompleted(outDevice, keywords);
            //DCPlayer dcPlay = new DCPlayer(this, outDevice, IniFile.CurrentPath() + IniFile.TEST_FILE_NAME, Metadata.ZoneDetails.GetDefaultVolume(defaultVolume));
        }

        private void txtLastControlDevice_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void dgvMusicMood_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

		private void button1_Click_1(object sender, EventArgs e)
		{

		}
    }
}
