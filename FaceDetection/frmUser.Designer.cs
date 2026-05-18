namespace FaceDetection
{
    partial class frmUser
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
            this.dgvUser = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Group = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreatedDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSync = new System.Windows.Forms.Button();
            this.btDeleteAll = new System.Windows.Forms.Button();
            this.cbbUserList = new System.Windows.Forms.ComboBox();
            this.tlpGridAndPreview = new System.Windows.Forms.TableLayoutPanel();
            this.grpImageReview = new System.Windows.Forms.GroupBox();
            this.tlpReview = new System.Windows.Forms.TableLayoutPanel();
            this.lblReviewFull = new System.Windows.Forms.Label();
            this.pbReviewFull = new System.Windows.Forms.PictureBox();
            this.lblReviewFace = new System.Windows.Forms.Label();
            this.pbReviewFace = new System.Windows.Forms.PictureBox();
            this.panelImageSaveOptions = new System.Windows.Forms.Panel();
            this.tlpSaveBar = new System.Windows.Forms.TableLayoutPanel();
            this.lblSaveFolder = new System.Windows.Forms.Label();
            this.tlpFolderPick = new System.Windows.Forms.TableLayoutPanel();
            this.txtSaveFolder = new System.Windows.Forms.TextBox();
            this.btnBrowseSaveFolder = new System.Windows.Forms.Button();
            this.panelSaveButtonRow = new System.Windows.Forms.Panel();
            this.btnSaveFullImage = new System.Windows.Forms.Button();
            this.lblExcelLog = new System.Windows.Forms.Label();
            this.tlpExcelPick = new System.Windows.Forms.TableLayoutPanel();
            this.txtExcelPath = new System.Windows.Forms.TextBox();
            this.btnBrowseExcel = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUser)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tlpGridAndPreview.SuspendLayout();
            this.grpImageReview.SuspendLayout();
            this.tlpReview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbReviewFull)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbReviewFace)).BeginInit();
            this.panelImageSaveOptions.SuspendLayout();
            this.tlpSaveBar.SuspendLayout();
            this.tlpFolderPick.SuspendLayout();
            this.panelSaveButtonRow.SuspendLayout();
            this.tlpExcelPick.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvUser
            // 
            this.dgvUser.AllowUserToAddRows = false;
            this.dgvUser.BackgroundColor = System.Drawing.SystemColors.InactiveBorder;
            this.dgvUser.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUser.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.Code,
            this.FullName,
            this.Group,
            this.CreatedDate,
            this.Delete});
            this.dgvUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUser.Location = new System.Drawing.Point(3, 3);
            this.dgvUser.Name = "dgvUser";
            this.dgvUser.ReadOnly = true;
            this.dgvUser.RowHeadersWidth = 62;
            this.dgvUser.Size = new System.Drawing.Size(809, 649);
            this.dgvUser.TabIndex = 0;
            this.dgvUser.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUser_CellContentClick);
            this.dgvUser.SelectionChanged += new System.EventHandler(this.dgvUser_SelectionChanged);
            // 
            // ID
            // 
            this.ID.DataPropertyName = "ID";
            this.ID.HeaderText = "ID";
            this.ID.MinimumWidth = 8;
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 150;
            // 
            // Code
            // 
            this.Code.DataPropertyName = "Code";
            this.Code.HeaderText = "Code";
            this.Code.MinimumWidth = 8;
            this.Code.Name = "Code";
            this.Code.ReadOnly = true;
            this.Code.Width = 150;
            // 
            // FullName
            // 
            this.FullName.DataPropertyName = "FullName";
            this.FullName.HeaderText = "Full name";
            this.FullName.MinimumWidth = 8;
            this.FullName.Name = "FullName";
            this.FullName.ReadOnly = true;
            this.FullName.Width = 200;
            // 
            // Group
            // 
            this.Group.DataPropertyName = "Group";
            this.Group.HeaderText = "Group";
            this.Group.MinimumWidth = 8;
            this.Group.Name = "Group";
            this.Group.ReadOnly = true;
            this.Group.Width = 150;
            // 
            // CreatedDate
            // 
            this.CreatedDate.DataPropertyName = "CreatedDate";
            this.CreatedDate.HeaderText = "Created Date";
            this.CreatedDate.MinimumWidth = 8;
            this.CreatedDate.Name = "CreatedDate";
            this.CreatedDate.ReadOnly = true;
            this.CreatedDate.Width = 150;
            // 
            // Delete
            // 
            this.Delete.DataPropertyName = "Delete";
            this.Delete.HeaderText = "Action";
            this.Delete.MinimumWidth = 8;
            this.Delete.Name = "Delete";
            this.Delete.ReadOnly = true;
            this.Delete.Text = "Delete";
            this.Delete.UseColumnTextForButtonValue = true;
            this.Delete.Width = 150;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tlpGridAndPreview, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1321, 783);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSync);
            this.panel1.Controls.Add(this.btDeleteAll);
            this.panel1.Controls.Add(this.cbbUserList);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(2, 72);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1317, 48);
            this.panel1.TabIndex = 1;
            // 
            // btnSync
            // 
            this.btnSync.Location = new System.Drawing.Point(544, 14);
            this.btnSync.Margin = new System.Windows.Forms.Padding(2);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(160, 23);
            this.btnSync.TabIndex = 2;
            this.btnSync.Text = "Sync Data";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            // 
            // btDeleteAll
            // 
            this.btDeleteAll.Location = new System.Drawing.Point(363, 14);
            this.btDeleteAll.Margin = new System.Windows.Forms.Padding(2);
            this.btDeleteAll.Name = "btDeleteAll";
            this.btDeleteAll.Size = new System.Drawing.Size(160, 23);
            this.btDeleteAll.TabIndex = 1;
            this.btDeleteAll.Text = "Delete All";
            this.btDeleteAll.UseVisualStyleBackColor = true;
            this.btDeleteAll.Click += new System.EventHandler(this.btDeleteAll_Click);
            // 
            // cbbUserList
            // 
            this.cbbUserList.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cbbUserList.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbbUserList.FormattingEnabled = true;
            this.cbbUserList.ItemHeight = 13;
            this.cbbUserList.Location = new System.Drawing.Point(1, 15);
            this.cbbUserList.Margin = new System.Windows.Forms.Padding(2);
            this.cbbUserList.Name = "cbbUserList";
            this.cbbUserList.Size = new System.Drawing.Size(347, 21);
            this.cbbUserList.TabIndex = 0;
            // 
            // tlpGridAndPreview
            // 
            this.tlpGridAndPreview.ColumnCount = 2;
            this.tlpGridAndPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62F));
            this.tlpGridAndPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tlpGridAndPreview.Controls.Add(this.dgvUser, 0, 0);
            this.tlpGridAndPreview.Controls.Add(this.grpImageReview, 1, 0);
            this.tlpGridAndPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGridAndPreview.Location = new System.Drawing.Point(3, 125);
            this.tlpGridAndPreview.Name = "tlpGridAndPreview";
            this.tlpGridAndPreview.RowCount = 1;
            this.tlpGridAndPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGridAndPreview.Size = new System.Drawing.Size(1315, 655);
            this.tlpGridAndPreview.TabIndex = 3;
            // 
            // grpImageReview
            // 
            this.grpImageReview.Controls.Add(this.tlpReview);
            this.grpImageReview.Controls.Add(this.panelImageSaveOptions);
            this.grpImageReview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpImageReview.Location = new System.Drawing.Point(823, 3);
            this.grpImageReview.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.grpImageReview.Name = "grpImageReview";
            this.grpImageReview.Padding = new System.Windows.Forms.Padding(6);
            this.grpImageReview.Size = new System.Drawing.Size(489, 649);
            this.grpImageReview.TabIndex = 1;
            this.grpImageReview.TabStop = false;
            this.grpImageReview.Text = "Image Review";
            // 
            // tlpReview
            // 
            this.tlpReview.ColumnCount = 1;
            this.tlpReview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReview.Controls.Add(this.lblReviewFull, 0, 0);
            this.tlpReview.Controls.Add(this.pbReviewFull, 0, 1);
            this.tlpReview.Controls.Add(this.lblReviewFace, 0, 2);
            this.tlpReview.Controls.Add(this.pbReviewFace, 0, 3);
            this.tlpReview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpReview.Location = new System.Drawing.Point(6, 19);
            this.tlpReview.Name = "tlpReview";
            this.tlpReview.RowCount = 4;
            this.tlpReview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpReview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpReview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpReview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpReview.Size = new System.Drawing.Size(477, 468);
            this.tlpReview.TabIndex = 0;
            // 
            // lblReviewFull
            // 
            this.lblReviewFull.AutoSize = true;
            this.lblReviewFull.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblReviewFull.Location = new System.Drawing.Point(3, 0);
            this.lblReviewFull.Name = "lblReviewFull";
            this.lblReviewFull.Size = new System.Drawing.Size(471, 22);
            this.lblReviewFull.TabIndex = 0;
            this.lblReviewFull.Text = "Review Full";
            this.lblReviewFull.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbReviewFull
            // 
            this.pbReviewFull.BackColor = System.Drawing.Color.Black;
            this.pbReviewFull.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbReviewFull.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbReviewFull.Location = new System.Drawing.Point(3, 25);
            this.pbReviewFull.Name = "pbReviewFull";
            this.pbReviewFull.Size = new System.Drawing.Size(471, 206);
            this.pbReviewFull.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbReviewFull.TabIndex = 1;
            this.pbReviewFull.TabStop = false;
            // 
            // lblReviewFace
            // 
            this.lblReviewFace.AutoSize = true;
            this.lblReviewFace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblReviewFace.Location = new System.Drawing.Point(3, 234);
            this.lblReviewFace.Name = "lblReviewFace";
            this.lblReviewFace.Size = new System.Drawing.Size(471, 22);
            this.lblReviewFace.TabIndex = 2;
            this.lblReviewFace.Text = "Review Face";
            this.lblReviewFace.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbReviewFace
            // 
            this.pbReviewFace.BackColor = System.Drawing.Color.Black;
            this.pbReviewFace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbReviewFace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbReviewFace.Location = new System.Drawing.Point(3, 259);
            this.pbReviewFace.Name = "pbReviewFace";
            this.pbReviewFace.Size = new System.Drawing.Size(471, 206);
            this.pbReviewFace.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbReviewFace.TabIndex = 3;
            this.pbReviewFace.TabStop = false;
            // 
            // panelImageSaveOptions
            // 
            this.panelImageSaveOptions.Controls.Add(this.tlpSaveBar);
            this.panelImageSaveOptions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelImageSaveOptions.Location = new System.Drawing.Point(6, 487);
            this.panelImageSaveOptions.Name = "panelImageSaveOptions";
            this.panelImageSaveOptions.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.panelImageSaveOptions.Size = new System.Drawing.Size(477, 156);
            this.panelImageSaveOptions.TabIndex = 1;
            // 
            // tlpSaveBar
            // 
            this.tlpSaveBar.ColumnCount = 1;
            this.tlpSaveBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSaveBar.Controls.Add(this.lblSaveFolder, 0, 0);
            this.tlpSaveBar.Controls.Add(this.tlpFolderPick, 0, 1);
            this.tlpSaveBar.Controls.Add(this.panelSaveButtonRow, 0, 2);
            this.tlpSaveBar.Controls.Add(this.lblExcelLog, 0, 3);
            this.tlpSaveBar.Controls.Add(this.tlpExcelPick, 0, 4);
            this.tlpSaveBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSaveBar.Location = new System.Drawing.Point(0, 4);
            this.tlpSaveBar.Name = "tlpSaveBar";
            this.tlpSaveBar.RowCount = 5;
            this.tlpSaveBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tlpSaveBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpSaveBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpSaveBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tlpSaveBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpSaveBar.Size = new System.Drawing.Size(477, 152);
            this.tlpSaveBar.TabIndex = 0;
            // 
            // lblSaveFolder
            // 
            this.lblSaveFolder.AutoSize = true;
            this.lblSaveFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSaveFolder.Location = new System.Drawing.Point(3, 0);
            this.lblSaveFolder.Name = "lblSaveFolder";
            this.lblSaveFolder.Size = new System.Drawing.Size(471, 18);
            this.lblSaveFolder.TabIndex = 0;
            this.lblSaveFolder.Text = "Save folder";
            this.lblSaveFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpFolderPick
            // 
            this.tlpFolderPick.ColumnCount = 2;
            this.tlpFolderPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFolderPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.tlpFolderPick.Controls.Add(this.txtSaveFolder, 0, 0);
            this.tlpFolderPick.Controls.Add(this.btnBrowseSaveFolder, 1, 0);
            this.tlpFolderPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFolderPick.Location = new System.Drawing.Point(3, 21);
            this.tlpFolderPick.Name = "tlpFolderPick";
            this.tlpFolderPick.RowCount = 1;
            this.tlpFolderPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFolderPick.Size = new System.Drawing.Size(471, 28);
            this.tlpFolderPick.TabIndex = 1;
            // 
            // txtSaveFolder
            // 
            this.txtSaveFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSaveFolder.Location = new System.Drawing.Point(3, 3);
            this.txtSaveFolder.MinimumSize = new System.Drawing.Size(4, 24);
            this.txtSaveFolder.Name = "txtSaveFolder";
            this.txtSaveFolder.Size = new System.Drawing.Size(341, 20);
            this.txtSaveFolder.TabIndex = 0;
            // 
            // btnBrowseSaveFolder
            // 
            this.btnBrowseSaveFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowseSaveFolder.Location = new System.Drawing.Point(350, 3);
            this.btnBrowseSaveFolder.MinimumSize = new System.Drawing.Size(112, 24);
            this.btnBrowseSaveFolder.Name = "btnBrowseSaveFolder";
            this.btnBrowseSaveFolder.Size = new System.Drawing.Size(118, 24);
            this.btnBrowseSaveFolder.TabIndex = 1;
            this.btnBrowseSaveFolder.Text = "Browse…";
            this.btnBrowseSaveFolder.UseVisualStyleBackColor = true;
            this.btnBrowseSaveFolder.Click += new System.EventHandler(this.btnBrowseSaveFolder_Click);
            // 
            // panelSaveButtonRow
            // 
            this.panelSaveButtonRow.Controls.Add(this.btnSaveFullImage);
            this.panelSaveButtonRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSaveButtonRow.Location = new System.Drawing.Point(3, 55);
            this.panelSaveButtonRow.Name = "panelSaveButtonRow";
            this.panelSaveButtonRow.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.panelSaveButtonRow.Size = new System.Drawing.Size(471, 30);
            this.panelSaveButtonRow.TabIndex = 2;
            // 
            // btnSaveFullImage
            // 
            this.btnSaveFullImage.AutoSize = true;
            this.btnSaveFullImage.Location = new System.Drawing.Point(0, 2);
            this.btnSaveFullImage.Name = "btnSaveFullImage";
            this.btnSaveFullImage.Size = new System.Drawing.Size(99, 23);
            this.btnSaveFullImage.TabIndex = 0;
            this.btnSaveFullImage.Text = "Save full image";
            this.btnSaveFullImage.UseVisualStyleBackColor = true;
            this.btnSaveFullImage.Click += new System.EventHandler(this.btnSaveFullImage_Click);
            // 
            // lblExcelLog
            // 
            this.lblExcelLog.AutoSize = true;
            this.lblExcelLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExcelLog.Location = new System.Drawing.Point(3, 88);
            this.lblExcelLog.Name = "lblExcelLog";
            this.lblExcelLog.Size = new System.Drawing.Size(471, 18);
            this.lblExcelLog.TabIndex = 3;
            this.lblExcelLog.Text = "Excel file (.xlsx)";
            this.lblExcelLog.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpExcelPick
            // 
            this.tlpExcelPick.ColumnCount = 2;
            this.tlpExcelPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpExcelPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.tlpExcelPick.Controls.Add(this.txtExcelPath, 0, 0);
            this.tlpExcelPick.Controls.Add(this.btnBrowseExcel, 1, 0);
            this.tlpExcelPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpExcelPick.Location = new System.Drawing.Point(3, 109);
            this.tlpExcelPick.Name = "tlpExcelPick";
            this.tlpExcelPick.RowCount = 1;
            this.tlpExcelPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpExcelPick.Size = new System.Drawing.Size(471, 40);
            this.tlpExcelPick.TabIndex = 4;
            // 
            // txtExcelPath
            // 
            this.txtExcelPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtExcelPath.Location = new System.Drawing.Point(3, 3);
            this.txtExcelPath.Name = "txtExcelPath";
            this.txtExcelPath.Size = new System.Drawing.Size(341, 20);
            this.txtExcelPath.TabIndex = 0;
            // 
            // btnBrowseExcel
            // 
            this.btnBrowseExcel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowseExcel.Location = new System.Drawing.Point(350, 3);
            this.btnBrowseExcel.MaximumSize = new System.Drawing.Size(0, 24);
            this.btnBrowseExcel.MinimumSize = new System.Drawing.Size(112, 24);
            this.btnBrowseExcel.Name = "btnBrowseExcel";
            this.btnBrowseExcel.Size = new System.Drawing.Size(118, 24);
            this.btnBrowseExcel.TabIndex = 1;
            this.btnBrowseExcel.Text = "Browse…";
            this.btnBrowseExcel.UseVisualStyleBackColor = true;
            this.btnBrowseExcel.Click += new System.EventHandler(this.btnBrowseExcel_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(2, 2);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1317, 66);
            this.panel2.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 40F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1317, 66);
            this.label1.TabIndex = 0;
            this.label1.Text = "Face List";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1321, 783);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "frmUser";
            this.Text = "Users";
            this.Load += new System.EventHandler(this.frmUser_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUser)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tlpGridAndPreview.ResumeLayout(false);
            this.grpImageReview.ResumeLayout(false);
            this.tlpReview.ResumeLayout(false);
            this.tlpReview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbReviewFull)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbReviewFace)).EndInit();
            this.panelImageSaveOptions.ResumeLayout(false);
            this.tlpSaveBar.ResumeLayout(false);
            this.tlpSaveBar.PerformLayout();
            this.tlpFolderPick.ResumeLayout(false);
            this.tlpFolderPick.PerformLayout();
            this.panelSaveButtonRow.ResumeLayout(false);
            this.panelSaveButtonRow.PerformLayout();
            this.tlpExcelPick.ResumeLayout(false);
            this.tlpExcelPick.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvUser;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btDeleteAll;
        private System.Windows.Forms.ComboBox cbbUserList;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Code;
        private System.Windows.Forms.DataGridViewTextBoxColumn FullName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Group;
        private System.Windows.Forms.DataGridViewTextBoxColumn CreatedDate;
        private System.Windows.Forms.DataGridViewButtonColumn Delete;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.TableLayoutPanel tlpGridAndPreview;
        private System.Windows.Forms.GroupBox grpImageReview;
        private System.Windows.Forms.TableLayoutPanel tlpReview;
        private System.Windows.Forms.Label lblReviewFull;
        private System.Windows.Forms.PictureBox pbReviewFull;
        private System.Windows.Forms.Label lblReviewFace;
        private System.Windows.Forms.PictureBox pbReviewFace;
        private System.Windows.Forms.Panel panelImageSaveOptions;
        private System.Windows.Forms.TableLayoutPanel tlpSaveBar;
        private System.Windows.Forms.Label lblSaveFolder;
        private System.Windows.Forms.TableLayoutPanel tlpFolderPick;
        private System.Windows.Forms.TextBox txtSaveFolder;
        private System.Windows.Forms.Button btnBrowseSaveFolder;
        private System.Windows.Forms.Panel panelSaveButtonRow;
        private System.Windows.Forms.Button btnSaveFullImage;
        private System.Windows.Forms.Label lblExcelLog;
        private System.Windows.Forms.TableLayoutPanel tlpExcelPick;
        private System.Windows.Forms.TextBox txtExcelPath;
        private System.Windows.Forms.Button btnBrowseExcel;
    }
}