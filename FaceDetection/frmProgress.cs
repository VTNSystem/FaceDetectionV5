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
    public partial class frmProgress : Form
    {
        public frmProgress(string message, int timeout)
        {
            InitializeComponent();
            lblMessage.Text = message;
            timerCountdown.Interval = 5000;
        }

        private void frmProgress_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        private void timerCountdown_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmProgress_Load(object sender, EventArgs e)
        {
            timerCountdown.Start();
        }
    }
}
