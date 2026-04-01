using EasyModbus;
using Luxand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceDetection
{
    public partial class frmRecognition : Form
    {
        List<string> mPhotoPaths = new List<string>();
        static int mPhotoIdx = 0;
        public DateTime currenttime = DateTime.Now;
        Properties.Settings mSettings;

        frmRecognizeTask frm;

        public frmRecognition()
        {
            InitializeComponent();
            mSettings = Properties.Settings.Default;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            string photoExt = ".jpg|.jpeg|.png|.bmp|.gif";
            string photoFolder = Properties.Settings.Default.PicturePath;

            if (Directory.Exists(photoFolder))
            {
                foreach (string file in Directory.GetFiles(photoFolder))
                {
                    if (photoExt.Contains(Path.GetExtension(file)))
                    {
                        mPhotoPaths.Add(file);
                    }
                }
                timerPhotoSlider_Tick(timerPhotoSlider, null);
            }

            RefreshRecognizeDialog();

            timerClock_Tick(timerClock, null);

            //currenttime = frmMain.GetDateTimeFromServer();
        }

        private void RefreshRecognizeDialog()
        {
            frm = new frmRecognizeTask();
            frm.WindowState = FormWindowState.Maximized;
            frm.FormBorderStyle = FormBorderStyle.None;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            tableLayoutPanel1.BackgroundImage = Properties.Resources.background_1;
            pbCamera.Image = Properties.Resources.camera_off_bg;
            splitLeft.Panel2.BackgroundImage = Properties.Resources.background;
            splitLeft.Panel2.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void pbPower_Click(object sender, EventArgs e)
        {
            if (!frm.IsShown)
            {
                frm.ShowDialog();
                RefreshRecognizeDialog();
            }
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            lblDateTime.Text = now.ToString("dddd, MMMM dd, yyyy HH:mm:ss");
            lblDateTime.Refresh();

            // automatically sync failed data every hour
            if (now.Minute == 21 && now.Second == 0)
            {
                FailedDataHandler.Sync();
            }

            // Turn Off Camera 11:00 PM - 05:00 AM everyday
            if (mSettings.TurnOffCamera)
            {
                //MessageBox.Show("TurnOffCamera");
                // automatically start recognition from 5 a.m to 23 p.m
                if ((now.Hour >= 5 && now.Hour < 23) && now.Second == 0)
                {
                    pbPower_Click(pbPower, null);
                }
            }
            else
            {
                if (now.Second == 0)
                {
                    pbPower_Click(pbPower, null);
                }
            }
            
        }

        private void timerPhotoSlider_Tick(object sender, EventArgs e)
        {
            if (mPhotoPaths.Count > 0)
            {
                if (mPhotoIdx >= mPhotoPaths.Count)
                {
                    mPhotoIdx = 0;
                }
                try
                {
                    pbWallpaper.Image = Image.FromFile(mPhotoPaths[mPhotoIdx]);
                    mPhotoIdx++;
                }
                catch { }
            }
        }

        private void FrmRecognition_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
