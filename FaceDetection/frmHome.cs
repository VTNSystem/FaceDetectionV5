
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FaceDetection
{
    public partial class frmHome : Form
    {
        private static bool isAdminLogged = false;
        public bool endLoginTime = false;
        public static TimeSpan beginLoginTime = DateTime.Now.TimeOfDay;

        public frmHome()
        {
            InitializeComponent();
        }

        private void frmHome_Load(object sender, EventArgs e)
        {
            // Changes hover and pressed button background color
            foreach (Control control in flowLayoutMain.Controls)
            {
                Color backColor = Color.FromArgb(100, 0, 0, 0);
                var btn = control as Button;
                if (btn != null)
                {
                    btn.FlatAppearance.MouseDownBackColor = backColor;
                    btn.FlatAppearance.MouseOverBackColor = backColor;
                }
            }
            //AdminGrated();

            //===== Check Admin Login Timeout ====
            //double preciseDifference = (DateTime.Now.TimeOfDay - beginLoginTime).TotalMinutes;
            //int differentMinutes = (int)preciseDifference;
            //if (differentMinutes > 30)
            //{
            //    isAdminLogged = false;
            //    AdminGrated();
            //}
        }

        private void frmHome_Shown(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources.technology_bg;
        }

        private void AdminGrated()
        {
            btnSettings.Visible = btnAddUser.Visible = btnRollCall.Visible = btnExit.Visible = isAdminLogged;
            if (isAdminLogged)
                lblAdmin.Text = "Logout";
            else
                lblAdmin.Text = "Admin login";
        }

        private void OpenMdiForm(Form form)
        {
            var parent = this.MdiParent as frmMain;
            if (parent != null)
            {
                parent.SetMdiChild(form);
            }
            else
            {
                MessageBox.Show("MDI Parent is not found!", "404-Not Found", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private bool Authenticate()
        {
            bool result = false;
            using (frmAuthenticate frmAuthenticate = new frmAuthenticate())
            {
                frmAuthenticate.FormBorderStyle = FormBorderStyle.None;
                frmAuthenticate.StartPosition = FormStartPosition.CenterParent;
                frmAuthenticate.Opacity = 0.95;
                frmAuthenticate.Width = this.Width - 100;
                frmAuthenticate.Height = this.Height - 100;
                if (frmAuthenticate.ShowDialog() == DialogResult.OK)
                {
                    result = true;
                }

                frmAuthenticate.Dispose();
            }
            return result;
        }
        //bool door = true;
        private void btnRecognition_Click(object sender, EventArgs e)
        {
            /*EasyModbus.ModbusClient modbusClient = null;
            ////
            string ip = Properties.Settings.Default.IPAddress;
            int port = Properties.Settings.Default.Port;
            int timeout = Properties.Settings.Default.ModuleTimeout;
            // Connect to door module
            try
            {
                //if (modbusClient != null && modbusClient.Connected)
                //    modbusClient.Disconnect();

                modbusClient = new EasyModbus.ModbusClient(ip, port);    //Ip-Address and Port of Modbus-TCP-Server
                modbusClient.ConnectionTimeout = timeout;
                modbusClient.Connect();

                modbusClient.WriteMultipleCoils(29, new bool[] { door, door, door });
                door = !door;
            }
            catch (Exception ex) { 
                MessageBox.Show(ex.Message);
                if (ex.Message.Contains("read"))
                {

                }
            }*/
            //var data = modbusClient.ReadCoils(29, 3);
           //foreach(var d in data)
           // {
           //     Console.Write(d + ",");
           // }
           // Console.WriteLine();
            OpenMdiForm(new frmRecognition());
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            pbLoading.SizeMode = PictureBoxSizeMode.CenterImage;
            pbLoading.Image = Properties.Resources.loader;
            pbLoading.Refresh();

            OpenMdiForm(new frmSettings());

            pbLoading.Image = null;
            pbLoading.Refresh();
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            pbLoading.SizeMode = PictureBoxSizeMode.CenterImage;
            pbLoading.Image = Properties.Resources.loader;
            pbLoading.Refresh();

            OpenMdiForm(new frmInput());

            pbLoading.Image = null;
            pbLoading.Refresh();
        }

        private void btnRollCall_Click(object sender, EventArgs e)
        {
            pbLoading.SizeMode = PictureBoxSizeMode.CenterImage;
            pbLoading.Image = Properties.Resources.loader;
            pbLoading.Refresh();

            OpenMdiForm(new frmUser());

            pbLoading.Image = null;
            pbLoading.Refresh();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lblAdmin_Click(object sender, EventArgs e)
        {
            isAdminLogged = isAdminLogged ? false : Authenticate();
            AdminGrated();
        }
    }
}
