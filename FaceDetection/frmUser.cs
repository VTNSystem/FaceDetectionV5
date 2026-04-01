using Luxand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public frmUser()
        {
            InitializeComponent();
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

            /*// find added face data list
            string prefix = StringToPattern(FaceDataHandler.PREFIX);
            string suffix = StringToPattern(FaceDataHandler.SUFFIX);
            Regex regex = new Regex($@"({prefix})(.*?)({suffix})");
            using (StreamReader reader = new StreamReader(filePath))
            {
                string data = reader.ReadToEnd();
                var matches = regex.Matches(data);
                foreach (Match match in matches)
                {
                    string value = FaceDataHandler.GetNameFromIdentifier(match.Value);
                    string[] info = value.Split('-');
                    if (info.Length >= 2)
                    {
                        string code = info[0].Trim();
                        string name = info[1].Trim();
                        if (dictFace.ContainsKey(code))
                        {
                            dictFace[code]++;
                        }
                        else
                        {
                            dictFace.Add(code, 1);
                            dictName.Add(code, name);
                        }
                    }
                }
            }

            foreach (string key in dictFace.Keys)
            {
                dgvUser.Rows.Add(key, dictName[key], dictFace[key]);
            }*/

            // set miss data to sync
            try
            {
                syncData = FailedDataHandler.Read();
                btnSync.Text = $"Sync ({syncData.Count}) failed data";
            }
            catch { }
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

                    //frr.Template = new byte[FSDK.TemplateSize];
                    //reader.GetBytes(12, 0, frr.Template, 0, FSDK.TemplateSize);

                    //Image img = Image.FromStream(new System.IO.MemoryStream(reader.GetSqlBinary(13).Value));
                    Image img_face = Image.FromStream(new System.IO.MemoryStream(reader.GetSqlBinary(14).Value));
                    //frr.image = new FSDK.CImage(img);
                    frr.faceImage = new FSDK.CImage(img_face);

                    frr.SimilarThreshold = reader.GetFloat(15);
                    frr.CreatedDate = reader.GetDateTime(16);

                    lsTFace.Add(frr);

                    //img.Dispose();
                    //img_face.Dispose();
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
                        string id = (string)grid.Rows[e.RowIndex].Cells["ID"].Value;
                        string fullname = (string)grid.Rows[e.RowIndex].Cells["Fullname"].Value;
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

        private void cbbUserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetFaceListByName();
        }

        private void GetFaceListByName()
        {
            List<TFaceRecord> faceList = new List<TFaceRecord>();
            string keyname = cbbUserList.SelectedItem.ToString();
            string code = keyname.Split('-')[0].Trim();
            faceList = getListFaceByUsername(code);
            DataTable dtfacelist = new DataTable();
            dtfacelist.Columns.Add("ID");
            dtfacelist.Columns.Add("Code");
            dtfacelist.Columns.Add("Fullname");
            dtfacelist.Columns.Add("Group");
            dtfacelist.Columns.Add("SimilarThreshold");
            dtfacelist.Columns.Add("CreatedDate");
            dtfacelist.Columns.Add("Image");
            foreach (var fr in faceList)
            {
                dtfacelist.Rows.Add(fr.ID, fr.Code, fr.Fullname, fr.Group, fr.SimilarThreshold.ToString(), fr.CreatedDate.ToString());
            }
            dgvUser.AutoGenerateColumns = false;
            dgvUser.DataSource = dtfacelist;
            dgvUser.Refresh();
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
