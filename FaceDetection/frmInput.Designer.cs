namespace FaceDetection
{
    partial class frmInput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmInput));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pbCamera = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rdNoFaceMask = new System.Windows.Forms.RadioButton();
            this.rdHasFaceMask = new System.Windows.Forms.RadioButton();
            this.nudSimilarThreshold = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.cbbGroup = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnRemove = new System.Windows.Forms.Button();
            this.cbbUsers = new System.Windows.Forms.ComboBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblRequiredCode = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSimilarThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblTitle, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pbCamera, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 142F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1630, 840);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Old English Text MT", 40F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.lblTitle.Location = new System.Drawing.Point(4, 0);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1622, 86);
            this.lblTitle.TabIndex = 3;
            this.lblTitle.Text = "New user";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbCamera
            // 
            this.pbCamera.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pbCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbCamera.Location = new System.Drawing.Point(4, 232);
            this.pbCamera.Margin = new System.Windows.Forms.Padding(4);
            this.pbCamera.Name = "pbCamera";
            this.pbCamera.Size = new System.Drawing.Size(1622, 604);
            this.pbCamera.TabIndex = 1;
            this.pbCamera.TabStop = false;
            this.pbCamera.MouseLeave += new System.EventHandler(this.pbCamera_MouseLeave);
            this.pbCamera.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbCamera_MouseMove);
            this.pbCamera.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbCamera_MouseUp);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rdNoFaceMask);
            this.panel1.Controls.Add(this.rdHasFaceMask);
            this.panel1.Controls.Add(this.nudSimilarThreshold);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.cbbGroup);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnRemove);
            this.panel1.Controls.Add(this.cbbUsers);
            this.panel1.Controls.Add(this.lblMessage);
            this.panel1.Controls.Add(this.lblRequiredCode);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 90);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1622, 134);
            this.panel1.TabIndex = 0;
            // 
            // rdNoFaceMask
            // 
            this.rdNoFaceMask.AutoSize = true;
            this.rdNoFaceMask.Checked = true;
            this.rdNoFaceMask.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdNoFaceMask.ForeColor = System.Drawing.Color.Red;
            this.rdNoFaceMask.Location = new System.Drawing.Point(1080, 96);
            this.rdNoFaceMask.Name = "rdNoFaceMask";
            this.rdNoFaceMask.Size = new System.Drawing.Size(150, 24);
            this.rdNoFaceMask.TabIndex = 18;
            this.rdNoFaceMask.TabStop = true;
            this.rdNoFaceMask.Text = "No Face Mask";
            this.rdNoFaceMask.UseVisualStyleBackColor = true;
            this.rdNoFaceMask.CheckedChanged += new System.EventHandler(this.rdNoFaceMask_CheckedChanged);
            // 
            // rdHasFaceMask
            // 
            this.rdHasFaceMask.AutoSize = true;
            this.rdHasFaceMask.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdHasFaceMask.ForeColor = System.Drawing.Color.Red;
            this.rdHasFaceMask.Location = new System.Drawing.Point(894, 96);
            this.rdHasFaceMask.Name = "rdHasFaceMask";
            this.rdHasFaceMask.Size = new System.Drawing.Size(161, 24);
            this.rdHasFaceMask.TabIndex = 18;
            this.rdHasFaceMask.Text = "Has Face Mask";
            this.rdHasFaceMask.UseVisualStyleBackColor = true;
            this.rdHasFaceMask.CheckedChanged += new System.EventHandler(this.rdHasFaceMask_CheckedChanged);
            // 
            // nudSimilarThreshold
            // 
            this.nudSimilarThreshold.DecimalPlaces = 5;
            this.nudSimilarThreshold.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudSimilarThreshold.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudSimilarThreshold.Location = new System.Drawing.Point(1076, 39);
            this.nudSimilarThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.nudSimilarThreshold.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            262144});
            this.nudSimilarThreshold.Minimum = new decimal(new int[] {
            9988,
            0,
            0,
            262144});
            this.nudSimilarThreshold.Name = "nudSimilarThreshold";
            this.nudSimilarThreshold.Size = new System.Drawing.Size(116, 30);
            this.nudSimilarThreshold.TabIndex = 17;
            this.nudSimilarThreshold.Value = new decimal(new int[] {
            9988,
            0,
            0,
            262144});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label5.Location = new System.Drawing.Point(892, 43);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(163, 20);
            this.label5.TabIndex = 16;
            this.label5.Text = "Similar Threshold:";
            // 
            // cbbGroup
            // 
            this.cbbGroup.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cbbGroup.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbbGroup.FormattingEnabled = true;
            this.cbbGroup.Items.AddRange(new object[] {
            "Office",
            "Operator",
            "Security",
            "Janitor",
            "Driver",
            "Kitchen Attendant",
            "Garbage Collector",
            "Bonsai Caretaker"});
            this.cbbGroup.Location = new System.Drawing.Point(483, 38);
            this.cbbGroup.Margin = new System.Windows.Forms.Padding(4);
            this.cbbGroup.Name = "cbbGroup";
            this.cbbGroup.Size = new System.Drawing.Size(245, 33);
            this.cbbGroup.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label3.Location = new System.Drawing.Point(491, 12);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 20);
            this.label3.TabIndex = 11;
            this.label3.Text = "Group:";
            // 
            // btnRemove
            // 
            this.btnRemove.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemove.ForeColor = System.Drawing.Color.DarkRed;
            this.btnRemove.Location = new System.Drawing.Point(742, 33);
            this.btnRemove.Margin = new System.Windows.Forms.Padding(4);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(130, 43);
            this.btnRemove.TabIndex = 9;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = false;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // cbbUsers
            // 
            this.cbbUsers.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cbbUsers.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbbUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbbUsers.FormattingEnabled = true;
            this.cbbUsers.Location = new System.Drawing.Point(14, 38);
            this.cbbUsers.Margin = new System.Windows.Forms.Padding(4);
            this.cbbUsers.Name = "cbbUsers";
            this.cbbUsers.Size = new System.Drawing.Size(454, 33);
            this.cbbUsers.TabIndex = 1;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblMessage.Font = new System.Drawing.Font("Gill Sans MT", 16F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblMessage.Location = new System.Drawing.Point(12, 82);
            this.lblMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(312, 38);
            this.lblMessage.TabIndex = 8;
            this.lblMessage.Text = "Click here to start camera";
            this.lblMessage.Click += new System.EventHandler(this.lblMessage_Click);
            // 
            // lblRequiredCode
            // 
            this.lblRequiredCode.AutoSize = true;
            this.lblRequiredCode.ForeColor = System.Drawing.Color.Red;
            this.lblRequiredCode.Location = new System.Drawing.Point(227, 12);
            this.lblRequiredCode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRequiredCode.Name = "lblRequiredCode";
            this.lblRequiredCode.Size = new System.Drawing.Size(13, 17);
            this.lblRequiredCode.TabIndex = 5;
            this.lblRequiredCode.Text = "*";
            this.lblRequiredCode.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label2.Location = new System.Drawing.Point(66, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "(code - full name):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(16, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "User";
            // 
            // frmInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(1630, 840);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmInput";
            this.Text = "Add face";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmInput_FormClosing);
            this.Load += new System.EventHandler(this.frmInput_Load);
            this.Shown += new System.EventHandler(this.frmInput_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSimilarThreshold)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pbCamera;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.ComboBox cbbUsers;
        private System.Windows.Forms.Label lblRequiredCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.ComboBox cbbGroup;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudSimilarThreshold;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton rdNoFaceMask;
        private System.Windows.Forms.RadioButton rdHasFaceMask;
    }
}