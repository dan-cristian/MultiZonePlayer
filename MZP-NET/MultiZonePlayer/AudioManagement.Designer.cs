namespace MultiZonePlayer
{
    partial class AudioManagement
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
            this.btPlayCount = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btPlayCount
            // 
            this.btPlayCount.Location = new System.Drawing.Point(12, 32);
            this.btPlayCount.Name = "btPlayCount";
            this.btPlayCount.Size = new System.Drawing.Size(107, 23);
            this.btPlayCount.TabIndex = 0;
            this.btPlayCount.Text = "Clear Play Count";
            this.btPlayCount.UseVisualStyleBackColor = true;
            this.btPlayCount.Click += new System.EventHandler(this.btPlayCount_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 61);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(107, 23);
            this.progressBar.TabIndex = 1;
            // 
            // AudioManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 273);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btPlayCount);
            this.Name = "AudioManagement";
            this.Text = "AudioManagement";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btPlayCount;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}