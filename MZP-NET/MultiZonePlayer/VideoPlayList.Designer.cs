namespace MultiZonePlayer
{
    partial class VideoPlayList
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.webMain = new System.Windows.Forms.WebBrowser();
            this.dgvVideos = new System.Windows.Forms.DataGridView();
            this.Video_Changed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Video_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_ImdbId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Rating = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Year = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Genre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Actors = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Director = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Seen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Resolution = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Video_ImageURL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btPlay = new System.Windows.Forms.Button();
            this.btImdbAll = new System.Windows.Forms.Button();
            this.btGetImdb = new System.Windows.Forms.Button();
            this.btGetChecked = new System.Windows.Forms.Button();
            this.btExport = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.btSave = new System.Windows.Forms.Button();
            this.btMissingImdb = new System.Windows.Forms.Button();
            this.btSetImdbId = new System.Windows.Forms.Button();
            this.btFind = new System.Windows.Forms.Button();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.txTitle = new System.Windows.Forms.TextBox();
            this.imgVideo = new System.Windows.Forms.PictureBox();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.txTestURL = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.txTest = new System.Windows.Forms.RichTextBox();
            this.btEpisode = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVideos)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgVideo)).BeginInit();
            this.flowLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // webMain
            // 
            this.webMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webMain.Location = new System.Drawing.Point(2, 264);
            this.webMain.Margin = new System.Windows.Forms.Padding(2);
            this.webMain.MinimumSize = new System.Drawing.Size(15, 16);
            this.webMain.Name = "webMain";
            this.webMain.ScriptErrorsSuppressed = true;
            this.webMain.Size = new System.Drawing.Size(790, 288);
            this.webMain.TabIndex = 1;
            this.webMain.Url = new System.Uri("", System.UriKind.Relative);
            this.webMain.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webMain_DocumentCompleted);
            // 
            // dgvVideos
            // 
            this.dgvVideos.AllowUserToAddRows = false;
            this.dgvVideos.AllowUserToDeleteRows = false;
            this.dgvVideos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvVideos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvVideos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVideos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Video_Changed,
            this.Video_Name,
            this.Video_ImdbId,
            this.Video_Rating,
            this.Video_Year,
            this.Video_Genre,
            this.Video_Actors,
            this.Video_Director,
            this.Video_Seen,
            this.Video_Resolution,
            this.Video_Description,
            this.Video_Path,
            this.Video_ImageURL});
            this.dgvVideos.Location = new System.Drawing.Point(2, 2);
            this.dgvVideos.Margin = new System.Windows.Forms.Padding(2);
            this.dgvVideos.MultiSelect = false;
            this.dgvVideos.Name = "dgvVideos";
            this.dgvVideos.RowTemplate.Height = 24;
            this.dgvVideos.Size = new System.Drawing.Size(790, 171);
            this.dgvVideos.TabIndex = 2;
            this.dgvVideos.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVideos_CellContentClick);
            this.dgvVideos.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvVideos_CellMouseDoubleClick);
            this.dgvVideos.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVideos_CellValueChanged);
            this.dgvVideos.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvVideos_DataError);
            this.dgvVideos.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVideos_RowEnter);
            this.dgvVideos.Click += new System.EventHandler(this.dgvVideos_Click);
            // 
            // Video_Changed
            // 
            this.Video_Changed.HeaderText = "Changed";
            this.Video_Changed.Name = "Video_Changed";
            this.Video_Changed.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Video_Changed.Width = 75;
            // 
            // Video_Name
            // 
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Video_Name.DefaultCellStyle = dataGridViewCellStyle3;
            this.Video_Name.HeaderText = "Name";
            this.Video_Name.Name = "Video_Name";
            this.Video_Name.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Video_Name.Width = 60;
            // 
            // Video_ImdbId
            // 
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Video_ImdbId.DefaultCellStyle = dataGridViewCellStyle4;
            this.Video_ImdbId.HeaderText = "ImdbId";
            this.Video_ImdbId.Name = "Video_ImdbId";
            this.Video_ImdbId.Width = 64;
            // 
            // Video_Rating
            // 
            this.Video_Rating.HeaderText = "Rating";
            this.Video_Rating.Name = "Video_Rating";
            this.Video_Rating.Width = 63;
            // 
            // Video_Year
            // 
            this.Video_Year.HeaderText = "Year";
            this.Video_Year.Name = "Video_Year";
            this.Video_Year.Width = 54;
            // 
            // Video_Genre
            // 
            this.Video_Genre.HeaderText = "Genre";
            this.Video_Genre.Name = "Video_Genre";
            this.Video_Genre.Width = 61;
            // 
            // Video_Actors
            // 
            this.Video_Actors.HeaderText = "Actors";
            this.Video_Actors.Name = "Video_Actors";
            this.Video_Actors.Width = 62;
            // 
            // Video_Director
            // 
            this.Video_Director.HeaderText = "Director";
            this.Video_Director.Name = "Video_Director";
            this.Video_Director.Width = 69;
            // 
            // Video_Seen
            // 
            this.Video_Seen.HeaderText = "Seen";
            this.Video_Seen.Name = "Video_Seen";
            this.Video_Seen.Width = 57;
            // 
            // Video_Resolution
            // 
            this.Video_Resolution.HeaderText = "Resolution";
            this.Video_Resolution.Name = "Video_Resolution";
            this.Video_Resolution.Width = 82;
            // 
            // Video_Description
            // 
            this.Video_Description.HeaderText = "Description";
            this.Video_Description.Name = "Video_Description";
            this.Video_Description.Width = 85;
            // 
            // Video_Path
            // 
            this.Video_Path.HeaderText = "Path";
            this.Video_Path.Name = "Video_Path";
            this.Video_Path.ReadOnly = true;
            this.Video_Path.Visible = false;
            this.Video_Path.Width = 62;
            // 
            // Video_ImageURL
            // 
            this.Video_ImageURL.HeaderText = "ImageURL";
            this.Video_ImageURL.Name = "Video_ImageURL";
            this.Video_ImageURL.ReadOnly = true;
            this.Video_ImageURL.Visible = false;
            this.Video_ImageURL.Width = 99;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.dgvVideos, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtURL, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.webMain, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtStatus, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.imgVideo, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txTitle, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel4, 1, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 2);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1059, 584);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // txtURL
            // 
            this.txtURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtURL.Location = new System.Drawing.Point(2, 556);
            this.txtURL.Margin = new System.Windows.Forms.Padding(2);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(790, 20);
            this.txtURL.TabIndex = 4;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.flowLayoutPanel1);
            this.flowLayoutPanel3.Controls.Add(this.flowLayoutPanel2);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(2, 177);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(639, 83);
            this.flowLayoutPanel3.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btPlay);
            this.flowLayoutPanel1.Controls.Add(this.btImdbAll);
            this.flowLayoutPanel1.Controls.Add(this.btGetImdb);
            this.flowLayoutPanel1.Controls.Add(this.btGetChecked);
            this.flowLayoutPanel1.Controls.Add(this.btExport);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(304, 75);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // btPlay
            // 
            this.btPlay.AutoSize = true;
            this.btPlay.Location = new System.Drawing.Point(2, 2);
            this.btPlay.Margin = new System.Windows.Forms.Padding(2);
            this.btPlay.Name = "btPlay";
            this.btPlay.Size = new System.Drawing.Size(37, 23);
            this.btPlay.TabIndex = 1;
            this.btPlay.Text = "Play";
            this.btPlay.UseVisualStyleBackColor = true;
            // 
            // btImdbAll
            // 
            this.btImdbAll.AutoSize = true;
            this.btImdbAll.Location = new System.Drawing.Point(43, 2);
            this.btImdbAll.Margin = new System.Windows.Forms.Padding(2);
            this.btImdbAll.Name = "btImdbAll";
            this.btImdbAll.Size = new System.Drawing.Size(100, 23);
            this.btImdbAll.TabIndex = 2;
            this.btImdbAll.Text = "Refresh All IMDB";
            this.btImdbAll.UseVisualStyleBackColor = true;
            this.btImdbAll.Click += new System.EventHandler(this.btImdbAll_Click);
            // 
            // btGetImdb
            // 
            this.btGetImdb.AutoSize = true;
            this.btGetImdb.Location = new System.Drawing.Point(147, 2);
            this.btGetImdb.Margin = new System.Windows.Forms.Padding(2);
            this.btGetImdb.Name = "btGetImdb";
            this.btGetImdb.Size = new System.Drawing.Size(109, 23);
            this.btGetImdb.TabIndex = 2;
            this.btGetImdb.Text = "Get Selected IMDB";
            this.btGetImdb.UseVisualStyleBackColor = true;
            this.btGetImdb.Click += new System.EventHandler(this.btGetImdb_Click);
            // 
            // btGetChecked
            // 
            this.btGetChecked.AutoSize = true;
            this.btGetChecked.Location = new System.Drawing.Point(2, 29);
            this.btGetChecked.Margin = new System.Windows.Forms.Padding(2);
            this.btGetChecked.Name = "btGetChecked";
            this.btGetChecked.Size = new System.Drawing.Size(110, 23);
            this.btGetChecked.TabIndex = 2;
            this.btGetChecked.Text = "Get Checked IMDB";
            this.btGetChecked.UseVisualStyleBackColor = true;
            this.btGetChecked.Click += new System.EventHandler(this.button1_Click);
            // 
            // btExport
            // 
            this.btExport.Location = new System.Drawing.Point(116, 29);
            this.btExport.Margin = new System.Windows.Forms.Padding(2);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(56, 19);
            this.btExport.TabIndex = 3;
            this.btExport.Text = "Export";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.btSave);
            this.flowLayoutPanel2.Controls.Add(this.btMissingImdb);
            this.flowLayoutPanel2.Controls.Add(this.btSetImdbId);
            this.flowLayoutPanel2.Controls.Add(this.btFind);
            this.flowLayoutPanel2.Controls.Add(this.btEpisode);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(310, 2);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(322, 75);
            this.flowLayoutPanel2.TabIndex = 6;
            // 
            // btSave
            // 
            this.btSave.AutoSize = true;
            this.btSave.Location = new System.Drawing.Point(2, 2);
            this.btSave.Margin = new System.Windows.Forms.Padding(2);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(42, 23);
            this.btSave.TabIndex = 0;
            this.btSave.Text = "Save";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btMissingImdb
            // 
            this.btMissingImdb.AutoSize = true;
            this.btMissingImdb.Location = new System.Drawing.Point(48, 2);
            this.btMissingImdb.Margin = new System.Windows.Forms.Padding(2);
            this.btMissingImdb.Name = "btMissingImdb";
            this.btMissingImdb.Size = new System.Drawing.Size(101, 23);
            this.btMissingImdb.TabIndex = 2;
            this.btMissingImdb.Text = "Get missing IMDB";
            this.btMissingImdb.UseVisualStyleBackColor = true;
            this.btMissingImdb.Click += new System.EventHandler(this.btMissingImdb_Click);
            // 
            // btSetImdbId
            // 
            this.btSetImdbId.AutoSize = true;
            this.btSetImdbId.Location = new System.Drawing.Point(153, 2);
            this.btSetImdbId.Margin = new System.Windows.Forms.Padding(2);
            this.btSetImdbId.Name = "btSetImdbId";
            this.btSetImdbId.Size = new System.Drawing.Size(71, 23);
            this.btSetImdbId.TabIndex = 4;
            this.btSetImdbId.Text = "Set Imdb Id";
            this.btSetImdbId.UseVisualStyleBackColor = true;
            this.btSetImdbId.Click += new System.EventHandler(this.btSetImdbId_Click);
            // 
            // btFind
            // 
            this.btFind.AutoSize = true;
            this.btFind.Location = new System.Drawing.Point(228, 2);
            this.btFind.Margin = new System.Windows.Forms.Padding(2);
            this.btFind.Name = "btFind";
            this.btFind.Size = new System.Drawing.Size(81, 23);
            this.btFind.TabIndex = 4;
            this.btFind.Text = "Search Video";
            this.btFind.UseVisualStyleBackColor = true;
            this.btFind.Click += new System.EventHandler(this.btFind_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(796, 177);
            this.txtStatus.Margin = new System.Windows.Forms.Padding(2);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(98, 20);
            this.txtStatus.TabIndex = 3;
            // 
            // txTitle
            // 
            this.txTitle.Location = new System.Drawing.Point(796, 556);
            this.txTitle.Margin = new System.Windows.Forms.Padding(2);
            this.txTitle.Name = "txTitle";
            this.txTitle.Size = new System.Drawing.Size(98, 20);
            this.txTitle.TabIndex = 9;
            // 
            // imgVideo
            // 
            this.imgVideo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imgVideo.Location = new System.Drawing.Point(796, 2);
            this.imgVideo.Margin = new System.Windows.Forms.Padding(2);
            this.imgVideo.Name = "imgVideo";
            this.imgVideo.Size = new System.Drawing.Size(261, 110);
            this.imgVideo.TabIndex = 8;
            this.imgVideo.TabStop = false;
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.button2);
            this.flowLayoutPanel4.Controls.Add(this.label1);
            this.flowLayoutPanel4.Controls.Add(this.txTestURL);
            this.flowLayoutPanel4.Controls.Add(this.txTest);
            this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel4.Location = new System.Drawing.Point(797, 265);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(259, 286);
            this.flowLayoutPanel4.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "TestCond Movie";
            // 
            // txTestURL
            // 
            this.txTestURL.Location = new System.Drawing.Point(69, 32);
            this.txTestURL.Name = "txTestURL";
            this.txTestURL.Size = new System.Drawing.Size(154, 20);
            this.txTestURL.TabIndex = 1;
            this.txTestURL.Text = "0133093";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(3, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(230, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Run TestCond";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // txTest
            // 
            this.txTest.Location = new System.Drawing.Point(3, 58);
            this.txTest.Name = "txTest";
            this.txTest.Size = new System.Drawing.Size(247, 212);
            this.txTest.TabIndex = 6;
            this.txTest.Text = "";
            // 
            // btEpisode
            // 
            this.btEpisode.Location = new System.Drawing.Point(3, 30);
            this.btEpisode.Name = "btEpisode";
            this.btEpisode.Size = new System.Drawing.Size(98, 23);
            this.btEpisode.TabIndex = 5;
            this.btEpisode.Text = "Set as Episode";
            this.btEpisode.UseVisualStyleBackColor = true;
            this.btEpisode.Click += new System.EventHandler(this.btEpisode_Click);
            // 
            // VideoPlayList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 597);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "VideoPlayList";
            this.Text = "VideoPlayList";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.VideoPlayList_FormClosed);
            this.Load += new System.EventHandler(this.VideoPlayList_Load);
            this.SizeChanged += new System.EventHandler(this.VideoPlayList_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVideos)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgVideo)).EndInit();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webMain;
        private System.Windows.Forms.DataGridView dgvVideos;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btFind;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Button btSetImdbId;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btPlay;
        private System.Windows.Forms.Button btGetImdb;
        private System.Windows.Forms.Button btImdbAll;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.PictureBox imgVideo;
        private System.Windows.Forms.TextBox txTitle;
        private System.Windows.Forms.Button btGetChecked;
        private System.Windows.Forms.Button btMissingImdb;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Video_Changed;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_ImdbId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Rating;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Year;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Genre;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Actors;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Director;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Seen;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Resolution;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Description;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_Path;
        private System.Windows.Forms.DataGridViewTextBoxColumn Video_ImageURL;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txTestURL;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RichTextBox txTest;
        private System.Windows.Forms.Button btEpisode;
    }
}