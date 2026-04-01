using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FaceDetection
{
    public partial class frmRollCall : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<string> syncData = new List<string>();
        private List<string> failedData = new List<string>();

        public frmRollCall()
        {
            InitializeComponent();
        }

        private void frmRollCall_Load(object sender, EventArgs e)
        {
            List<Models.User> users = DB_TimeLog.GetUsers();
            foreach (var user in users)
            {
                cbbUsers.Items.Add($"{user.Username} - {user.NickName}");
            }

            try
            {
                using (StreamReader reader = new StreamReader(FaceDataHandler.FAILED_DATA_PATH))
                {
                    while (!reader.EndOfStream)
                    {
                        String line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                            syncData.Add(line);
                    }
                    btnSync.Text = $"Sync ({syncData.Count})";
                }
            }
            catch { }
        }

        private void UpdateData()
        {
            string code = cbbUsers.Text.Split('-')[0].Trim();
            if (chkAll.Checked)
                code = "";

            var table = DB_TimeLog.GetRollCall(dtpDate.Value, code);
            dgvRollCall.DataSource = table;
            if (dgvRollCall.Columns.Count >= 5)
            {
                dgvRollCall.Columns[0].Width = 100;
                dgvRollCall.Columns[1].Width = 200;
                dgvRollCall.Columns[2].Width = 300;
                dgvRollCall.Columns[3].Width = 200;
                dgvRollCall.Columns[4].Width = 200;
            }
        }

        private void cbbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void dtpDate_ValueChanged(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            UpdateData();
            cbbUsers.Enabled = !chkAll.Checked;
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            if (syncData.Count > 0)
            {
                int count = 0;
                btnSync.Enabled = false;
                failedData.Clear();
                foreach (string data in syncData)
                {
                    try
                    {
                        string[] s = data.Split(';');
                        // inserting if data is available
                        if (s.Length >= 3)
                        {
                            string code = s[0];
                            DateTime time = DateTime.Parse(s[1]);
                            int mode = Int32.Parse(s[2]);
                            if (DB_TimeLog.InsertData(code, time, mode))
                            {
                                int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Sync data successfully (code: {code}, date: {time}, inout: {mode})", "Info");
                                if (res == 0)
                                {
                                    log.Info($"Sync data successfully (code: {code}, date: {time}, inout: {mode})");
                                }
                                count++;
                            }
                            else
                            {
                                failedData.Add(data);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        failedData.Add(data);
                        int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] {ex.ToString()}", "Error");
                        if (res == 0)
                        {
                            log.Error(ex);
                        }
                    }
                }
                // write inserting failed data to file
                using (StreamWriter file = new StreamWriter(FaceDataHandler.FAILED_DATA_PATH))
                {
                    foreach (string data in failedData)
                    {
                        file.WriteLine(data);
                    }
                }

                MessageBox.Show($"Sync successfully {count} data!", "Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSync.Enabled = true;
                btnSync.Text = $"Sync ({failedData.Count})";

                UpdateData();
            }
        }
    }
}
