using Luxand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FaceDetection
{
    public partial class frmUser : Form
    {
        private List<string> syncData = new List<string>();

        Dictionary<string, int> dictFace = new Dictionary<string, int>();
        Dictionary<string, string> dictName = new Dictionary<string, string>();

        private readonly Dictionary<int, Image> _previewFullById = new Dictionary<int, Image>();
        private readonly Dictionary<int, Image> _previewFaceById = new Dictionary<int, Image>();

        public frmUser()
        {
            InitializeComponent();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ClearPreviewResources();
            base.OnFormClosed(e);
        }

        private void ClearPreviewResources()
        {
            pbReviewFull.Image = null;
            pbReviewFace.Image = null;
            foreach (var img in _previewFullById.Values)
                img.Dispose();
            _previewFullById.Clear();
            foreach (var img in _previewFaceById.Values)
                img.Dispose();
            _previewFaceById.Clear();
        }

        private void dgvUser_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUser.CurrentRow == null || dgvUser.CurrentRow.IsNewRow)
            {
                pbReviewFull.Image = null;
                pbReviewFace.Image = null;
                return;
            }

            object idVal = dgvUser.CurrentRow.Cells["ID"].Value;
            if (idVal == null || idVal == DBNull.Value)
            {
                pbReviewFull.Image = null;
                pbReviewFace.Image = null;
                return;
            }

            int id = Convert.ToInt32(idVal);
            pbReviewFull.Image = _previewFullById.TryGetValue(id, out Image full) ? full : null;
            pbReviewFace.Image = _previewFaceById.TryGetValue(id, out Image face) ? face : null;
        }

        private static void PersistFaceListSaveFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;
            var s = Properties.Settings.Default;
            s.FaceListImageSaveFolder = folderPath.Trim();
            s.Save();
        }

        private static void PersistFaceListExcelPath(string excelPath)
        {
            if (string.IsNullOrWhiteSpace(excelPath))
                return;
            var s = Properties.Settings.Default;
            s.FaceListExcelPath = excelPath.Trim();
            s.Save();
        }

        private void btnBrowseExcel_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Excel workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                dlg.Title = "Select Excel file";
                string cur = txtExcelPath.Text.Trim();
                if (!string.IsNullOrEmpty(cur))
                    dlg.FileName = cur;
                else
                {
                    string saved = (Properties.Settings.Default.FaceListExcelPath ?? "").Trim();
                    if (!string.IsNullOrEmpty(saved))
                        dlg.FileName = saved;
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtExcelPath.Text = dlg.FileName;
                    PersistFaceListExcelPath(dlg.FileName);
                }
            }
        }

        private void btnBrowseSaveFolder_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select folder to save images";
                dlg.ShowNewFolderButton = true;
                string initial = txtSaveFolder.Text.Trim();
                if (string.IsNullOrEmpty(initial))
                    initial = (Properties.Settings.Default.FaceListImageSaveFolder ?? "").Trim();
                if (!string.IsNullOrEmpty(initial) && Directory.Exists(initial))
                    dlg.SelectedPath = initial;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtSaveFolder.Text = dlg.SelectedPath;
                    PersistFaceListSaveFolder(dlg.SelectedPath);
                }
            }
        }

        private void btnSaveFullImage_Click(object sender, EventArgs e)
        {
            if (dgvUser.CurrentRow == null || dgvUser.CurrentRow.IsNewRow)
            {
                MessageBox.Show(this, "Select a row in the list.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string folder = txtSaveFolder.Text.Trim();
            if (string.IsNullOrEmpty(folder))
            {
                MessageBox.Show(this, "Choose or enter a save folder.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not create folder: " + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            object idVal = dgvUser.CurrentRow.Cells["ID"].Value;
            if (idVal == null || idVal == DBNull.Value)
            {
                MessageBox.Show(this, "Could not resolve the selected record.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(idVal);
            if (!_previewFullById.TryGetValue(id, out Image source) || source == null)
            {
                MessageBox.Show(this, "No full image is available for the selected row.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string empCode = Convert.ToString(dgvUser.CurrentRow.Cells["Code"].Value);
            string empName = Convert.ToString(dgvUser.CurrentRow.Cells["FullName"].Value);
            string fileName = SanitizeFileNamePart(empCode) + " - " + SanitizeFileNamePart(empName) + ".jpg";
            string fullPath = Path.Combine(folder, fileName);

            try
            {
                using (Bitmap copy = new Bitmap(source))
                    copy.Save(fullPath, ImageFormat.Jpeg);
                PersistFaceListSaveFolder(folder);

                string excelPath = txtExcelPath.Text.Trim();
                if (!string.IsNullOrEmpty(excelPath))
                {
                    try
                    {
                        FaceListExcelLog.AppendRow(excelPath, empCode, empName, fullPath);
                        PersistFaceListExcelPath(excelPath);
                    }
                    catch (Exception exExcel)
                    {
                        MessageBox.Show(this,
                            "Image saved, but the Excel file could not be updated:\n" + exExcel.Message,
                            "Save",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }

                MessageBox.Show(this, "Saved:\n" + fullPath, "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Save failed: " + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string SanitizeFileNamePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";
            char[] invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(value.Trim().Length);
            foreach (char c in value.Trim())
            {
                if (Array.IndexOf(invalid, c) >= 0 || c < 32)
                    sb.Append('_');
                else
                    sb.Append(c);
            }
            string s = sb.ToString().Trim();
            return string.IsNullOrEmpty(s) ? "Unknown" : s;
        }

        private void frmUser_Load(object sender, EventArgs e)
        {
            string filePath = FaceDataHandler.GetTempDataFilePath(Properties.Settings.Default.FaceDataPath);

            var list = DB_TimeLog.GetUsers();
            foreach (var user in list)
            {
                if (user.Active)
                {
                    cbbUserList.Items.Add(user.Username + " - " + user.FullName);
                }
            }
            try
            {
                syncData = FailedDataHandler.Read();
                btnSync.Text = $"Sync ({syncData.Count}) failed data";
            }
            catch 
            { 
            
            }

            string savedFolder = Properties.Settings.Default.FaceListImageSaveFolder;
            if (!string.IsNullOrWhiteSpace(savedFolder))
                txtSaveFolder.Text = savedFolder.Trim();

            string savedExcel = Properties.Settings.Default.FaceListExcelPath;
            if (!string.IsNullOrWhiteSpace(savedExcel))
                txtExcelPath.Text = savedExcel.Trim();
        }

        private string StringToPattern(string text)
        {
            return Regex.Replace(text, @"[-\/\\^$*+?.()|[\]{}]", "\\$&");
        }

        /*private void btnSync_Click(object sender, EventArgs e)
        {
            if (syncData.Count > 0)
            {
                btnSync.Enabled = false;
                int count = FailedDataHandler.Sync();
                syncData = FailedDataHandler.Read();

                MessageBox.Show($"Sync successfully {count} data!", "Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSync.Enabled = true;
                btnSync.Text = $"Sync ({syncData.Count}) failed data";
            }
        }*/

        private void RemoveFaceByCode(string Code)
        {
            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {
                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + frmMain.SQLServerAddress + "; initial catalog=" + frmMain.DatabaseName + "; User Id=" + frmMain.DBUid + "; Pwd=" + frmMain.DBPwd);
                sqlConnect.Open();
                System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand("DELETE FROM FaceList Where Code = '" + Code + "'", sqlConnect);

                int iresult = sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception on delete Face from database");
            }
            finally
            {
                if (sqlConnect != null)
                    sqlConnect.Close();
            }
        }

        private void RemoveFaceByID(string ID)
        {
            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {
                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + frmMain.SQLServerAddress + "; initial catalog=" + frmMain.DatabaseName + "; User Id=" + frmMain.DBUid + "; Pwd=" + frmMain.DBPwd);
                sqlConnect.Open();
                System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand("DELETE FROM [FaceList] Where ID = " + ID, sqlConnect);

                int iresult = sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception on delete Face from database");
            }
            finally
            {
                if (sqlConnect != null)
                    sqlConnect.Close();
            }
        }

        private List<TFaceRecord> getListFaceByUsername(string code)
        {
            List<TFaceRecord> lsTFace = new List<TFaceRecord>();
            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {
                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + frmMain.SQLServerAddress + "; initial catalog=" + frmMain.DatabaseName + "; User Id=" + frmMain.DBUid + "; Pwd=" + frmMain.DBPwd);
                sqlConnect.Open();

                System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand("SELECT ID, Code, Fullname, [Group], FacePositionXc, FacePositionYc, FacePositionW, FacePositionAngle, Eye1X, Eye1Y, Eye2X, Eye2Y, Template, Image, FaceImage, [SimilarThreshold], CreatedDate FROM FaceList WHERE Code = '" + code + "'", sqlConnect);

                System.Data.SqlClient.SqlDataReader reader = sqlCmd.ExecuteReader();
                while (reader.Read())
                {
                    TFaceRecord frr = new TFaceRecord();
                    frr.ID = reader.GetInt32(0);
                    frr.Code = reader.GetString(1);
                    frr.Fullname = reader.GetString(2);
                    frr.Group = reader.GetString(3);

                    frr.FacePosition = new FSDK.TFacePosition();
                    frr.FacePosition.xc = reader.GetInt32(4);
                    frr.FacePosition.yc = reader.GetInt32(5);
                    frr.FacePosition.w = reader.GetInt32(6);
                    frr.FacePosition.angle = reader.GetFloat(7);

                    frr.FacialFeatures = new FSDK.TPoint[2];
                    frr.FacialFeatures[0] = new FSDK.TPoint();
                    frr.FacialFeatures[0].x = reader.GetInt32(8);
                    frr.FacialFeatures[0].y = reader.GetInt32(9);
                    frr.FacialFeatures[1] = new FSDK.TPoint();
                    frr.FacialFeatures[1].x = reader.GetInt32(10);
                    frr.FacialFeatures[1].y = reader.GetInt32(11);

                    frr.Template = new byte[FSDK.TemplateSize];
                    reader.GetBytes(12, 0, frr.Template, 0, FSDK.TemplateSize);

                    if (!reader.IsDBNull(13) && !reader.IsDBNull(14))
                    {
                        byte[] fullBin = (byte[])reader[13];
                        byte[] faceBin = (byte[])reader[14];
                        Image img = null;
                        Image img_face = null;
                        try
                        {
                            using (var ms = new MemoryStream(fullBin))
                                img = Image.FromStream(ms);
                            frr.image = new FSDK.CImage(img);
                        }
                        finally
                        {
                            img?.Dispose();
                        }

                        using (var ms = new MemoryStream(fullBin))
                        using (Image tmp = Image.FromStream(ms))
                            _previewFullById[frr.ID] = new Bitmap(tmp);

                        try
                        {
                            using (var ms = new MemoryStream(faceBin))
                                img_face = Image.FromStream(ms);
                            frr.faceImage = new FSDK.CImage(img_face);
                        }
                        finally
                        {
                            img_face?.Dispose();
                        }

                        using (var ms = new MemoryStream(faceBin))
                        using (Image tmp = Image.FromStream(ms))
                            _previewFaceById[frr.ID] = new Bitmap(tmp);
                    }

                    frr.SimilarThreshold = reader.GetFloat(15);
                    frr.CreatedDate = reader.GetDateTime(16);

                    lsTFace.Add(frr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception on loading database");
            }
            finally
            {
                if (sqlConnect != null)
                    sqlConnect.Close();
            }
            return lsTFace;
        }

        private void btDeleteAll_Click(object sender, EventArgs e)
        {
            string keyName = cbbUserList.Text.Trim();
            string code = keyName.Split('-')[0].Trim();
            string fullname = keyName.Split('-')[1].Trim();
            DialogResult result = MessageBox.Show("Do you want to delete face list of " + code + "-" + fullname + "?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                RemoveFaceByCode(code);
                MessageBox.Show($"Deleted {code} - {fullname}");
                GetFaceListByName();
            }
        }

        private void dgvUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = (DataGridView)sender;
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == this.Delete.Index)
                {
                    if (grid[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell)
                    {
                        string id = Convert.ToString(grid.Rows[e.RowIndex].Cells["ID"].Value);
                        string fullname = Convert.ToString(grid.Rows[e.RowIndex].Cells["FullName"].Value);
                        DialogResult result = MessageBox.Show("Do you want to delete face of " + id + "-" + fullname + "?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            RemoveFaceByID(id);
                            MessageBox.Show($"Deleted {id} - {fullname}");
                            GetFaceListByName();
                        }
                    }
                }
            }
        }

 
        private void GetFaceListByName()
        {
            if (cbbUserList.SelectedItem == null)
                return;

            ClearPreviewResources();

            string keyname = cbbUserList.SelectedItem.ToString();
            string code = keyname.Split('-')[0].Trim();
            List<TFaceRecord> faceList = getListFaceByUsername(code);
            DataTable dtfacelist = new DataTable();
            dtfacelist.Columns.Add("ID", typeof(int));
            dtfacelist.Columns.Add("Code", typeof(string));
            dtfacelist.Columns.Add("FullName", typeof(string));
            dtfacelist.Columns.Add("Group", typeof(string));
            dtfacelist.Columns.Add("CreatedDate", typeof(string));
            foreach (var fr in faceList)
            {
                dtfacelist.Rows.Add(fr.ID, fr.Code, fr.Fullname, fr.Group, fr.CreatedDate.ToString());
            }
            dgvUser.AutoGenerateColumns = false;
            dgvUser.DataSource = dtfacelist;
            dgvUser.Refresh();

            if (dgvUser.Rows.Count > 0)
            {
                dgvUser.ClearSelection();
                dgvUser.Rows[0].Selected = true;
                dgvUser.CurrentCell = dgvUser.Rows[0].Cells["Code"];
            }
            else
            {
                pbReviewFull.Image = null;
                pbReviewFace.Image = null;
            }
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            if (syncData.Count > 0)
            {
                btnSync.Enabled = false;
                int count = FailedDataHandler.Sync();
                syncData = FailedDataHandler.Read();

                MessageBox.Show($"Sync successfully {count} data!", "Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSync.Enabled = true;
                btnSync.Text = $"Sync ({syncData.Count}) failed data";
            }
        }
    }
}
