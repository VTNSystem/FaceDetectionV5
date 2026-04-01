namespace FaceDetection
{
    partial class frmRecognizeTask
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRecognizeTask));
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitLeft = new System.Windows.Forms.SplitContainer();
            this.tableLayoutTitle = new System.Windows.Forms.TableLayoutPanel();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblClock = new System.Windows.Forms.Label();
            this.pbEmoji = new System.Windows.Forms.PictureBox();
            this.txtBarcodeScanner = new System.Windows.Forms.TextBox();
            this.pbBackground = new System.Windows.Forms.PictureBox();
            this.pbCamera = new System.Windows.Forms.PictureBox();
            this.lblSmileTitle = new System.Windows.Forms.Label();
            this.lblCode = new System.Windows.Forms.Label();
            this.lblSmile = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblNameTitle = new System.Windows.Forms.Label();
            this.lblCodeTitle = new System.Windows.Forms.Label();
            this.timerBlockUI = new System.Windows.Forms.Timer(this.components);
            this.splitParent = new System.Windows.Forms.SplitContainer();
            this.tableLayoutInfo = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.pbReport = new System.Windows.Forms.PictureBox();
            this.pbClose = new System.Windows.Forms.PictureBox();
            this.timerClock = new System.Windows.Forms.Timer(this.components);
            this.timerDoor = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).BeginInit();
            this.splitLeft.Panel1.SuspendLayout();
            this.splitLeft.Panel2.SuspendLayout();
            this.splitLeft.SuspendLayout();
            this.tableLayoutTitle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbEmoji)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitParent)).BeginInit();
            this.splitParent.Panel1.SuspendLayout();
            this.splitParent.Panel2.SuspendLayout();
            this.splitParent.SuspendLayout();
            this.tableLayoutInfo.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbReport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbClose)).BeginInit();
            this.SuspendLayout();
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.Color.White;
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Margin = new System.Windows.Forms.Padding(4);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.splitLeft);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.txtBarcodeScanner);
            this.splitMain.Panel2.Controls.Add(this.pbBackground);
            this.splitMain.Panel2.Controls.Add(this.pbCamera);
            this.splitMain.Size = new System.Drawing.Size(1712, 830);
            this.splitMain.SplitterDistance = 290;
            this.splitMain.SplitterWidth = 5;
            this.splitMain.TabIndex = 0;
            // 
            // splitLeft
            // 
            this.splitLeft.BackColor = System.Drawing.Color.White;
            this.splitLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLeft.Location = new System.Drawing.Point(0, 0);
            this.splitLeft.Margin = new System.Windows.Forms.Padding(4);
            this.splitLeft.Name = "splitLeft";
            this.splitLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitLeft.Panel1
            // 
            this.splitLeft.Panel1.BackColor = System.Drawing.Color.White;
            this.splitLeft.Panel1.Controls.Add(this.tableLayoutTitle);
            // 
            // splitLeft.Panel2
            // 
            this.splitLeft.Panel2.BackColor = System.Drawing.Color.MintCream;
            this.splitLeft.Panel2.Controls.Add(this.pbEmoji);
            this.splitLeft.Size = new System.Drawing.Size(290, 830);
            this.splitLeft.SplitterDistance = 492;
            this.splitLeft.SplitterWidth = 5;
            this.splitLeft.TabIndex = 1;
            // 
            // tableLayoutTitle
            // 
            this.tableLayoutTitle.ColumnCount = 1;
            this.tableLayoutTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutTitle.Controls.Add(this.lblWelcome, 0, 1);
            this.tableLayoutTitle.Controls.Add(this.lblClock, 0, 0);
            this.tableLayoutTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutTitle.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutTitle.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutTitle.Name = "tableLayoutTitle";
            this.tableLayoutTitle.RowCount = 2;
            this.tableLayoutTitle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutTitle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutTitle.Size = new System.Drawing.Size(290, 492);
            this.tableLayoutTitle.TabIndex = 5;
            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.BackColor = System.Drawing.Color.Transparent;
            this.lblWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWelcome.Font = new System.Drawing.Font("Palatino Linotype", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWelcome.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblWelcome.Location = new System.Drawing.Point(4, 100);
            this.lblWelcome.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(282, 392);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "Welcome to Spartronics Vietnam";
            this.lblWelcome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblClock
            // 
            this.lblClock.AutoSize = true;
            this.lblClock.BackColor = System.Drawing.Color.Transparent;
            this.lblClock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblClock.Font = new System.Drawing.Font("Palatino Linotype", 35F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClock.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblClock.Location = new System.Drawing.Point(3, 0);
            this.lblClock.Name = "lblClock";
            this.lblClock.Size = new System.Drawing.Size(284, 100);
            this.lblClock.TabIndex = 1;
            this.lblClock.Text = "07:30:00";
            this.lblClock.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbEmoji
            // 
            this.pbEmoji.BackColor = System.Drawing.Color.Lavender;
            this.pbEmoji.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbEmoji.Location = new System.Drawing.Point(0, 0);
            this.pbEmoji.Margin = new System.Windows.Forms.Padding(4);
            this.pbEmoji.Name = "pbEmoji";
            this.pbEmoji.Size = new System.Drawing.Size(290, 333);
            this.pbEmoji.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbEmoji.TabIndex = 1;
            this.pbEmoji.TabStop = false;
            // 
            // txtBarcodeScanner
            // 
            this.txtBarcodeScanner.Location = new System.Drawing.Point(1391, 3);
            this.txtBarcodeScanner.Name = "txtBarcodeScanner";
            this.txtBarcodeScanner.Size = new System.Drawing.Size(10, 22);
            this.txtBarcodeScanner.TabIndex = 4;
            this.txtBarcodeScanner.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBarcodeScanner_KeyDown);
            // 
            // pbBackground
            // 
            this.pbBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbBackground.Location = new System.Drawing.Point(0, 0);
            this.pbBackground.Name = "pbBackground";
            this.pbBackground.Size = new System.Drawing.Size(1417, 830);
            this.pbBackground.TabIndex = 3;
            this.pbBackground.TabStop = false;
            // 
            // pbCamera
            // 
            this.pbCamera.BackColor = System.Drawing.Color.Transparent;
            this.pbCamera.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbCamera.Image = global::FaceDetection.Properties.Resources.loader;
            this.pbCamera.Location = new System.Drawing.Point(0, 0);
            this.pbCamera.Margin = new System.Windows.Forms.Padding(4);
            this.pbCamera.Name = "pbCamera";
            this.pbCamera.Size = new System.Drawing.Size(1417, 830);
            this.pbCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbCamera.TabIndex = 2;
            this.pbCamera.TabStop = false;
            // 
            // lblSmileTitle
            // 
            this.lblSmileTitle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblSmileTitle.AutoSize = true;
            this.lblSmileTitle.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSmileTitle.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblSmileTitle.Location = new System.Drawing.Point(16, 11);
            this.lblSmileTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSmileTitle.Name = "lblSmileTitle";
            this.lblSmileTitle.Size = new System.Drawing.Size(100, 32);
            this.lblSmileTitle.TabIndex = 14;
            this.lblSmileTitle.Text = "Smile:";
            this.lblSmileTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCode
            // 
            this.lblCode.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblCode.AutoSize = true;
            this.lblCode.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCode.ForeColor = System.Drawing.Color.Black;
            this.lblCode.Location = new System.Drawing.Point(89, 7);
            this.lblCode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(151, 35);
            this.lblCode.TabIndex = 9;
            this.lblCode.Text = "Unknown";
            this.lblCode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSmile
            // 
            this.lblSmile.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblSmile.AutoSize = true;
            this.lblSmile.Font = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSmile.ForeColor = System.Drawing.Color.Black;
            this.lblSmile.Location = new System.Drawing.Point(113, 12);
            this.lblSmile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSmile.Name = "lblSmile";
            this.lblSmile.Size = new System.Drawing.Size(124, 30);
            this.lblSmile.TabIndex = 11;
            this.lblSmile.Text = "Unknown";
            this.lblSmile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.Black;
            this.lblName.Location = new System.Drawing.Point(117, 7);
            this.lblName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(151, 35);
            this.lblName.TabIndex = 10;
            this.lblName.Text = "Unknown";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNameTitle
            // 
            this.lblNameTitle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNameTitle.AutoSize = true;
            this.lblNameTitle.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNameTitle.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblNameTitle.Location = new System.Drawing.Point(21, 11);
            this.lblNameTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNameTitle.Name = "lblNameTitle";
            this.lblNameTitle.Size = new System.Drawing.Size(101, 32);
            this.lblNameTitle.TabIndex = 13;
            this.lblNameTitle.Text = "Name:";
            this.lblNameTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCodeTitle
            // 
            this.lblCodeTitle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblCodeTitle.AutoSize = true;
            this.lblCodeTitle.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCodeTitle.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblCodeTitle.Location = new System.Drawing.Point(4, 11);
            this.lblCodeTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCodeTitle.Name = "lblCodeTitle";
            this.lblCodeTitle.Size = new System.Drawing.Size(94, 32);
            this.lblCodeTitle.TabIndex = 12;
            this.lblCodeTitle.Text = "Code:";
            this.lblCodeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timerBlockUI
            // 
            this.timerBlockUI.Interval = 2000;
            this.timerBlockUI.Tick += new System.EventHandler(this.timerBlockUI_Tick);
            // 
            // splitParent
            // 
            this.splitParent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitParent.Location = new System.Drawing.Point(0, 0);
            this.splitParent.Margin = new System.Windows.Forms.Padding(4);
            this.splitParent.Name = "splitParent";
            this.splitParent.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitParent.Panel1
            // 
            this.splitParent.Panel1.Controls.Add(this.tableLayoutInfo);
            // 
            // splitParent.Panel2
            // 
            this.splitParent.Panel2.Controls.Add(this.splitMain);
            this.splitParent.Size = new System.Drawing.Size(1712, 897);
            this.splitParent.SplitterDistance = 62;
            this.splitParent.SplitterWidth = 5;
            this.splitParent.TabIndex = 1;
            // 
            // tableLayoutInfo
            // 
            this.tableLayoutInfo.BackColor = System.Drawing.Color.LightCoral;
            this.tableLayoutInfo.ColumnCount = 5;
            this.tableLayoutInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.tableLayoutInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 667F));
            this.tableLayoutInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.tableLayoutInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutInfo.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutInfo.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutInfo.Controls.Add(this.panel3, 2, 0);
            this.tableLayoutInfo.Controls.Add(this.panel4, 4, 0);
            this.tableLayoutInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutInfo.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutInfo.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutInfo.Name = "tableLayoutInfo";
            this.tableLayoutInfo.RowCount = 1;
            this.tableLayoutInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutInfo.Size = new System.Drawing.Size(1712, 62);
            this.tableLayoutInfo.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblCode);
            this.panel1.Controls.Add(this.lblCodeTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(259, 54);
            this.panel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblName);
            this.panel2.Controls.Add(this.lblNameTitle);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(271, 4);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(659, 54);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lblSmileTitle);
            this.panel3.Controls.Add(this.lblSmile);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(938, 4);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(259, 54);
            this.panel3.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.pbReport);
            this.panel4.Controls.Add(this.pbClose);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(1516, 4);
            this.panel4.Margin = new System.Windows.Forms.Padding(4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(192, 54);
            this.panel4.TabIndex = 2;
            // 
            // pbReport
            // 
            this.pbReport.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbReport.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbReport.BackgroundImage")));
            this.pbReport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbReport.Location = new System.Drawing.Point(39, 4);
            this.pbReport.Margin = new System.Windows.Forms.Padding(4);
            this.pbReport.Name = "pbReport";
            this.pbReport.Size = new System.Drawing.Size(53, 49);
            this.pbReport.TabIndex = 0;
            this.pbReport.TabStop = false;
            this.pbReport.Click += new System.EventHandler(this.pbReport_Click);
            // 
            // pbClose
            // 
            this.pbClose.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbClose.BackgroundImage = global::FaceDetection.Properties.Resources.red_power_button;
            this.pbClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbClose.Location = new System.Drawing.Point(117, 4);
            this.pbClose.Margin = new System.Windows.Forms.Padding(4);
            this.pbClose.Name = "pbClose";
            this.pbClose.Size = new System.Drawing.Size(53, 49);
            this.pbClose.TabIndex = 0;
            this.pbClose.TabStop = false;
            this.pbClose.Click += new System.EventHandler(this.pbClose_Click);
            // 
            // timerClock
            // 
            this.timerClock.Enabled = true;
            this.timerClock.Interval = 1000;
            this.timerClock.Tick += new System.EventHandler(this.timerClock_Tick);
            // 
            // timerDoor
            // 
            this.timerDoor.Enabled = true;
            this.timerDoor.Interval = 5000;
            this.timerDoor.Tick += new System.EventHandler(this.timerDoor_Tick);
            // 
            // frmRecognizeTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1712, 897);
            this.Controls.Add(this.splitParent);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmRecognizeTask";
            this.Text = "Recognize";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmRecognizeTask_Load);
            this.Shown += new System.EventHandler(this.frmRecognizeTask_Shown);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitLeft.Panel1.ResumeLayout(false);
            this.splitLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).EndInit();
            this.splitLeft.ResumeLayout(false);
            this.tableLayoutTitle.ResumeLayout(false);
            this.tableLayoutTitle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbEmoji)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).EndInit();
            this.splitParent.Panel1.ResumeLayout(false);
            this.splitParent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitParent)).EndInit();
            this.splitParent.ResumeLayout(false);
            this.tableLayoutInfo.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbReport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbClose)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Timer timerBlockUI;
        private System.Windows.Forms.SplitContainer splitLeft;
        private System.Windows.Forms.TableLayoutPanel tableLayoutTitle;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblSmileTitle;
        private System.Windows.Forms.Label lblNameTitle;
        private System.Windows.Forms.Label lblCodeTitle;
        private System.Windows.Forms.Label lblSmile;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.SplitContainer splitParent;
        private System.Windows.Forms.PictureBox pbCamera;
        private System.Windows.Forms.PictureBox pbClose;
        private System.Windows.Forms.TableLayoutPanel tableLayoutInfo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.PictureBox pbEmoji;
        private System.Windows.Forms.PictureBox pbReport;
        private System.Windows.Forms.PictureBox pbBackground;
        private System.Windows.Forms.Label lblClock;
        private System.Windows.Forms.Timer timerClock;
        private System.Windows.Forms.Timer timerDoor;
        private System.Windows.Forms.TextBox txtBarcodeScanner;
    }
}