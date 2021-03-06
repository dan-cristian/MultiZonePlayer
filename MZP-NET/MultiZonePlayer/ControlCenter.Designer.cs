﻿using System;

namespace MultiZonePlayer
{
    partial class ControlCenter
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
            try
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (Exception)
            {

            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.txtError = new System.Windows.Forms.RichTextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cmbCurrentZone = new System.Windows.Forms.ComboBox();
			this.txtCommand = new System.Windows.Forms.TextBox();
			this.cmbCurrentCode = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cmbCurrentCmd = new System.Windows.Forms.ComboBox();
			this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
			this.label3 = new System.Windows.Forms.Label();
			this.txtInactivityCycles = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.txtZoneCount = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtPwrControl = new System.Windows.Forms.TextBox();
			this.txtPowerSaving = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.txtLog2 = new System.Windows.Forms.RichTextBox();
			this.txtLog3 = new System.Windows.Forms.RichTextBox();
			this.bt1 = new System.Windows.Forms.Button();
			this.bt2 = new System.Windows.Forms.Button();
			this.tbButtons = new System.Windows.Forms.TableLayoutPanel();
			this.bt3 = new System.Windows.Forms.Button();
			this.bt4 = new System.Windows.Forms.Button();
			this.bt5 = new System.Windows.Forms.Button();
			this.bt6 = new System.Windows.Forms.Button();
			this.bt7 = new System.Windows.Forms.Button();
			this.bt8 = new System.Windows.Forms.Button();
			this.bt9 = new System.Windows.Forms.Button();
			this.bt0 = new System.Windows.Forms.Button();
			this.btMusic = new System.Windows.Forms.Button();
			this.btRadio = new System.Windows.Forms.Button();
			this.btVideo = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.btTV = new System.Windows.Forms.Button();
			this.chkPowerControl = new System.Windows.Forms.CheckBox();
			this.label9 = new System.Windows.Forms.Label();
			this.txtThreadCount = new System.Windows.Forms.TextBox();
			this.txSocketsStatus = new System.Windows.Forms.TextBox();
			this.lstCommands = new System.Windows.Forms.ListBox();
			this.txAudioCount = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.txVideoCount = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.txAudioSave = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.txAlarm = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.txPictureCount = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.txtGPIO = new System.Windows.Forms.TextBox();
			this.button2 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
			this.tbButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtError
			// 
			this.txtError.HideSelection = false;
			this.txtError.Location = new System.Drawing.Point(12, 190);
			this.txtError.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtError.Name = "txtError";
			this.txtError.Size = new System.Drawing.Size(601, 147);
			this.txtError.TabIndex = 0;
			this.txtError.Text = "";
			this.txtError.TextChanged += new System.EventHandler(this.txtError_TextChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(71, 17);
			this.label4.TabIndex = 16;
			this.label4.Text = "Command";
			// 
			// cmbCurrentZone
			// 
			this.cmbCurrentZone.FormattingEnabled = true;
			this.cmbCurrentZone.Location = new System.Drawing.Point(140, 94);
			this.cmbCurrentZone.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.cmbCurrentZone.Name = "cmbCurrentZone";
			this.cmbCurrentZone.Size = new System.Drawing.Size(112, 24);
			this.cmbCurrentZone.TabIndex = 21;
			// 
			// txtCommand
			// 
			this.txtCommand.AcceptsReturn = true;
			this.txtCommand.AcceptsTab = true;
			this.txtCommand.BackColor = System.Drawing.Color.MistyRose;
			this.txtCommand.Location = new System.Drawing.Point(140, 9);
			this.txtCommand.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtCommand.Name = "txtCommand";
			this.txtCommand.Size = new System.Drawing.Size(112, 22);
			this.txtCommand.TabIndex = 0;
			this.txtCommand.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// cmbCurrentCode
			// 
			this.cmbCurrentCode.FormattingEnabled = true;
			this.cmbCurrentCode.Location = new System.Drawing.Point(140, 37);
			this.cmbCurrentCode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.cmbCurrentCode.Name = "cmbCurrentCode";
			this.cmbCurrentCode.Size = new System.Drawing.Size(112, 24);
			this.cmbCurrentCode.TabIndex = 22;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 37);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(92, 17);
			this.label8.TabIndex = 17;
			this.label8.Text = "Current Code";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 97);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(92, 17);
			this.label5.TabIndex = 18;
			this.label5.Text = "Current Zone";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 70);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(122, 17);
			this.label2.TabIndex = 17;
			this.label2.Text = "Current Command";
			// 
			// cmbCurrentCmd
			// 
			this.cmbCurrentCmd.FormattingEnabled = true;
			this.cmbCurrentCmd.Location = new System.Drawing.Point(140, 66);
			this.cmbCurrentCmd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.cmbCurrentCmd.Name = "cmbCurrentCmd";
			this.cmbCurrentCmd.Size = new System.Drawing.Size(112, 24);
			this.cmbCurrentCmd.TabIndex = 22;
			// 
			// fileSystemWatcher1
			// 
			this.fileSystemWatcher1.EnableRaisingEvents = true;
			this.fileSystemWatcher1.SynchronizingObject = this;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(280, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(107, 17);
			this.label3.TabIndex = 16;
			this.label3.Text = "Inactivity Cycles";
			// 
			// txtInactivityCycles
			// 
			this.txtInactivityCycles.AcceptsReturn = true;
			this.txtInactivityCycles.AcceptsTab = true;
			this.txtInactivityCycles.Location = new System.Drawing.Point(393, 9);
			this.txtInactivityCycles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtInactivityCycles.Name = "txtInactivityCycles";
			this.txtInactivityCycles.Size = new System.Drawing.Size(76, 22);
			this.txtInactivityCycles.TabIndex = 0;
			this.txtInactivityCycles.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(277, 118);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(103, 17);
			this.label7.TabIndex = 16;
			this.label7.Text = "Zones Opened";
			// 
			// txtZoneCount
			// 
			this.txtZoneCount.AcceptsReturn = true;
			this.txtZoneCount.AcceptsTab = true;
			this.txtZoneCount.Location = new System.Drawing.Point(393, 115);
			this.txtZoneCount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtZoneCount.Name = "txtZoneCount";
			this.txtZoneCount.Size = new System.Drawing.Size(76, 22);
			this.txtZoneCount.TabIndex = 0;
			this.txtZoneCount.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(277, 34);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 17);
			this.label1.TabIndex = 16;
			this.label1.Text = "Power Control";
			// 
			// txtPwrControl
			// 
			this.txtPwrControl.AcceptsReturn = true;
			this.txtPwrControl.AcceptsTab = true;
			this.txtPwrControl.Location = new System.Drawing.Point(393, 34);
			this.txtPwrControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtPwrControl.Name = "txtPwrControl";
			this.txtPwrControl.Size = new System.Drawing.Size(76, 22);
			this.txtPwrControl.TabIndex = 0;
			this.txtPwrControl.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// txtPowerSaving
			// 
			this.txtPowerSaving.AcceptsReturn = true;
			this.txtPowerSaving.AcceptsTab = true;
			this.txtPowerSaving.Location = new System.Drawing.Point(393, 89);
			this.txtPowerSaving.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtPowerSaving.Name = "txtPowerSaving";
			this.txtPowerSaving.Size = new System.Drawing.Size(76, 22);
			this.txtPowerSaving.TabIndex = 0;
			this.txtPowerSaving.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(277, 94);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(94, 17);
			this.label6.TabIndex = 16;
			this.label6.Text = "Power Saving";
			// 
			// txtLog2
			// 
			this.txtLog2.HideSelection = false;
			this.txtLog2.Location = new System.Drawing.Point(12, 342);
			this.txtLog2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtLog2.Name = "txtLog2";
			this.txtLog2.Size = new System.Drawing.Size(601, 57);
			this.txtLog2.TabIndex = 0;
			this.txtLog2.Text = "";
			this.txtLog2.Click += new System.EventHandler(this.txtLog2_Click);
			this.txtLog2.DoubleClick += new System.EventHandler(this.txtLog2_DoubleClick);
			// 
			// txtLog3
			// 
			this.txtLog3.HideSelection = false;
			this.txtLog3.Location = new System.Drawing.Point(816, 7);
			this.txtLog3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtLog3.Name = "txtLog3";
			this.txtLog3.Size = new System.Drawing.Size(96, 187);
			this.txtLog3.TabIndex = 0;
			this.txtLog3.Text = "";
			this.txtLog3.Click += new System.EventHandler(this.txtLog2_Click);
			this.txtLog3.DoubleClick += new System.EventHandler(this.txtLog2_DoubleClick);
			// 
			// bt1
			// 
			this.bt1.Location = new System.Drawing.Point(3, 2);
			this.bt1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt1.Name = "bt1";
			this.bt1.Size = new System.Drawing.Size(15, 23);
			this.bt1.TabIndex = 23;
			this.bt1.Text = "1";
			this.bt1.UseVisualStyleBackColor = true;
			this.bt1.Click += new System.EventHandler(this.bt1_Click);
			// 
			// bt2
			// 
			this.bt2.Location = new System.Drawing.Point(24, 2);
			this.bt2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt2.Name = "bt2";
			this.bt2.Size = new System.Drawing.Size(17, 23);
			this.bt2.TabIndex = 23;
			this.bt2.Text = "2";
			this.bt2.UseVisualStyleBackColor = true;
			this.bt2.Click += new System.EventHandler(this.bt1_Click);
			// 
			// tbButtons
			// 
			this.tbButtons.ColumnCount = 5;
			this.tbButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.07692F));
			this.tbButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.92308F));
			this.tbButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 21F));
			this.tbButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 21F));
			this.tbButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tbButtons.Controls.Add(this.bt1, 0, 0);
			this.tbButtons.Controls.Add(this.bt2, 1, 0);
			this.tbButtons.Controls.Add(this.bt3, 2, 0);
			this.tbButtons.Controls.Add(this.bt4, 0, 1);
			this.tbButtons.Controls.Add(this.bt5, 1, 1);
			this.tbButtons.Controls.Add(this.bt6, 2, 1);
			this.tbButtons.Controls.Add(this.bt7, 0, 2);
			this.tbButtons.Controls.Add(this.bt8, 1, 2);
			this.tbButtons.Controls.Add(this.bt9, 2, 2);
			this.tbButtons.Controls.Add(this.bt0, 0, 3);
			this.tbButtons.Controls.Add(this.btMusic, 0, 4);
			this.tbButtons.Controls.Add(this.btRadio, 1, 4);
			this.tbButtons.Controls.Add(this.btVideo, 2, 4);
			this.tbButtons.Controls.Add(this.button1, 3, 4);
			this.tbButtons.Controls.Add(this.btTV, 0, 5);
			this.tbButtons.Location = new System.Drawing.Point(695, 11);
			this.tbButtons.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tbButtons.Name = "tbButtons";
			this.tbButtons.RowCount = 6;
			this.tbButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tbButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tbButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tbButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tbButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tbButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tbButtons.Size = new System.Drawing.Size(116, 181);
			this.tbButtons.TabIndex = 24;
			// 
			// bt3
			// 
			this.bt3.Location = new System.Drawing.Point(47, 2);
			this.bt3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt3.Name = "bt3";
			this.bt3.Size = new System.Drawing.Size(15, 23);
			this.bt3.TabIndex = 23;
			this.bt3.Text = "3";
			this.bt3.UseVisualStyleBackColor = true;
			this.bt3.Click += new System.EventHandler(this.bt1_Click);
			// 
			// bt4
			// 
			this.bt4.Location = new System.Drawing.Point(3, 31);
			this.bt4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt4.Name = "bt4";
			this.bt4.Size = new System.Drawing.Size(15, 23);
			this.bt4.TabIndex = 23;
			this.bt4.Text = "4";
			this.bt4.UseVisualStyleBackColor = true;
			this.bt4.Click += new System.EventHandler(this.bt1_Click);
			// 
			// bt5
			// 
			this.bt5.Location = new System.Drawing.Point(24, 31);
			this.bt5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt5.Name = "bt5";
			this.bt5.Size = new System.Drawing.Size(17, 23);
			this.bt5.TabIndex = 23;
			this.bt5.Text = "5";
			this.bt5.UseVisualStyleBackColor = true;
			this.bt5.Click += new System.EventHandler(this.bt1_Click);
			// 
			// bt6
			// 
			this.bt6.Location = new System.Drawing.Point(47, 31);
			this.bt6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt6.Name = "bt6";
			this.bt6.Size = new System.Drawing.Size(15, 23);
			this.bt6.TabIndex = 23;
			this.bt6.Text = "6";
			this.bt6.UseVisualStyleBackColor = true;
			this.bt6.Click += new System.EventHandler(this.bt1_Click);
			// 
			// bt7
			// 
			this.bt7.Location = new System.Drawing.Point(3, 60);
			this.bt7.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt7.Name = "bt7";
			this.bt7.Size = new System.Drawing.Size(15, 23);
			this.bt7.TabIndex = 23;
			this.bt7.Text = "7";
			this.bt7.UseVisualStyleBackColor = true;
			this.bt7.Click += new System.EventHandler(this.bt1_Click);
			// 
			// bt8
			// 
			this.bt8.Location = new System.Drawing.Point(24, 60);
			this.bt8.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt8.Name = "bt8";
			this.bt8.Size = new System.Drawing.Size(17, 23);
			this.bt8.TabIndex = 23;
			this.bt8.Text = "8";
			this.bt8.UseVisualStyleBackColor = true;
			this.bt8.Click += new System.EventHandler(this.bt1_Click);
			// 
			// bt9
			// 
			this.bt9.Location = new System.Drawing.Point(47, 60);
			this.bt9.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt9.Name = "bt9";
			this.bt9.Size = new System.Drawing.Size(15, 23);
			this.bt9.TabIndex = 23;
			this.bt9.Text = "9";
			this.bt9.UseVisualStyleBackColor = true;
			this.bt9.Click += new System.EventHandler(this.bt1_Click);
			// 
			// bt0
			// 
			this.bt0.Location = new System.Drawing.Point(3, 91);
			this.bt0.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.bt0.Name = "bt0";
			this.bt0.Size = new System.Drawing.Size(15, 23);
			this.bt0.TabIndex = 23;
			this.bt0.Text = "0";
			this.bt0.UseVisualStyleBackColor = true;
			this.bt0.Click += new System.EventHandler(this.bt1_Click);
			// 
			// btMusic
			// 
			this.btMusic.Location = new System.Drawing.Point(3, 121);
			this.btMusic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.btMusic.Name = "btMusic";
			this.btMusic.Size = new System.Drawing.Size(15, 23);
			this.btMusic.TabIndex = 24;
			this.btMusic.Tag = "music";
			this.btMusic.Text = "M";
			this.btMusic.UseVisualStyleBackColor = true;
			this.btMusic.Click += new System.EventHandler(this.btCommand_Click);
			// 
			// btRadio
			// 
			this.btRadio.Location = new System.Drawing.Point(24, 121);
			this.btRadio.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.btRadio.Name = "btRadio";
			this.btRadio.Size = new System.Drawing.Size(17, 23);
			this.btRadio.TabIndex = 24;
			this.btRadio.Tag = "radio";
			this.btRadio.Text = "R";
			this.btRadio.UseVisualStyleBackColor = true;
			this.btRadio.Click += new System.EventHandler(this.btCommand_Click);
			// 
			// btVideo
			// 
			this.btVideo.Location = new System.Drawing.Point(47, 121);
			this.btVideo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.btVideo.Name = "btVideo";
			this.btVideo.Size = new System.Drawing.Size(15, 23);
			this.btVideo.TabIndex = 24;
			this.btVideo.Tag = "video";
			this.btVideo.Text = "V";
			this.btVideo.UseVisualStyleBackColor = true;
			this.btVideo.Click += new System.EventHandler(this.btCommand_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(68, 121);
			this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(15, 23);
			this.button1.TabIndex = 24;
			this.button1.Tag = "streammp3";
			this.button1.Text = "S";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.btCommand_Click);
			// 
			// btTV
			// 
			this.btTV.Location = new System.Drawing.Point(3, 151);
			this.btTV.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.btTV.Name = "btTV";
			this.btTV.Size = new System.Drawing.Size(15, 23);
			this.btTV.TabIndex = 24;
			this.btTV.Tag = "tv";
			this.btTV.Text = "T";
			this.btTV.UseVisualStyleBackColor = true;
			this.btTV.Click += new System.EventHandler(this.btCommand_Click);
			// 
			// chkPowerControl
			// 
			this.chkPowerControl.AutoSize = true;
			this.chkPowerControl.Location = new System.Drawing.Point(16, 122);
			this.chkPowerControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.chkPowerControl.Name = "chkPowerControl";
			this.chkPowerControl.Size = new System.Drawing.Size(114, 21);
			this.chkPowerControl.TabIndex = 25;
			this.chkPowerControl.Text = "PowerControl";
			this.chkPowerControl.UseVisualStyleBackColor = true;
			this.chkPowerControl.CheckedChanged += new System.EventHandler(this.chkPowerControl_CheckedChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(280, 66);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(59, 17);
			this.label9.TabIndex = 16;
			this.label9.Text = "ThrdCnt";
			// 
			// txtThreadCount
			// 
			this.txtThreadCount.AcceptsReturn = true;
			this.txtThreadCount.AcceptsTab = true;
			this.txtThreadCount.Location = new System.Drawing.Point(393, 62);
			this.txtThreadCount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtThreadCount.Name = "txtThreadCount";
			this.txtThreadCount.Size = new System.Drawing.Size(76, 22);
			this.txtThreadCount.TabIndex = 0;
			this.txtThreadCount.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// txSocketsStatus
			// 
			this.txSocketsStatus.Location = new System.Drawing.Point(140, 122);
			this.txSocketsStatus.Margin = new System.Windows.Forms.Padding(4);
			this.txSocketsStatus.Name = "txSocketsStatus";
			this.txSocketsStatus.Size = new System.Drawing.Size(112, 22);
			this.txSocketsStatus.TabIndex = 26;
			// 
			// lstCommands
			// 
			this.lstCommands.FormattingEnabled = true;
			this.lstCommands.ItemHeight = 16;
			this.lstCommands.Location = new System.Drawing.Point(620, 202);
			this.lstCommands.Margin = new System.Windows.Forms.Padding(4);
			this.lstCommands.Name = "lstCommands";
			this.lstCommands.Size = new System.Drawing.Size(292, 196);
			this.lstCommands.TabIndex = 27;
			// 
			// txAudioCount
			// 
			this.txAudioCount.AcceptsReturn = true;
			this.txAudioCount.AcceptsTab = true;
			this.txAudioCount.Location = new System.Drawing.Point(587, 11);
			this.txAudioCount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txAudioCount.Name = "txAudioCount";
			this.txAudioCount.Size = new System.Drawing.Size(100, 22);
			this.txAudioCount.TabIndex = 0;
			this.txAudioCount.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(483, 11);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(85, 17);
			this.label10.TabIndex = 16;
			this.label10.Text = "Audio Count";
			// 
			// txVideoCount
			// 
			this.txVideoCount.AcceptsReturn = true;
			this.txVideoCount.AcceptsTab = true;
			this.txVideoCount.Location = new System.Drawing.Point(587, 38);
			this.txVideoCount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txVideoCount.Name = "txVideoCount";
			this.txVideoCount.Size = new System.Drawing.Size(100, 22);
			this.txVideoCount.TabIndex = 0;
			this.txVideoCount.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(483, 38);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(85, 17);
			this.label11.TabIndex = 16;
			this.label11.Text = "Video Count";
			// 
			// txAudioSave
			// 
			this.txAudioSave.AcceptsReturn = true;
			this.txAudioSave.AcceptsTab = true;
			this.txAudioSave.Location = new System.Drawing.Point(587, 89);
			this.txAudioSave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txAudioSave.Name = "txAudioSave";
			this.txAudioSave.Size = new System.Drawing.Size(100, 22);
			this.txAudioSave.TabIndex = 0;
			this.txAudioSave.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(483, 89);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(96, 17);
			this.label12.TabIndex = 16;
			this.label12.Text = "Audio to Save";
			// 
			// txAlarm
			// 
			this.txAlarm.AcceptsReturn = true;
			this.txAlarm.AcceptsTab = true;
			this.txAlarm.Location = new System.Drawing.Point(587, 117);
			this.txAlarm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txAlarm.Name = "txAlarm";
			this.txAlarm.Size = new System.Drawing.Size(100, 22);
			this.txAlarm.TabIndex = 0;
			this.txAlarm.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(483, 117);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(81, 17);
			this.label13.TabIndex = 16;
			this.label13.Text = "Alarm State";
			// 
			// txPictureCount
			// 
			this.txPictureCount.AcceptsReturn = true;
			this.txPictureCount.AcceptsTab = true;
			this.txPictureCount.Location = new System.Drawing.Point(587, 61);
			this.txPictureCount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txPictureCount.Name = "txPictureCount";
			this.txPictureCount.Size = new System.Drawing.Size(100, 22);
			this.txPictureCount.TabIndex = 0;
			this.txPictureCount.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(483, 61);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(93, 17);
			this.label14.TabIndex = 16;
			this.label14.Text = "Picture Count";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(12, 150);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(42, 17);
			this.label15.TabIndex = 16;
			this.label15.Text = "GPIO";
			// 
			// txtGPIO
			// 
			this.txtGPIO.AcceptsReturn = true;
			this.txtGPIO.AcceptsTab = true;
			this.txtGPIO.Location = new System.Drawing.Point(140, 150);
			this.txtGPIO.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtGPIO.Name = "txtGPIO";
			this.txtGPIO.Size = new System.Drawing.Size(329, 22);
			this.txtGPIO.TabIndex = 0;
			this.txtGPIO.Leave += new System.EventHandler(this.txtCommand_Leave);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(501, 150);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 28;
			this.button2.Text = "button2";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// ControlCenter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(924, 414);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.lstCommands);
			this.Controls.Add(this.txSocketsStatus);
			this.Controls.Add(this.chkPowerControl);
			this.Controls.Add(this.tbButtons);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label15);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtPowerSaving);
			this.Controls.Add(this.cmbCurrentZone);
			this.Controls.Add(this.txtThreadCount);
			this.Controls.Add(this.txtPwrControl);
			this.Controls.Add(this.txPictureCount);
			this.Controls.Add(this.txVideoCount);
			this.Controls.Add(this.txAlarm);
			this.Controls.Add(this.txtGPIO);
			this.Controls.Add(this.txtZoneCount);
			this.Controls.Add(this.txAudioSave);
			this.Controls.Add(this.txAudioCount);
			this.Controls.Add(this.txtInactivityCycles);
			this.Controls.Add(this.txtCommand);
			this.Controls.Add(this.cmbCurrentCmd);
			this.Controls.Add(this.cmbCurrentCode);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtLog3);
			this.Controls.Add(this.txtLog2);
			this.Controls.Add(this.txtError);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MaximizeBox = false;
			this.Name = "ControlCenter";
			this.Text = "Control Center";
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ControlCenter_FormClosing);
			this.Load += new System.EventHandler(this.ControlCenter_Load);
			this.Shown += new System.EventHandler(this.ControlCenter_Shown);
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
			this.tbButtons.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtError;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbCurrentZone;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.ComboBox cmbCurrentCode;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbCurrentCmd;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtInactivityCycles;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtZoneCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPwrControl;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPowerSaving;
        private System.Windows.Forms.RichTextBox txtLog2;
        private System.Windows.Forms.RichTextBox txtLog3;
        private System.Windows.Forms.TableLayoutPanel tbButtons;
        private System.Windows.Forms.Button bt1;
        private System.Windows.Forms.Button bt2;
        private System.Windows.Forms.Button bt3;
        private System.Windows.Forms.Button bt4;
        private System.Windows.Forms.Button bt5;
        private System.Windows.Forms.Button bt6;
        private System.Windows.Forms.Button bt7;
        private System.Windows.Forms.Button bt8;
        private System.Windows.Forms.Button bt9;
        private System.Windows.Forms.Button bt0;
        private System.Windows.Forms.Button btMusic;
        private System.Windows.Forms.Button btRadio;
        private System.Windows.Forms.CheckBox chkPowerControl;
        private System.Windows.Forms.Button btVideo;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtThreadCount;
        private System.Windows.Forms.TextBox txSocketsStatus;
        private System.Windows.Forms.ListBox lstCommands;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txVideoCount;
        private System.Windows.Forms.TextBox txAudioCount;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txAudioSave;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txAlarm;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btTV;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox txPictureCount;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TextBox txtGPIO;
		private System.Windows.Forms.Button button2;
    }
}