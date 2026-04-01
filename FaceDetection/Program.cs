using System;
using System.Windows.Forms;

namespace FaceDetection
{
    static class Program
    {
        static System.Threading.Mutex mutex;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isFirst = false;
            mutex = new System.Threading.Mutex(true, Application.ProductName, out isFirst);
            if (isFirst)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show($"{Application.ProductName} is already running!");
            }
        }
    }
}
