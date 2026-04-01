using Luxand;
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
    public struct TFaceRecord
    {
        public byte[] Template;
        public FSDK.TFacePosition FacePosition;
        public FSDK.TPoint[] FacialFeatures;

        public int ID;
        public string Code;
        public string Fullname;
        public string Group;
        public float SimilarThreshold;
        public DateTime CreatedDate;
        public bool HasFaceMask;

        public FSDK.CImage image;
        public FSDK.CImage faceImage;
    }

    public partial class frmMain : Form
    {
        public static string SQLServerAddress = Properties.Settings.Default.FaceServer;
        public static string DatabaseName = Properties.Settings.Default.FaceDatabase;
        public static string DBUid = Properties.Settings.Default.FaceUsername;
        public static string DBPwd = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Settings.Default.FacePassword));

        public static List<TFaceRecord> FaceList = new List<TFaceRecord>();

        public frmMain()
        {
            InitializeComponent();

            // Remove default background from MdiClient to display background image
            foreach (Control control in this.Controls)
            {
                if (control is MdiClient)
                {
                    control.BackColor = Color.Black;
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //pbHome_Click(pbHome, null);
            //SetMdiChild(new frmRecognition());
            SetMdiChild(new frmHome());
            this.FormBorderStyle = FormBorderStyle.None;

            try
            {
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
            catch { }

            FaceList = frmMain.LoadFaceList();
        }

        private void pbHome_Click(object sender, EventArgs e)
        {
            SetMdiChild(new frmHome());
        }

        private void pbFullScreen_Click(object sender, EventArgs e)
        {
            //this.FormBorderStyle = this.FormBorderStyle == FormBorderStyle.None
            //    ? FormBorderStyle.Sizable : FormBorderStyle.None;
        }

        public void SetMdiChild(Form child)
        {
            if (ActiveMdiChild != null && ActiveMdiChild.GetType() == child.GetType())
            {
                return;
            }

            foreach (Form frm in MdiChildren)
            {
                frm.Close();
                frm.Dispose();
            }
            child.MdiParent = this;
            child.WindowState = FormWindowState.Maximized;
            child.Show();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
                e.Cancel = true;
        }


        public static List<TFaceRecord> LoadFaceList()
        {
            List<TFaceRecord> lsTFace = new List<TFaceRecord>();
            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {
                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + SQLServerAddress + "; initial catalog=" + DatabaseName + "; User Id=" + DBUid + "; Pwd=" + DBPwd);
                sqlConnect.Open();

                //=== Check Face Mask On =====
                System.Data.SqlClient.SqlCommand sqlCmd = null;
                if (Properties.Settings.Default.AllowFaceMask)
                {
                    sqlCmd = new System.Data.SqlClient.SqlCommand("SELECT [Code], [Fullname], [Group], [FacePositionXc], [FacePositionYc], [FacePositionW], [FacePositionAngle], [Eye1X], [Eye1Y], [Eye2X], [Eye2Y], [Template], [SimilarThreshold], [ID], [HasFaceMask] FROM FaceList", sqlConnect);
                }
                else
                {
                    sqlCmd = new System.Data.SqlClient.SqlCommand("SELECT [Code], [Fullname], [Group], [FacePositionXc], [FacePositionYc], [FacePositionW], [FacePositionAngle], [Eye1X], [Eye1Y], [Eye2X], [Eye2Y], [Template], [SimilarThreshold], [ID], [HasFaceMask] FROM FaceList WHERE [HasFaceMask]=0", sqlConnect);
                }

                if (Properties.Settings.Default.OnlyFaceMask)
                {
                    sqlCmd = new System.Data.SqlClient.SqlCommand("SELECT [Code], [Fullname], [Group], [FacePositionXc], [FacePositionYc], [FacePositionW], [FacePositionAngle], [Eye1X], [Eye1Y], [Eye2X], [Eye2Y], [Template], [SimilarThreshold], [ID], [HasFaceMask] FROM FaceList WHERE [HasFaceMask]=1", sqlConnect);
                }

                System.Data.SqlClient.SqlDataReader reader = sqlCmd.ExecuteReader();
                while (reader.Read())
                {
                    TFaceRecord frr = new TFaceRecord();
                    frr.Code = reader.GetString(0);
                    frr.Fullname = reader.GetString(1);
                    frr.Group = reader.GetString(2);

                    frr.FacePosition = new FSDK.TFacePosition();
                    frr.FacePosition.xc = reader.GetInt32(3);
                    frr.FacePosition.yc = reader.GetInt32(4);
                    frr.FacePosition.w = reader.GetInt32(5);
                    frr.FacePosition.angle = reader.GetFloat(6);

                    frr.FacialFeatures = new FSDK.TPoint[2];
                    frr.FacialFeatures[0] = new FSDK.TPoint();
                    frr.FacialFeatures[0].x = reader.GetInt32(7);
                    frr.FacialFeatures[0].y = reader.GetInt32(8);
                    frr.FacialFeatures[1] = new FSDK.TPoint();
                    frr.FacialFeatures[1].x = reader.GetInt32(9);
                    frr.FacialFeatures[1].y = reader.GetInt32(10);
                    frr.Template = new byte[FSDK.TemplateSize];
                    reader.GetBytes(11, 0, frr.Template, 0, FSDK.TemplateSize);
                    frr.SimilarThreshold = reader.GetFloat(12);
                    frr.ID = reader.GetInt32(13);
                    frr.HasFaceMask = reader.GetBoolean(14);

                    lsTFace.Add(frr);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Exception on loading database");
            }
            finally
            {
                if (sqlConnect != null)
                    sqlConnect.Close();
            }
            return lsTFace;
        }

        public static List<string> LoadFaceListTest()
        {
            List<string> lsFaceListTest = new List<string>();
            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {
                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + SQLServerAddress + "; initial catalog=" + DatabaseName + "; User Id=" + DBUid + "; Pwd=" + DBPwd);
                sqlConnect.Open();

                System.Data.SqlClient.SqlCommand sqlCmd = null;
                sqlCmd = new System.Data.SqlClient.SqlCommand("SELECT [Code] FROM [FaceTest]", sqlConnect);

                System.Data.SqlClient.SqlDataReader reader = sqlCmd.ExecuteReader();
                while (reader.Read())
                {
                    lsFaceListTest.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Exception on loading database");
            }
            finally
            {
                if (sqlConnect != null)
                    sqlConnect.Close();
            }
            return lsFaceListTest;
        }

        public static TFaceRecord GoSearchFace(TFaceRecord SearchFace, List<TFaceRecord> faceList)
        {
            float Threshold = Properties.Settings.Default.Threshold;
            float smtmp = 0;
            int ifctmp = 0;
            //Reset the most similar face
            TFaceRecord rsFace = new TFaceRecord();
            rsFace.Code = String.Empty;
            rsFace.Fullname = String.Empty;
            rsFace.Group = String.Empty;

            for (int i = 0; i < faceList.Count; i++)
            {
                float Similarity = 0.0f;
                TFaceRecord CurrentFace = faceList[i];
                FSDK.MatchFaces(ref SearchFace.Template, ref CurrentFace.Template, ref Similarity);

                if (Threshold != CurrentFace.SimilarThreshold)
                    Threshold = CurrentFace.SimilarThreshold;

                //if (Similarity > 0.95)
                //    if (faceList[i].Code == "0972" || faceList[i].Code == "0525" || faceList[i].Code == "0971" || faceList[i].Code == "0112")
                //    Console.WriteLine($"{faceList[i].Fullname} \t {Similarity} \t {faceList[i].HasFaceMask} \t {faceList[i].ID}");

                if (Similarity >= Threshold)
                {
                    //Compare to get the most similar face
                    if (Similarity > smtmp)
                    {
                        smtmp = Similarity;
                        ifctmp = i;
                    }

                    //Set value of the most similar face
                    rsFace.Code = faceList[ifctmp].Code;
                    rsFace.Fullname = faceList[ifctmp].Fullname;
                    rsFace.Group = faceList[ifctmp].Group;
                    rsFace.SimilarThreshold = smtmp;
                    rsFace.ID = faceList[ifctmp].ID;
                    rsFace.HasFaceMask = faceList[ifctmp].HasFaceMask;
                }
            }

            return rsFace;
        }

        public static DateTime GetDateTimeFromServer()
        {
            string server = Properties.Settings.Default.Server;
            string database = Properties.Settings.Default.Database;
            string user = Properties.Settings.Default.Username;
            string password = Encoding.UTF8.GetString(Convert.FromBase64String(Properties.Settings.Default.Password));
            DateTime rsDateTime = DateTime.Now;
            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {
                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + server + "; initial catalog=" + database + "; User Id=" + user + "; Pwd=" + password);
                sqlConnect.Open();

                System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand("SELECT GETDATE()", sqlConnect);

                System.Data.SqlClient.SqlDataReader reader = sqlCmd.ExecuteReader();
                while (reader.Read())
                {
                    rsDateTime = reader.GetDateTime(0);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Exception on loading database");
                SaveLogsToDB("Exception on loading database - Get DateTime From Server Fail", "error");
            }
            finally
            {
                if (sqlConnect != null)
                    sqlConnect.Close();
            }
            return rsDateTime;
        }

        public static int SaveLogsToDB(string message, string type)
        {
            int iresult = 0;
            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {

                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + frmMain.SQLServerAddress + "; initial catalog=" + frmMain.DatabaseName + "; User Id=" + frmMain.DBUid + "; Pwd=" + frmMain.DBPwd);
                sqlConnect.Open();
                System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand("INSERT INTO Logs([Message], [Type], [CreatedDate]) " + " values(@Message, @Type, GETDATE())", sqlConnect);

                sqlCmd.Parameters.Add("@Message", System.Data.SqlDbType.Text);
                sqlCmd.Parameters.Add("@Type", System.Data.SqlDbType.VarChar, 50);

                sqlCmd.Parameters["@Message"].Value = message;
                sqlCmd.Parameters["@Type"].Value = type;

                iresult = sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Exception on saving to database");
            }
            finally
            {
                if (sqlConnect != null)
                    sqlConnect.Close();
            }
            return iresult;
        }

    }
}
