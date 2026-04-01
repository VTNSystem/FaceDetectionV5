using EasyModbus;
using Luxand;
using System;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace FaceDetection
{
    public partial class frmSettings : Form
    {
        private Properties.Settings mSettings;

        public frmSettings()
        {
            InitializeComponent();
        }

        private void frmSettings_Shown(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources.hexagon_bg;
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            mSettings = Properties.Settings.Default;
            
            // database settings
            txtServer.Text = mSettings.Server;
            txtDatabase.Text = mSettings.Database;
            txtUsername.Text = mSettings.Username;
            if (!String.IsNullOrEmpty(mSettings.Password))
                txtPassword.Text = Encoding.UTF8.GetString(Convert.FromBase64String(mSettings.Password));
            nudDatabaseTimeout.Value = mSettings.DatabaseTimeout;

            //Face database settings
            txtFaceServer.Text = mSettings.FaceServer;
            txtFaceDatabase.Text = mSettings.FaceDatabase;
            txtFaceUsername.Text = mSettings.FaceUsername;
            if (!String.IsNullOrEmpty(mSettings.FacePassword))
                txtFacePassword.Text = Encoding.UTF8.GetString(Convert.FromBase64String(mSettings.FacePassword));
            nudFaceDatabaseTimeout.Value = mSettings.FaceDatabaseTimeout;

            // module settings
            txtIPAddress.Text = mSettings.IPAddress;
            txtPort.Text = mSettings.Port.ToString();
            nudModuleTimeout.Value = mSettings.ModuleTimeout;

            // Door Settings
            rdAlwayClose.Checked = mSettings.AlwayCloseDoor;
            rdOptionalOpen.Checked = mSettings.OptionalOpenDoor;
            cbRange1.Checked = mSettings.Range1;
            cbRange2.Checked = mSettings.Range2;
            cbRange3.Checked = mSettings.Range3;
            cbRange4.Checked = mSettings.Range4;
            if (mSettings.Range1)
            {
                fromTimePicker1.Enabled = true;
                toTimePicker1.Enabled = true;
            }
            else
            {
                fromTimePicker1.Enabled = false;
                toTimePicker1.Enabled = false;
            }
            if (mSettings.Range2)
            {
                fromTimePicker2.Enabled = true;
                toTimePicker2.Enabled = true;
            }
            else
            {
                fromTimePicker2.Enabled = false;
                toTimePicker2.Enabled = false;
            }
            if (mSettings.Range3)
            {
                fromTimePicker3.Enabled = true;
                toTimePicker3.Enabled = true;
            }
            else
            {
                fromTimePicker3.Enabled = false;
                toTimePicker3.Enabled = false;
            }
            if (mSettings.Range4)
            {
                fromTimePicker4.Enabled = true;
                toTimePicker4.Enabled = true;
            }
            else
            {
                fromTimePicker4.Enabled = false;
                toTimePicker4.Enabled = false;
            }

            fromTimePicker1.Text = mSettings.FromTime1;
            toTimePicker1.Text = mSettings.ToTime1;
            fromTimePicker2.Text = mSettings.FromTime2;
            toTimePicker2.Text = mSettings.ToTime2;
            fromTimePicker3.Text = mSettings.FromTime3;
            toTimePicker3.Text = mSettings.ToTime3;
            fromTimePicker4.Text = mSettings.FromTime4;
            toTimePicker4.Text = mSettings.ToTime4;

            // camera IP settings
            chkUseCameraIP.Checked = mSettings.UseCameraIP;
            ChkUseCameraIP_CheckedChanged(chkUseCameraIP, null);
            txtCameraURL.Text = mSettings.CameraIP_URL;
            txtCameraUsername.Text = mSettings.CameraIP_Username;
            txtCameraPassword.Text = mSettings.CameraIP_Password;
            nudCameraTimeout.Value = mSettings.CameraIP_Timeout;

            // general settings
            trbSmile.Value = mSettings.SmilePassed;
            trbSmile_Scroll(trbSmile, null);
            nudDoorTimeout.Value = mSettings.DoorTimeout;
            nudDetectionThreshold.Value = mSettings.DetectionThreshold;
            nudThreshold.Value = (decimal)mSettings.Threshold;
            chkLearning.Checked = mSettings.Learning;
            nudMemoryLimit.Value = mSettings.MemoryLimit;
            chkUseEmoji.Checked = mSettings.UseEmoji;
            chkInOut.Checked = mSettings.InOutMode;
            txtFailedMessage.Text = mSettings.FailedMessage;
            txtAdmin.Text = mSettings.AdminUsers;
            txtDataPath.Text = mSettings.FaceDataPath;
            txtPicturePath.Text = mSettings.PicturePath;
            txtReportPath.Text = mSettings.ReportPath;
            if (mSettings.ExtentDataPath != null)
            {
                foreach (string path in mSettings.ExtentDataPath)
                {
                    lbxExtendPath.Items.Add(path);
                }
            }

            string permission = mSettings.Permission;

            foreach (Control control in gbPermission.Controls)
            {
                if (control is CheckBox)
                {
                    ((CheckBox)control).Checked = permission.Contains(control.Text);
                }
            }

            chkAllowFaceMask.Checked = mSettings.AllowFaceMask;
            chkOnlyFaceMask.Checked = mSettings.OnlyFaceMask;
            chkTurnOffCamera.Checked = mSettings.TurnOffCamera;
            chkUseDCAM710.Checked = mSettings.UseDCAM710;

            // get all video format
            InitializeVideoFormat();
        }

        private void InitializeVideoFormat()
        {
            if (FSDK.FSDKE_OK != FSDK.ActivateLibrary(FaceDataHandler.LUXAND_LICENSE))
            {
                MessageBox.Show("Please run the License Key Wizard (Start - Luxand - FaceSDK - License Key Wizard)", "Error activating FaceSDK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            FSDK.InitializeLibrary();

            string[] cameraList;
            int count;
            FSDKCam.GetCameraList(out cameraList, out count);
            if (0 == count)
            {
                MessageBox.Show("Please attach a camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                cbbAttachedCamera.Items.AddRange(cameraList);
                cbbAttachedCamera.SelectedIndex = mSettings.AttachedCamera < cameraList.Length ? mSettings.AttachedCamera : 0;
                CbbAttachedCamera_SelectedIndexChanged(cbbAttachedCamera, null);
            }
        }

        private void Save()
        {
            mSettings.Server = txtServer.Text;
            mSettings.Database = txtDatabase.Text;
            mSettings.Username = txtUsername.Text;
            mSettings.Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(txtPassword.Text));
            mSettings.DatabaseTimeout = (int)nudDatabaseTimeout.Value;

            mSettings.FaceServer = txtFaceServer.Text;
            mSettings.FaceDatabase = txtFaceDatabase.Text;
            mSettings.FaceUsername = txtFaceUsername.Text;
            mSettings.FacePassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(txtFacePassword.Text));
            mSettings.FaceDatabaseTimeout = (int)nudFaceDatabaseTimeout.Value;

            mSettings.IPAddress = txtIPAddress.Text;
            mSettings.Port = int.Parse(txtPort.Text);
            mSettings.ModuleTimeout = (int)nudModuleTimeout.Value;

            mSettings.AlwayCloseDoor = rdAlwayClose.Checked;
            mSettings.OptionalOpenDoor = rdOptionalOpen.Checked;
            mSettings.Range1 = cbRange1.Checked;
            mSettings.Range2 = cbRange2.Checked;
            mSettings.Range3 = cbRange3.Checked;
            mSettings.Range4 = cbRange4.Checked;
            mSettings.FromTime1 = fromTimePicker1.Text;
            mSettings.ToTime1 = toTimePicker1.Text;
            mSettings.FromTime2 = fromTimePicker2.Text;
            mSettings.ToTime2 = toTimePicker2.Text;
            mSettings.FromTime3 = fromTimePicker3.Text;
            mSettings.ToTime3 = toTimePicker3.Text;
            mSettings.FromTime4 = fromTimePicker4.Text;
            mSettings.ToTime4 = toTimePicker4.Text;

            mSettings.UseCameraIP = chkUseCameraIP.Checked;
            mSettings.CameraIP_URL = txtCameraURL.Text;
            mSettings.CameraIP_Username = txtCameraUsername.Text;
            mSettings.CameraIP_Password = txtCameraPassword.Text;
            mSettings.CameraIP_Timeout = (int)nudCameraTimeout.Value;

            mSettings.SmilePassed = trbSmile.Value;
            mSettings.DoorTimeout = (int)nudDoorTimeout.Value;
            mSettings.UseEmoji = chkUseEmoji.Checked;
            mSettings.InOutMode = chkInOut.Checked;
            mSettings.FailedMessage = txtFailedMessage.Text.Trim();
            if (cbbVideoFormat.SelectedIndex >= 0)
                mSettings.CameraVideoFormat = cbbVideoFormat.SelectedIndex;
            if (cbbAttachedCamera.SelectedIndex >= 0)
                mSettings.AttachedCamera = cbbAttachedCamera.SelectedIndex;
            mSettings.AdminUsers = txtAdmin.Text.Trim();
            mSettings.FaceDataPath = txtDataPath.Text;
            mSettings.PicturePath = txtPicturePath.Text;
            mSettings.ReportPath = txtReportPath.Text;
            StringCollection extendPaths = new StringCollection();
            foreach (var item in lbxExtendPath.Items)
                extendPaths.Add(item.ToString());
            mSettings.ExtentDataPath = extendPaths;

            // Face parameters
            mSettings.DetectionThreshold = (int)nudDetectionThreshold.Value;
            mSettings.Threshold = (float)nudThreshold.Value;
            mSettings.MemoryLimit = (int)nudMemoryLimit.Value;
            mSettings.Learning = chkLearning.Checked;
            mSettings.AllowFaceMask = chkAllowFaceMask.Checked;
            mSettings.OnlyFaceMask = chkOnlyFaceMask.Checked;
            mSettings.TurnOffCamera = chkTurnOffCamera.Checked;
            mSettings.UseDCAM710 = chkUseDCAM710.Checked;

            mSettings.Permission = "";
            foreach (Control control in gbPermission.Controls)
            {
                CheckBox chk = control as CheckBox;
                if (chk != null && chk.Checked)
                {
                    mSettings.Permission += $"{control.Text};";
                }
            }

            mSettings.Save();
            
            // check SQL connection
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string server = Properties.Settings.Default.Server;
                string database = Properties.Settings.Default.Database;
                string user = Properties.Settings.Default.Username;
                string password = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Settings.Default.Password));
                int timeout = Properties.Settings.Default.DatabaseTimeout;
                DB_TimeLog.CheckConnection(server, database, user, password, timeout);
            });
        }

        private void trbSmile_Scroll(object sender, EventArgs e)
        {
            lblTrackedSmile.Text = trbSmile.Value.ToString() + "%";
        }

        private void btnPickPicture_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty(folderDialog.SelectedPath))
            {
                bool isPictureFolder = false;
                foreach (string file in System.IO.Directory.GetFiles(folderDialog.SelectedPath))
                {
                    if (".jpg|.jpeg|.png|.bmp|.gif".Contains(System.IO.Path.GetExtension(file)))
                    {
                        isPictureFolder = true;
                        break;
                    }
                }

                if (!isPictureFolder)
                {
                    MessageBox.Show("The selected folder doesn't contain any images!", "Missing image");
                }
                else
                {
                    txtPicturePath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void BtnReportPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty(folderDialog.SelectedPath))
            {
                txtReportPath.Text = folderDialog.SelectedPath;
            }
        }

        private void btnPickFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Data files (*.dat) | *.dat";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtDataPath.Text = fileDialog.FileName;
            }
        }

        private void btnExtendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Data files (*.dat) | *.dat";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = fileDialog.FileName;
                if (lbxExtendPath.Items.Contains(filePath))
                {
                    MessageBox.Show($"{filePath} has already added!", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lbxExtendPath.Items.Add(fileDialog.FileName);
                }
            }
        }

        private void btnCheckDatabase_Click(object sender, EventArgs e)
        {
            string server = txtServer.Text;
            string database = txtDatabase.Text;
            string user = txtUsername.Text;
            string password = txtPassword.Text;
            int timeout = (int)nudDatabaseTimeout.Value;
            string connectionString = $"Server={server};Database={database};User Id={user};Password={password};Connection Timeout={timeout}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show("Connected!", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed to connection to SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }
            }
        }

        private void btnCheckModule_Click(object sender, EventArgs e)
        {
            string ip = txtIPAddress.Text;
            int port = Int32.Parse(txtPort.Text);
            int timeout = (int)nudModuleTimeout.Value;

            try
            {
                ModbusClient modbusClient = new ModbusClient(ip, port);
                modbusClient.ConnectionTimeout = timeout;
                modbusClient.Connect();
                MessageBox.Show("Connected!", "Success");
            }
            catch (EasyModbus.Exceptions.ConnectionException ex)
            {
                MessageBox.Show(ex.Message, "Failed to connection to Module", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure to reset current settings?", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Properties.Settings.Default.Reset();
                frmSettings_Load(this, null);
            }
        }

        private void frmSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save();
        }

        private void BtnCheckCamera_Click(object sender, EventArgs e)
        {
            try
            {
                int cameraHandle = 0;
                int result = FSDKCam.OpenIPVideoCamera(FSDKCam.FSDK_VIDEOCOMPRESSIONTYPE.FSDK_MJPEG, txtCameraURL.Text, txtCameraUsername.Text,
                    txtCameraPassword.Text, (int)nudCameraTimeout.Value, ref cameraHandle);
                if (result != FSDK.FSDKE_OK)
                {
                    throw new Exception("Cannot connect to IP Camera!");
                }
                MessageBox.Show("Connected!", "Success");
                FSDKCam.CloseVideoCamera(cameraHandle);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void ChkUseCameraIP_CheckedChanged(object sender, EventArgs e)
        {
            txtCameraURL.Enabled = txtCameraUsername.Enabled = txtCameraPassword.Enabled
                = nudCameraTimeout.Enabled = btnCheckCamera.Enabled = chkUseCameraIP.Checked;
        }

        private void CbbAttachedCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            string cameraName = cbbAttachedCamera.Text;

            if (!String.IsNullOrEmpty(cameraName) && cameraName != cbbVideoFormat.Tag?.ToString())
            {
                int count;
                FSDKCam.VideoFormatInfo[] formatList;
                FSDKCam.GetVideoFormatList(ref cameraName, out formatList, out count);

                cbbVideoFormat.Items.Clear();
                foreach (var format in formatList)
                {
                    cbbVideoFormat.Items.Add($"{format.Width}x{format.Height}");
                }

                if (cbbVideoFormat.Items.Count > 0)
                {
                    cbbVideoFormat.SelectedIndex = mSettings.CameraVideoFormat < cbbVideoFormat.Items.Count
                        ? mSettings.CameraVideoFormat : 0;
                    // assign current camera name to tag property
                    cbbVideoFormat.Tag = cameraName;
                }
            }
        }

        private void lbxExtendPath_DoubleClick(object sender, EventArgs e)
        {
            string item = lbxExtendPath.SelectedItem?.ToString();
            if (item != null)
            {
                if (MessageBox.Show($"Do you want to delete {item}?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    lbxExtendPath.Items.Remove(item);
                }
            }
        }

        private void btnExtendPathUp_Click(object sender, EventArgs e)
        {
            int index = lbxExtendPath.SelectedIndex;
            if (index > 0)
            {
                // Swap selected item and upper item
                var temp = lbxExtendPath.Items[index];
                lbxExtendPath.Items[index] = lbxExtendPath.Items[index - 1];
                lbxExtendPath.Items[index - 1] = temp;
                lbxExtendPath.SelectedIndex = index - 1;
            }
        }

        private void btnExtendPathDown_Click(object sender, EventArgs e)
        {
            int index = lbxExtendPath.SelectedIndex;
            if (index < lbxExtendPath.Items.Count - 1)
            {
                // Swap selected item and lower item
                var temp = lbxExtendPath.Items[index];
                lbxExtendPath.Items[index] = lbxExtendPath.Items[index + 1];
                lbxExtendPath.Items[index + 1] = temp;
                lbxExtendPath.SelectedIndex = index + 1;
            }
        }

        private void btnCheckFaceDatabase_Click(object sender, EventArgs e)
        {
            string server = txtFaceServer.Text;
            string database = txtFaceDatabase.Text;
            string user = txtFaceUsername.Text;
            string password = txtFacePassword.Text;
            int timeout = (int)nudFaceDatabaseTimeout.Value;
            string connectionString = $"Server={server};Database={database};User Id={user};Password={password};Connection Timeout={timeout}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show("Connected!", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed to connection to SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }
            }
        }

        private void rdOptionalOpen_CheckedChanged(object sender, EventArgs e)
        {
            cbRange1.Enabled = true;
            cbRange1.Checked = false;
            cbRange2.Enabled = true;
            cbRange2.Checked = false;
            cbRange3.Enabled = true;
            cbRange3.Checked = false;
            cbRange4.Enabled = true;
            cbRange4.Checked = false;
        }

        private void rdAlwayClose_CheckedChanged(object sender, EventArgs e)
        {
            cbRange1.Enabled = false;
            cbRange2.Enabled = false;
            cbRange3.Enabled = false;
            cbRange4.Enabled = false;
            fromTimePicker1.Enabled = false;
            toTimePicker1.Enabled = false;
            fromTimePicker2.Enabled = false;
            toTimePicker2.Enabled = false;
            fromTimePicker3.Enabled = false;
            toTimePicker3.Enabled = false;
            fromTimePicker4.Enabled = false;
            toTimePicker4.Enabled = false;
        }

        private void cbRange1_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRange1.Checked)
            {
                fromTimePicker1.Enabled = true;
                toTimePicker1.Enabled = true;
            }
            else
            {
                fromTimePicker1.Enabled = false;
                toTimePicker1.Enabled = false;
            }
        }

        private void cbRange2_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRange2.Checked)
            {
                fromTimePicker2.Enabled = true;
                toTimePicker2.Enabled = true;
            }
            else
            {
                fromTimePicker2.Enabled = false;
                toTimePicker2.Enabled = false;
            }
        }

        private void cbRange3_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRange3.Checked)
            {
                fromTimePicker3.Enabled = true;
                toTimePicker3.Enabled = true;
            }
            else
            {
                fromTimePicker3.Enabled = false;
                toTimePicker3.Enabled = false;
            }
        }

        private void cbRange4_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRange4.Checked)
            {
                fromTimePicker4.Enabled = true;
                toTimePicker4.Enabled = true;
            }
            else
            {
                fromTimePicker4.Enabled = false;
                toTimePicker4.Enabled = false;
            }
        }
    }
}
