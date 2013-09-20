namespace MultiZonePlayer
{
    partial class FormOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.btnSave = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.tabParams = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.dgvZones = new System.Windows.Forms.DataGridView();
			this.tabControl = new System.Windows.Forms.TabPage();
			this.label1 = new System.Windows.Forms.Label();
			this.txtLastControlDevice = new System.Windows.Forms.TextBox();
			this.dgvControl = new System.Windows.Forms.DataGridView();
			this.Control_ZoneId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Control_DeviceName = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Control_Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Control_Identify = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.tabMusic = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.dgvMusic = new System.Windows.Forms.DataGridView();
			this.Playlist_UserId = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Playlist_FullPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Playlist_Mood = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Playlist_OpenPlaylist = new System.Windows.Forms.DataGridViewButtonColumn();
			this.Playlist_OpenFolder = new System.Windows.Forms.DataGridViewButtonColumn();
			this.dgvMusicMood = new System.Windows.Forms.DataGridView();
			this.Music_MoodName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Music_MoodGenres = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Music_MoodAuthors = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Music_MoodRatings = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Music_AgeInWeeks = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.MusicMood_LogicalOperation = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.MusicMood_GroupByTop = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.MusicMood_GroupByTopCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.MusicMood_Random = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.MusicMood_NumericCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvMusicSchedule = new System.Windows.Forms.DataGridView();
			this.Music_ScheduleZoneId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Music_ScheduleStartTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Music_ScheduleEndTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Music_ScheduleWeekDay = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Music_ScheduleMood = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tabVideo = new System.Windows.Forms.TabPage();
			this.dgvDisplay = new System.Windows.Forms.DataGridView();
			this.Display_ZoneID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Display_Type = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Display_ConnectionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dgvVideo = new System.Windows.Forms.DataGridView();
			this.Video_UserId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Video_PlaylistFullPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tabInput = new System.Windows.Forms.TabPage();
			this.dgvInputs = new System.Windows.Forms.DataGridView();
			this.InputId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.InputName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.InputTypeId_Inputs = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.InZoneId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.InputDeviceName = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.tabUsers = new System.Windows.Forms.TabPage();
			this.dgvUsers = new System.Windows.Forms.DataGridView();
			this.UserId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.UserCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.button1 = new System.Windows.Forms.Button();
			this.dgvParams = new System.Windows.Forms.DataGridView();
			this.ParamName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ParamValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Param_Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ZoneId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ZoneName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_OutputKeywords = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.OutputDevice = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Zones_PowerIndexDK = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_DefaultVolume = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_CameraId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_AlarmZoneId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_AlarmAreadId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_ParentZoneId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_ClosureRelayType = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Zones_ClosureIdList = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_PowerOnDelay = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_NearbyZoneIdList = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_TempDeviceId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Zones_PowerType = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.tabParams.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvZones)).BeginInit();
			this.tabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvControl)).BeginInit();
			this.tabMusic.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvMusic)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvMusicMood)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvMusicSchedule)).BeginInit();
			this.tabVideo.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvDisplay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvVideo)).BeginInit();
			this.tabInput.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvInputs)).BeginInit();
			this.tabUsers.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
			this.tabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvParams)).BeginInit();
			this.SuspendLayout();
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(3, 437);
			this.btnSave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(83, 34);
			this.btnSave.TabIndex = 7;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// tabParams
			// 
			this.tabParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabParams.Controls.Add(this.tabPage1);
			this.tabParams.Controls.Add(this.tabControl);
			this.tabParams.Controls.Add(this.tabMusic);
			this.tabParams.Controls.Add(this.tabVideo);
			this.tabParams.Controls.Add(this.tabInput);
			this.tabParams.Controls.Add(this.tabUsers);
			this.tabParams.Controls.Add(this.tabPage2);
			this.tabParams.Location = new System.Drawing.Point(3, 14);
			this.tabParams.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabParams.Name = "tabParams";
			this.tabParams.SelectedIndex = 0;
			this.tabParams.Size = new System.Drawing.Size(1427, 418);
			this.tabParams.TabIndex = 1;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.dgvZones);
			this.tabPage1.Location = new System.Drawing.Point(4, 25);
			this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabPage1.Size = new System.Drawing.Size(1419, 389);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Zones";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// dgvZones
			// 
			this.dgvZones.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dgvZones.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvZones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvZones.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ZoneId,
            this.ZoneName,
            this.Zones_OutputKeywords,
            this.OutputDevice,
            this.Zones_PowerIndexDK,
            this.Zones_DefaultVolume,
            this.Zones_CameraId,
            this.Zones_AlarmZoneId,
            this.Zones_AlarmAreadId,
            this.Zones_ParentZoneId,
            this.Zones_ClosureRelayType,
            this.Zones_ClosureIdList,
            this.Zones_PowerOnDelay,
            this.Zones_NearbyZoneIdList,
            this.Zones_TempDeviceId,
            this.Zones_PowerType});
			this.dgvZones.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvZones.Location = new System.Drawing.Point(3, 2);
			this.dgvZones.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dgvZones.Name = "dgvZones";
			this.dgvZones.RowTemplate.Height = 24;
			this.dgvZones.Size = new System.Drawing.Size(1413, 385);
			this.dgvZones.TabIndex = 6;
			this.dgvZones.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvZones_CellContentClick);
			this.dgvZones.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvZones_DataError);
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.label1);
			this.tabControl.Controls.Add(this.txtLastControlDevice);
			this.tabControl.Controls.Add(this.dgvControl);
			this.tabControl.Location = new System.Drawing.Point(4, 25);
			this.tabControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabControl.Name = "tabControl";
			this.tabControl.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabControl.Size = new System.Drawing.Size(1419, 389);
			this.tabControl.TabIndex = 5;
			this.tabControl.Text = "Control";
			this.tabControl.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(5, 283);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 17);
			this.label1.TabIndex = 2;
			this.label1.Text = "Control Device";
			// 
			// txtLastControlDevice
			// 
			this.txtLastControlDevice.Location = new System.Drawing.Point(128, 281);
			this.txtLastControlDevice.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtLastControlDevice.Name = "txtLastControlDevice";
			this.txtLastControlDevice.Size = new System.Drawing.Size(857, 22);
			this.txtLastControlDevice.TabIndex = 1;
			this.txtLastControlDevice.TextChanged += new System.EventHandler(this.txtLastControlDevice_TextChanged);
			// 
			// dgvControl
			// 
			this.dgvControl.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dgvControl.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvControl.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Control_ZoneId,
            this.Control_DeviceName,
            this.Control_Description,
            this.Control_Identify});
			this.dgvControl.Location = new System.Drawing.Point(5, 6);
			this.dgvControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dgvControl.Name = "dgvControl";
			this.dgvControl.RowTemplate.Height = 24;
			this.dgvControl.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvControl.Size = new System.Drawing.Size(1389, 268);
			this.dgvControl.TabIndex = 0;
			this.dgvControl.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvControl_CellContentClick);
			this.dgvControl.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvControl_DataError);
			// 
			// Control_ZoneId
			// 
			this.Control_ZoneId.HeaderText = "ZoneId";
			this.Control_ZoneId.Name = "Control_ZoneId";
			this.Control_ZoneId.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Control_ZoneId.Width = 77;
			// 
			// Control_DeviceName
			// 
			this.Control_DeviceName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Control_DeviceName.HeaderText = "DeviceName";
			this.Control_DeviceName.Name = "Control_DeviceName";
			this.Control_DeviceName.Width = 94;
			// 
			// Control_Description
			// 
			this.Control_Description.HeaderText = "Description";
			this.Control_Description.Name = "Control_Description";
			this.Control_Description.Width = 104;
			// 
			// Control_Identify
			// 
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.Control_Identify.DefaultCellStyle = dataGridViewCellStyle2;
			this.Control_Identify.HeaderText = "Identify";
			this.Control_Identify.Items.AddRange(new object[] {
            "Manual",
            "Auto"});
			this.Control_Identify.Name = "Control_Identify";
			this.Control_Identify.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Control_Identify.Width = 59;
			// 
			// tabMusic
			// 
			this.tabMusic.Controls.Add(this.tableLayoutPanel1);
			this.tabMusic.Location = new System.Drawing.Point(4, 25);
			this.tabMusic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabMusic.Name = "tabMusic";
			this.tabMusic.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabMusic.Size = new System.Drawing.Size(1419, 389);
			this.tabMusic.TabIndex = 1;
			this.tabMusic.Text = "Music";
			this.tabMusic.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.dgvMusic, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.dgvMusicMood, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.dgvMusicSchedule, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 2);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1413, 385);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// dgvMusic
			// 
			this.dgvMusic.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvMusic.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Playlist_UserId,
            this.Playlist_FullPath,
            this.Playlist_Mood,
            this.Playlist_OpenPlaylist,
            this.Playlist_OpenFolder});
			this.dgvMusic.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvMusic.Location = new System.Drawing.Point(3, 2);
			this.dgvMusic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dgvMusic.Name = "dgvMusic";
			this.dgvMusic.RowTemplate.Height = 24;
			this.dgvMusic.Size = new System.Drawing.Size(1407, 73);
			this.dgvMusic.TabIndex = 0;
			this.dgvMusic.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMusic_CellContentClick);
			// 
			// Playlist_UserId
			// 
			this.Playlist_UserId.HeaderText = "UserId";
			this.Playlist_UserId.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
			this.Playlist_UserId.Name = "Playlist_UserId";
			// 
			// Playlist_FullPath
			// 
			this.Playlist_FullPath.HeaderText = "PlaylistFullPath";
			this.Playlist_FullPath.Name = "Playlist_FullPath";
			this.Playlist_FullPath.Width = 300;
			// 
			// Playlist_Mood
			// 
			this.Playlist_Mood.HeaderText = "Mood";
			this.Playlist_Mood.Name = "Playlist_Mood";
			// 
			// Playlist_OpenPlaylist
			// 
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.ButtonFace;
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Playlist_OpenPlaylist.DefaultCellStyle = dataGridViewCellStyle3;
			this.Playlist_OpenPlaylist.HeaderText = "Add Playlist";
			this.Playlist_OpenPlaylist.Name = "Playlist_OpenPlaylist";
			this.Playlist_OpenPlaylist.Text = "Open";
			// 
			// Playlist_OpenFolder
			// 
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.Playlist_OpenFolder.DefaultCellStyle = dataGridViewCellStyle4;
			this.Playlist_OpenFolder.HeaderText = "Add Directory";
			this.Playlist_OpenFolder.Name = "Playlist_OpenFolder";
			// 
			// dgvMusicMood
			// 
			this.dgvMusicMood.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvMusicMood.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Music_MoodName,
            this.Music_MoodGenres,
            this.Music_MoodAuthors,
            this.Music_MoodRatings,
            this.Music_AgeInWeeks,
            this.MusicMood_LogicalOperation,
            this.MusicMood_GroupByTop,
            this.MusicMood_GroupByTopCount,
            this.MusicMood_Random,
            this.MusicMood_NumericCode});
			this.dgvMusicMood.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvMusicMood.Location = new System.Drawing.Point(4, 81);
			this.dgvMusicMood.Margin = new System.Windows.Forms.Padding(4);
			this.dgvMusicMood.Name = "dgvMusicMood";
			this.dgvMusicMood.Size = new System.Drawing.Size(1405, 146);
			this.dgvMusicMood.TabIndex = 1;
			this.dgvMusicMood.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMusicMood_CellContentClick);
			// 
			// Music_MoodName
			// 
			this.Music_MoodName.HeaderText = "MoodName";
			this.Music_MoodName.Name = "Music_MoodName";
			// 
			// Music_MoodGenres
			// 
			this.Music_MoodGenres.HeaderText = "MoodGenres";
			this.Music_MoodGenres.Name = "Music_MoodGenres";
			this.Music_MoodGenres.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// Music_MoodAuthors
			// 
			this.Music_MoodAuthors.HeaderText = "MoodAuthors";
			this.Music_MoodAuthors.Name = "Music_MoodAuthors";
			// 
			// Music_MoodRatings
			// 
			this.Music_MoodRatings.HeaderText = "MoodRatings";
			this.Music_MoodRatings.Name = "Music_MoodRatings";
			// 
			// Music_AgeInWeeks
			// 
			this.Music_AgeInWeeks.HeaderText = "AgeInWeeks";
			this.Music_AgeInWeeks.Name = "Music_AgeInWeeks";
			// 
			// MusicMood_LogicalOperation
			// 
			this.MusicMood_LogicalOperation.HeaderText = "LogicalOperation";
			this.MusicMood_LogicalOperation.Items.AddRange(new object[] {
            "Union",
            "Intersect"});
			this.MusicMood_LogicalOperation.Name = "MusicMood_LogicalOperation";
			// 
			// MusicMood_GroupByTop
			// 
			this.MusicMood_GroupByTop.HeaderText = "GroupByTop";
			this.MusicMood_GroupByTop.Name = "MusicMood_GroupByTop";
			// 
			// MusicMood_GroupByTopCount
			// 
			this.MusicMood_GroupByTopCount.HeaderText = "TopCount";
			this.MusicMood_GroupByTopCount.Name = "MusicMood_GroupByTopCount";
			// 
			// MusicMood_Random
			// 
			this.MusicMood_Random.HeaderText = "Random";
			this.MusicMood_Random.Name = "MusicMood_Random";
			// 
			// MusicMood_NumericCode
			// 
			this.MusicMood_NumericCode.HeaderText = "NumericCode";
			this.MusicMood_NumericCode.Name = "MusicMood_NumericCode";
			// 
			// dgvMusicSchedule
			// 
			this.dgvMusicSchedule.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvMusicSchedule.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Music_ScheduleZoneId,
            this.Music_ScheduleStartTime,
            this.Music_ScheduleEndTime,
            this.Music_ScheduleWeekDay,
            this.Music_ScheduleMood});
			this.dgvMusicSchedule.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvMusicSchedule.Location = new System.Drawing.Point(4, 235);
			this.dgvMusicSchedule.Margin = new System.Windows.Forms.Padding(4);
			this.dgvMusicSchedule.Name = "dgvMusicSchedule";
			this.dgvMusicSchedule.Size = new System.Drawing.Size(1405, 146);
			this.dgvMusicSchedule.TabIndex = 2;
			// 
			// Music_ScheduleZoneId
			// 
			this.Music_ScheduleZoneId.HeaderText = "ZoneId";
			this.Music_ScheduleZoneId.Name = "Music_ScheduleZoneId";
			// 
			// Music_ScheduleStartTime
			// 
			this.Music_ScheduleStartTime.HeaderText = "StartTime";
			this.Music_ScheduleStartTime.Name = "Music_ScheduleStartTime";
			// 
			// Music_ScheduleEndTime
			// 
			this.Music_ScheduleEndTime.HeaderText = "EndTime";
			this.Music_ScheduleEndTime.Name = "Music_ScheduleEndTime";
			// 
			// Music_ScheduleWeekDay
			// 
			this.Music_ScheduleWeekDay.HeaderText = "WeekDay";
			this.Music_ScheduleWeekDay.Name = "Music_ScheduleWeekDay";
			// 
			// Music_ScheduleMood
			// 
			this.Music_ScheduleMood.HeaderText = "Mood";
			this.Music_ScheduleMood.Name = "Music_ScheduleMood";
			// 
			// tabVideo
			// 
			this.tabVideo.Controls.Add(this.dgvDisplay);
			this.tabVideo.Controls.Add(this.dgvVideo);
			this.tabVideo.Location = new System.Drawing.Point(4, 25);
			this.tabVideo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabVideo.Name = "tabVideo";
			this.tabVideo.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabVideo.Size = new System.Drawing.Size(1419, 389);
			this.tabVideo.TabIndex = 6;
			this.tabVideo.Text = "Video";
			this.tabVideo.UseVisualStyleBackColor = true;
			// 
			// dgvDisplay
			// 
			this.dgvDisplay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvDisplay.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Display_ZoneID,
            this.Display_Type,
            this.Display_ConnectionName});
			this.dgvDisplay.Location = new System.Drawing.Point(4, 217);
			this.dgvDisplay.Margin = new System.Windows.Forms.Padding(4);
			this.dgvDisplay.Name = "dgvDisplay";
			this.dgvDisplay.Size = new System.Drawing.Size(599, 164);
			this.dgvDisplay.TabIndex = 1;
			// 
			// Display_ZoneID
			// 
			this.Display_ZoneID.HeaderText = "ZoneId";
			this.Display_ZoneID.Name = "Display_ZoneID";
			// 
			// Display_Type
			// 
			this.Display_Type.HeaderText = "Type";
			this.Display_Type.Items.AddRange(new object[] {
            "LGTV",
            "XBMC"});
			this.Display_Type.Name = "Display_Type";
			// 
			// Display_ConnectionName
			// 
			this.Display_ConnectionName.HeaderText = "Connection";
			this.Display_ConnectionName.Name = "Display_ConnectionName";
			// 
			// dgvVideo
			// 
			this.dgvVideo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvVideo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Video_UserId,
            this.Video_PlaylistFullPath});
			this.dgvVideo.Location = new System.Drawing.Point(3, 2);
			this.dgvVideo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dgvVideo.Name = "dgvVideo";
			this.dgvVideo.RowTemplate.Height = 24;
			this.dgvVideo.Size = new System.Drawing.Size(1411, 198);
			this.dgvVideo.TabIndex = 0;
			// 
			// Video_UserId
			// 
			this.Video_UserId.HeaderText = "UserId";
			this.Video_UserId.Name = "Video_UserId";
			// 
			// Video_PlaylistFullPath
			// 
			this.Video_PlaylistFullPath.HeaderText = "Playlist";
			this.Video_PlaylistFullPath.Name = "Video_PlaylistFullPath";
			// 
			// tabInput
			// 
			this.tabInput.Controls.Add(this.dgvInputs);
			this.tabInput.Location = new System.Drawing.Point(4, 25);
			this.tabInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabInput.Name = "tabInput";
			this.tabInput.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabInput.Size = new System.Drawing.Size(1419, 389);
			this.tabInput.TabIndex = 2;
			this.tabInput.Text = "Input";
			this.tabInput.UseVisualStyleBackColor = true;
			// 
			// dgvInputs
			// 
			this.dgvInputs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvInputs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.InputId,
            this.InputName,
            this.InputTypeId_Inputs,
            this.InZoneId,
            this.InputDeviceName});
			this.dgvInputs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvInputs.Location = new System.Drawing.Point(3, 2);
			this.dgvInputs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dgvInputs.Name = "dgvInputs";
			this.dgvInputs.RowTemplate.Height = 24;
			this.dgvInputs.Size = new System.Drawing.Size(1413, 385);
			this.dgvInputs.TabIndex = 0;
			this.dgvInputs.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvInputs_DataError);
			// 
			// InputId
			// 
			this.InputId.HeaderText = "InputId";
			this.InputId.Name = "InputId";
			this.InputId.Width = 50;
			// 
			// InputName
			// 
			this.InputName.HeaderText = "InputName";
			this.InputName.Name = "InputName";
			// 
			// InputTypeId_Inputs
			// 
			this.InputTypeId_Inputs.HeaderText = "InputTypeId";
			this.InputTypeId_Inputs.Items.AddRange(new object[] {
            "Radio",
            "TV",
            "DVD",
            "AUX",
            "Microphone",
            "Other"});
			this.InputTypeId_Inputs.Name = "InputTypeId_Inputs";
			// 
			// InZoneId
			// 
			dataGridViewCellStyle5.NullValue = "N/A";
			this.InZoneId.DefaultCellStyle = dataGridViewCellStyle5;
			this.InZoneId.HeaderText = "InZoneId";
			this.InZoneId.Name = "InZoneId";
			this.InZoneId.Width = 75;
			// 
			// InputDeviceName
			// 
			this.InputDeviceName.HeaderText = "InputDeviceName";
			this.InputDeviceName.Name = "InputDeviceName";
			this.InputDeviceName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.InputDeviceName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.InputDeviceName.Width = 500;
			// 
			// tabUsers
			// 
			this.tabUsers.Controls.Add(this.dgvUsers);
			this.tabUsers.Location = new System.Drawing.Point(4, 25);
			this.tabUsers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabUsers.Name = "tabUsers";
			this.tabUsers.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabUsers.Size = new System.Drawing.Size(1419, 389);
			this.tabUsers.TabIndex = 3;
			this.tabUsers.Text = "Users";
			this.tabUsers.UseVisualStyleBackColor = true;
			// 
			// dgvUsers
			// 
			this.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UserId,
            this.UserName,
            this.UserCode});
			this.dgvUsers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvUsers.Location = new System.Drawing.Point(3, 2);
			this.dgvUsers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dgvUsers.Name = "dgvUsers";
			this.dgvUsers.RowTemplate.Height = 24;
			this.dgvUsers.Size = new System.Drawing.Size(1413, 385);
			this.dgvUsers.TabIndex = 0;
			// 
			// UserId
			// 
			this.UserId.HeaderText = "UserId";
			this.UserId.Name = "UserId";
			// 
			// UserName
			// 
			this.UserName.HeaderText = "UserName";
			this.UserName.Name = "UserName";
			// 
			// UserCode
			// 
			this.UserCode.HeaderText = "UserCode";
			this.UserCode.Name = "UserCode";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.button1);
			this.tabPage2.Controls.Add(this.dgvParams);
			this.tabPage2.Location = new System.Drawing.Point(4, 25);
			this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabPage2.Size = new System.Drawing.Size(1419, 389);
			this.tabPage2.TabIndex = 4;
			this.tabPage2.Text = "Parameters";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(17, 355);
			this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(93, 30);
			this.button1.TabIndex = 3;
			this.button1.Text = "LocateApp";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// dgvParams
			// 
			this.dgvParams.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dgvParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvParams.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ParamName,
            this.ParamValue,
            this.Param_Description});
			this.dgvParams.Location = new System.Drawing.Point(17, 17);
			this.dgvParams.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dgvParams.Name = "dgvParams";
			this.dgvParams.RowTemplate.Height = 24;
			this.dgvParams.Size = new System.Drawing.Size(1256, 334);
			this.dgvParams.TabIndex = 2;
			// 
			// ParamName
			// 
			this.ParamName.HeaderText = "Name";
			this.ParamName.Name = "ParamName";
			this.ParamName.Width = 70;
			// 
			// ParamValue
			// 
			this.ParamValue.HeaderText = "Value";
			this.ParamValue.Name = "ParamValue";
			this.ParamValue.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.ParamValue.Width = 69;
			// 
			// Param_Description
			// 
			this.Param_Description.HeaderText = "Description";
			this.Param_Description.Name = "Param_Description";
			this.Param_Description.Width = 104;
			// 
			// ZoneId
			// 
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.ZoneId.DefaultCellStyle = dataGridViewCellStyle1;
			this.ZoneId.HeaderText = "ZoneId";
			this.ZoneId.Name = "ZoneId";
			this.ZoneId.Width = 77;
			// 
			// ZoneName
			// 
			this.ZoneName.HeaderText = "ZoneName";
			this.ZoneName.Name = "ZoneName";
			this.ZoneName.Width = 103;
			// 
			// Zones_OutputKeywords
			// 
			this.Zones_OutputKeywords.HeaderText = "OutputKeys";
			this.Zones_OutputKeywords.Name = "Zones_OutputKeywords";
			this.Zones_OutputKeywords.Width = 107;
			// 
			// OutputDevice
			// 
			this.OutputDevice.HeaderText = "OutputDev";
			this.OutputDevice.Name = "OutputDevice";
			this.OutputDevice.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.OutputDevice.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.OutputDevice.Width = 101;
			// 
			// Zones_PowerIndexDK
			// 
			this.Zones_PowerIndexDK.HeaderText = "PwrIndex";
			this.Zones_PowerIndexDK.Name = "Zones_PowerIndexDK";
			this.Zones_PowerIndexDK.Width = 89;
			// 
			// Zones_DefaultVolume
			// 
			this.Zones_DefaultVolume.HeaderText = "DefaultVol%";
			this.Zones_DefaultVolume.Name = "Zones_DefaultVolume";
			this.Zones_DefaultVolume.Width = 110;
			// 
			// Zones_CameraId
			// 
			this.Zones_CameraId.HeaderText = "CameraId";
			this.Zones_CameraId.Name = "Zones_CameraId";
			this.Zones_CameraId.Width = 93;
			// 
			// Zones_AlarmZoneId
			// 
			this.Zones_AlarmZoneId.HeaderText = "AlarmZoneId";
			this.Zones_AlarmZoneId.Name = "Zones_AlarmZoneId";
			this.Zones_AlarmZoneId.Width = 113;
			// 
			// Zones_AlarmAreadId
			// 
			this.Zones_AlarmAreadId.HeaderText = "AlarmAreaId";
			this.Zones_AlarmAreadId.Name = "Zones_AlarmAreadId";
			this.Zones_AlarmAreadId.Width = 110;
			// 
			// Zones_ParentZoneId
			// 
			this.Zones_ParentZoneId.HeaderText = "ParentId";
			this.Zones_ParentZoneId.Name = "Zones_ParentZoneId";
			this.Zones_ParentZoneId.Width = 86;
			// 
			// Zones_ClosureRelayType
			// 
			this.Zones_ClosureRelayType.HeaderText = "ClosureType";
			this.Zones_ClosureRelayType.Items.AddRange(new object[] {
            "Undefined",
            "NormalOpen",
            "NormalClosed",
            "Button"});
			this.Zones_ClosureRelayType.Name = "Zones_ClosureRelayType";
			this.Zones_ClosureRelayType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Zones_ClosureRelayType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.Zones_ClosureRelayType.Width = 113;
			// 
			// Zones_ClosureIdList
			// 
			this.Zones_ClosureIdList.HeaderText = "ClosureIds";
			this.Zones_ClosureIdList.Name = "Zones_ClosureIdList";
			this.Zones_ClosureIdList.Width = 99;
			// 
			// Zones_PowerOnDelay
			// 
			this.Zones_PowerOnDelay.HeaderText = "PowerOnDelay";
			this.Zones_PowerOnDelay.Name = "Zones_PowerOnDelay";
			this.Zones_PowerOnDelay.Width = 127;
			// 
			// Zones_NearbyZoneIdList
			// 
			this.Zones_NearbyZoneIdList.HeaderText = "NearbyZones";
			this.Zones_NearbyZoneIdList.Name = "Zones_NearbyZoneIdList";
			this.Zones_NearbyZoneIdList.Width = 119;
			// 
			// Zones_TempDeviceId
			// 
			this.Zones_TempDeviceId.HeaderText = "TempId";
			this.Zones_TempDeviceId.Name = "Zones_TempDeviceId";
			this.Zones_TempDeviceId.Width = 80;
			// 
			// Zones_PowerType
			// 
			this.Zones_PowerType.HeaderText = "PowerType";
			this.Zones_PowerType.Items.AddRange(new object[] {
            "Denkovi",
            "Numato",
            "None"});
			this.Zones_PowerType.Name = "Zones_PowerType";
			this.Zones_PowerType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Zones_PowerType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.Zones_PowerType.Width = 104;
			// 
			// FormOptions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1370, 485);
			this.Controls.Add(this.tabParams);
			this.Controls.Add(this.btnSave);
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Name = "FormOptions";
			this.Text = "FormOptions";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormOptions_FormClosing);
			this.Load += new System.EventHandler(this.FormOptions_Load);
			this.tabParams.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvZones)).EndInit();
			this.tabControl.ResumeLayout(false);
			this.tabControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvControl)).EndInit();
			this.tabMusic.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvMusic)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvMusicMood)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvMusicSchedule)).EndInit();
			this.tabVideo.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvDisplay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvVideo)).EndInit();
			this.tabInput.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvInputs)).EndInit();
			this.tabUsers.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
			this.tabPage2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvParams)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TabControl tabParams;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dgvZones;
        private System.Windows.Forms.TabPage tabControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLastControlDevice;
        private System.Windows.Forms.DataGridView dgvControl;
        private System.Windows.Forms.TabPage tabMusic;
        private System.Windows.Forms.DataGridView dgvMusic;
        private System.Windows.Forms.DataGridViewComboBoxColumn Playlist_UserId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Playlist_FullPath;
        private System.Windows.Forms.DataGridViewComboBoxColumn Playlist_Mood;
        private System.Windows.Forms.DataGridViewButtonColumn Playlist_OpenPlaylist;
        private System.Windows.Forms.DataGridViewButtonColumn Playlist_OpenFolder;
        private System.Windows.Forms.TabPage tabVideo;
        private System.Windows.Forms.DataGridView dgvVideo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_UserId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_PlaylistFullPath;
        private System.Windows.Forms.TabPage tabInput;
        private System.Windows.Forms.DataGridView dgvInputs;
        private System.Windows.Forms.DataGridViewTextBoxColumn InputId;
        private System.Windows.Forms.DataGridViewTextBoxColumn InputName;
        private System.Windows.Forms.DataGridViewComboBoxColumn InputTypeId_Inputs;
        private System.Windows.Forms.DataGridViewTextBoxColumn InZoneId;
        private System.Windows.Forms.DataGridViewComboBoxColumn InputDeviceName;
        private System.Windows.Forms.TabPage tabUsers;
        private System.Windows.Forms.DataGridView dgvUsers;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserId;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserCode;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button1;
		private System.Windows.Forms.DataGridView dgvParams;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgvMusicMood;
        private System.Windows.Forms.DataGridView dgvMusicSchedule;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_ScheduleZoneId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_ScheduleStartTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_ScheduleEndTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_ScheduleWeekDay;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_ScheduleMood;
		private System.Windows.Forms.DataGridView dgvDisplay;
        private System.Windows.Forms.DataGridViewTextBoxColumn Display_ZoneID;
        private System.Windows.Forms.DataGridViewComboBoxColumn Display_Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Display_ConnectionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_MoodName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_MoodGenres;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_MoodAuthors;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_MoodRatings;
        private System.Windows.Forms.DataGridViewTextBoxColumn Music_AgeInWeeks;
        private System.Windows.Forms.DataGridViewComboBoxColumn MusicMood_LogicalOperation;
        private System.Windows.Forms.DataGridViewCheckBoxColumn MusicMood_GroupByTop;
        private System.Windows.Forms.DataGridViewTextBoxColumn MusicMood_GroupByTopCount;
        private System.Windows.Forms.DataGridViewCheckBoxColumn MusicMood_Random;
		private System.Windows.Forms.DataGridViewTextBoxColumn MusicMood_NumericCode;
		private System.Windows.Forms.DataGridViewTextBoxColumn Control_ZoneId;
		private System.Windows.Forms.DataGridViewComboBoxColumn Control_DeviceName;
		private System.Windows.Forms.DataGridViewTextBoxColumn Control_Description;
		private System.Windows.Forms.DataGridViewComboBoxColumn Control_Identify;
		private System.Windows.Forms.DataGridViewTextBoxColumn ParamName;
		private System.Windows.Forms.DataGridViewTextBoxColumn ParamValue;
		private System.Windows.Forms.DataGridViewTextBoxColumn Param_Description;
		private System.Windows.Forms.DataGridViewTextBoxColumn ZoneId;
		private System.Windows.Forms.DataGridViewTextBoxColumn ZoneName;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_OutputKeywords;
		private System.Windows.Forms.DataGridViewComboBoxColumn OutputDevice;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_PowerIndexDK;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_DefaultVolume;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_CameraId;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_AlarmZoneId;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_AlarmAreadId;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_ParentZoneId;
		private System.Windows.Forms.DataGridViewComboBoxColumn Zones_ClosureRelayType;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_ClosureIdList;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_PowerOnDelay;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_NearbyZoneIdList;
		private System.Windows.Forms.DataGridViewTextBoxColumn Zones_TempDeviceId;
		private System.Windows.Forms.DataGridViewComboBoxColumn Zones_PowerIndexType;
		private System.Windows.Forms.DataGridViewComboBoxColumn Zones_PowerType;
    }
}