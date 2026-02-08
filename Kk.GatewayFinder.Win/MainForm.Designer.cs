namespace Kk.GatewayFinder.Win
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblLastKnown = new System.Windows.Forms.Label();
            this.txtLastKnown = new System.Windows.Forms.TextBox();
            this.btnDetect = new System.Windows.Forms.Button();
            this.btnOpenBrowser = new System.Windows.Forms.Button();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.chkContinueAfterFirst = new System.Windows.Forms.CheckBox();
            this.lstGateways = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(24, 20);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(316, 21);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Détection de passerelles Milesight UG6x";
            // 
            // lblLastKnown
            // 
            this.lblLastKnown.AutoSize = true;
            this.lblLastKnown.Location = new System.Drawing.Point(26, 62);
            this.lblLastKnown.Name = "lblLastKnown";
            this.lblLastKnown.Size = new System.Drawing.Size(141, 15);
            this.lblLastKnown.TabIndex = 1;
            this.lblLastKnown.Text = "Dernier IP connu (option)";
            // 
            // txtLastKnown
            // 
            this.txtLastKnown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLastKnown.Location = new System.Drawing.Point(29, 81);
            this.txtLastKnown.Name = "txtLastKnown";
            this.txtLastKnown.Size = new System.Drawing.Size(326, 23);
            this.txtLastKnown.TabIndex = 2;
            // 
            // btnDetect
            // 
            this.btnDetect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDetect.Location = new System.Drawing.Point(371, 79);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(122, 27);
            this.btnDetect.TabIndex = 3;
            this.btnDetect.Text = "Détecter";
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.Click += new System.EventHandler(this.BtnDetect_Click);
            // 
            // btnOpenBrowser
            // 
            this.btnOpenBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenBrowser.Location = new System.Drawing.Point(509, 79);
            this.btnOpenBrowser.Name = "btnOpenBrowser";
            this.btnOpenBrowser.Size = new System.Drawing.Size(142, 27);
            this.btnOpenBrowser.TabIndex = 4;
            this.btnOpenBrowser.Text = "Ouvrir le navigateur";
            this.btnOpenBrowser.UseVisualStyleBackColor = true;
            this.btnOpenBrowser.Click += new System.EventHandler(this.BtnOpenBrowser_Click);
            // 
            // lstLog
            // 
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.HorizontalScrollbar = true;
            this.lstLog.ItemHeight = 15;
            this.lstLog.Location = new System.Drawing.Point(29, 168);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(622, 139);
            this.lstLog.TabIndex = 5;
            // 
            // progress
            // 
            this.progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progress.Location = new System.Drawing.Point(29, 125);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(622, 16);
            this.progress.TabIndex = 6;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Location = new System.Drawing.Point(29, 144);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(622, 21);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "Prêt";
            // 
            // chkContinueAfterFirst
            // 
            this.chkContinueAfterFirst.AutoSize = true;
            this.chkContinueAfterFirst.Location = new System.Drawing.Point(371, 56);
            this.chkContinueAfterFirst.Name = "chkContinueAfterFirst";
            this.chkContinueAfterFirst.Size = new System.Drawing.Size(215, 19);
            this.chkContinueAfterFirst.TabIndex = 8;
            this.chkContinueAfterFirst.Text = "Continuer après la première gateway";
            this.chkContinueAfterFirst.UseVisualStyleBackColor = true;
            // 
            // lstGateways
            // 
            this.lstGateways.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstGateways.FormattingEnabled = true;
            this.lstGateways.HorizontalScrollbar = true;
            this.lstGateways.ItemHeight = 15;
            this.lstGateways.Location = new System.Drawing.Point(29, 313);
            this.lstGateways.Name = "lstGateways";
            this.lstGateways.Size = new System.Drawing.Size(622, 94);
            this.lstGateways.TabIndex = 9;
            this.lstGateways.SelectedIndexChanged += new System.EventHandler(this.LstGateways_SelectedIndexChanged);
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 421);
            this.Controls.Add(this.lstGateways);
            this.Controls.Add(this.chkContinueAfterFirst);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.lstLog);
            this.Controls.Add(this.btnOpenBrowser);
            this.Controls.Add(this.btnDetect);
            this.Controls.Add(this.txtLastKnown);
            this.Controls.Add(this.lblLastKnown);
            this.Controls.Add(this.lblHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KropKontrol - Gateway Finder v0.2b";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblLastKnown;
        private System.Windows.Forms.TextBox txtLastKnown;
        private System.Windows.Forms.Button btnDetect;
        private System.Windows.Forms.Button btnOpenBrowser;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox chkContinueAfterFirst;
        private System.Windows.Forms.ListBox lstGateways;
    }
}
