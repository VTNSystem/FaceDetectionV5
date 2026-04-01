using Luxand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static FaceDetection.DCAM710Handler;

namespace FaceDetection
{
    public partial class frmAuthenticate : Form
    {
        String cameraName;
        String TrackerMemoryFile;
        string authenticatedUser;
        bool needClose = false;
        int cameraHandle = 0;

        string adminList;
        TFaceRecord fr = new TFaceRecord();
        public List<TFaceRecord> FaceList = new List<TFaceRecord>();

        Properties.Settings mSettings;
        //=== DCAM 710 ===
        public PsReturnStatus psReturnstatus;
        public PsDeviceHandle deviceHandle = new PsDeviceHandle();
        public IntPtr pdevicde;
        public UInt32 sessionIndex = 0;

        public frmAuthenticate()
        {
            InitializeComponent();
            mSettings = Properties.Settings.Default;
        }

        private void frmAuthenticate_Load(object sender, EventArgs e)
        {
            pbCamera.SizeMode = PictureBoxSizeMode.CenterImage;
            pbCamera.Image = Properties.Resources.loader;
            pbCamera.Refresh();

            if (FSDK.FSDKE_OK != FSDK.ActivateLibrary(FaceDataHandler.LUXAND_LICENSE))
            {
                MessageBox.Show("Please run the License Key Wizard (Start - Luxand - FaceSDK - License Key Wizard)", "Error activating FaceSDK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            FSDK.InitializeLibrary();
            //FSDKCam.InitializeCapturing();

            string[] cameraList;
            int count;
            FSDKCam.GetCameraList(out cameraList, out count);
            if (0 == count && !Properties.Settings.Default.UseCameraIP)
            {
                MessageBox.Show("Please attach a camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            else if (0 < count)
            {
                int cameraIndex = Properties.Settings.Default.AttachedCamera;
                if (cameraIndex >= count)
                    cameraIndex = 0;
                cameraName = cameraList[cameraIndex];
                FSDKCam.VideoFormatInfo[] formatList;
                FSDKCam.GetVideoFormatList(ref cameraName, out formatList, out count);
                int videoFormat = Properties.Settings.Default.CameraVideoFormat;
                if (videoFormat >= count)
                    videoFormat = 0;
                FSDKCam.SetVideoFormat(ref cameraName, formatList[videoFormat]);
            }

            TrackerMemoryFile = FaceDataHandler.GetTempDataFilePath(Properties.Settings.Default.FaceDataPath);

            //Only get face list of Admin Users
            adminList = Properties.Settings.Default.AdminUsers;
            string[] arrUserAdmin = adminList.Split(';');
            string lsAdminUser = String.Empty;
            for (int i = 0; i < arrUserAdmin.Length; i++)
            {
                if (i == arrUserAdmin.Length - 1)
                {
                    lsAdminUser += "'" + arrUserAdmin[i] + "'";
                }
                else
                {
                    lsAdminUser += "'" + arrUserAdmin[i] + "',";
                }
            }
            FaceList = LoadAdminFaceList(lsAdminUser);
        }

        BackgroundWorker mWorker = new BackgroundWorker();

        private void frmAuthenticate_Shown(object sender, EventArgs e)
        {
            //tableLayoutPanel1.BackgroundImageLayout = ImageLayout.Stretch;
            tableLayoutPanel1.BackgroundImage = Properties.Resources.hexagon_bg;
            lblTitle.BackColor = panel1.BackColor = pbCamera.BackColor = Color.Transparent;
            adminList = Properties.Settings.Default.AdminUsers;

            SetMessage("Authenticated by face data OR login with username and password", Color.Orange);
            
            mWorker.DoWork += (_, __) =>
            {
                int result = FSDK.FSDKE_FAILED;

                //===FaceMask-On from SDK===
                int err = 0;
                FSDK.SetFaceDetectionThreshold(5);
                FSDK.SetFaceDetectionParameters(true, true, 512);
                if (FSDK.FSDKE_OK != FSDK.SetParameters("FaceDetectionModel=fd_masks1.bin;TrimFacesWithUncertainFacialFeatures=false", ref err))
                {
                    MessageBox.Show("Error loading face detection model!");
                }

                if (mSettings.UseDCAM710 || cameraName == "PicoZense RGBD Camera")
                {
                    //====== DCAM 710 ========
                    if (result != FSDK.FSDKE_OK)
                    {
                        PsCameraParameters cameraParameters = new PsCameraParameters();
                        try
                        {
                            UInt32 deviceCount = 0;
                            PsDeviceInfo pDevices = new PsDeviceInfo();
                            PsDeviceInfo[] pDeviceListInfo = new PsDeviceInfo[deviceCount];
                            psReturnstatus = Ps2_Initialize();
                            psReturnstatus = Ps2_GetDeviceCount(ref deviceCount);
                            //psReturnstatus = Ps2_GetDeviceListInfo(pDeviceListInfo, deviceCount);
                            psReturnstatus = Ps2_GetDeviceInfo(ref pDevices, 0);
                            deviceHandle = new PsDeviceHandle();
                            pdevicde = Marshal.AllocHGlobal(Marshal.SizeOf(deviceHandle));
                            psReturnstatus = Ps2_OpenDevice(pDevices.uri, ref pdevicde);
                            psReturnstatus = Ps2_StartStream(pdevicde, sessionIndex);
                            //Set PixelFormat as PsPixelFormatBGR888 for opencv display
                            //status = Ps2_SetColorPixelFormat(pdevicde, sessionIndex, PsPixelFormat.PsPixelFormatRGB888);
                            psReturnstatus = Ps2_GetCameraParameters(pdevicde, sessionIndex, PsSensorType.PsRgbSensor, ref cameraParameters);
                            PsCameraExtrinsicParameters CameraExtrinsicParameters = new PsCameraExtrinsicParameters();
                            psReturnstatus = Ps2_GetCameraExtrinsicParameters(pdevicde, sessionIndex, ref CameraExtrinsicParameters);
                            //PsDataMode dataMode = PsDataMode.PsDepthAndIR_30;
                            //psReturnstatus = Ps2_GetDataMode(pdevicde, sessionIndex, ref dataMode);
                            psReturnstatus = Ps2_SetDataMode(pdevicde, sessionIndex, PsDataMode.PsDepthAndRGB_30);
                            psReturnstatus = Ps2_SetRGBResolution(pdevicde, sessionIndex, PsResolution.PsRGB_Resolution_640_480);

                            if (psReturnstatus == PsReturnStatus.PsRetOK)
                            {
                                result = FSDK.FSDKE_OK;
                            }
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show("Error opening the DCAM710 Camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    if (Properties.Settings.Default.UseCameraIP)
                    {
                        string url = Properties.Settings.Default.CameraIP_URL;
                        string user = Properties.Settings.Default.CameraIP_Username;
                        string password = Properties.Settings.Default.CameraIP_Password;
                        int timeout = Properties.Settings.Default.CameraIP_Timeout;
                        result = FSDKCam.OpenIPVideoCamera(FSDKCam.FSDK_VIDEOCOMPRESSIONTYPE.FSDK_MJPEG,
                            url, user, password, timeout, ref cameraHandle);
                    }

                    if (result != FSDK.FSDKE_OK)
                    {
                        result = FSDKCam.OpenVideoCamera(ref cameraName, ref cameraHandle);
                    }
                }

                if (result != FSDK.FSDKE_OK)
                {
                    __.Result = false;
                }
            };
            mWorker.RunWorkerCompleted += (_, __) =>
            {
                bool? result = __.Result as bool?;
                // open camera failed
                if (result.HasValue && result.Value == false)
                {
                    this.Close();
                }
                else
                {
                    Run();
                }
            };
            mWorker.RunWorkerAsync();
        }

        private void frmAuthenticate_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCamera();
            
            // set the result for the form
            if (!String.IsNullOrEmpty(authenticatedUser))
                DialogResult = DialogResult.OK;
        }

        private void CloseCamera()
        {
            string message = "";
            if (!String.IsNullOrEmpty(authenticatedUser))
                message = authenticatedUser + " authenticated!!! ";
            message += "Closing camera...";
            SetMessage(message, Color.Orange);
            txtUsername.Enabled = txtPassword.Enabled = btnLogin.Enabled = btnCancel.Enabled = false;
            // set true to finish background worker
            needClose = true;
        }

        private void Run()
        {
            

            while (!needClose)
            {
                Int32 imageHandle = 0;
                Image frameImage = null;
                FSDK.CImage image = null;

                //=== DCAM 710 ===
                if (mSettings.UseDCAM710 || cameraName == "PicoZense RGBD Camera")
                {
                    PsFrameReady frameReady = new PsFrameReady();
                    psReturnstatus = Ps2_ReadNextFrame(pdevicde, sessionIndex, ref frameReady);
                    PsFrame rgbFrame = new PsFrame();
                    psReturnstatus = Ps2_GetFrame(pdevicde, sessionIndex, PsFrameType.PsRGBFrame, ref rgbFrame);
                    byte[] byteData = new byte[rgbFrame.dataLen];
                    Marshal.Copy(rgbFrame.pFrameData, byteData, 0, (int)rgbFrame.dataLen);
                    frameImage = ImageFromRawBgraArray(byteData, rgbFrame.width, rgbFrame.height);
                    image = new FSDK.CImage(frameImage);
                    //imageHandle = BitConverter.ToInt32(byteData, 0);
                }
                else
                {
                    if (FSDK.FSDKE_OK != FSDKCam.GrabFrame(cameraHandle, ref imageHandle)) // grab the current frame from the camera
                    {
                        Application.DoEvents();
                        continue;
                    }
                    image = new FSDK.CImage(imageHandle);
                    frameImage = image.ToCLRImage();
                }
                
                try
                {
                    //frameImage = image.ToCLRImage();
                    Graphics gr = Graphics.FromImage(frameImage);

                    //New detection
                    fr.FacePosition = new FSDK.TFacePosition();
                    fr.FacialFeatures = new FSDK.TPoint[2];
                    fr.Template = new byte[FSDK.TemplateSize];
                    fr.image = image;
                    fr.FacePosition = fr.image.DetectFace();

                    fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition.xc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.xc + Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc + Math.Round(fr.FacePosition.w * 0.5)));
                    fr.FacialFeatures = fr.image.DetectFacialFeaturesInRegion(ref fr.FacePosition);
                    fr.Template = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition); // get template with higher precision

                    int left = fr.FacePosition.xc - (int)(fr.FacePosition.w * 0.6);
                    int top = fr.FacePosition.yc - (int)(fr.FacePosition.w * 0.5);
                    int w = (int)(fr.FacePosition.w * 1.2);

                    // draw to detected face
                    Pen pen = new Pen(Color.Red);
                    TFaceRecord nface = frmMain.GoSearchFace(fr, FaceList);
                    if (nface.Code.Length > 0)
                    { // draw name
                        string drawText = nface.Code + " - " + nface.Fullname;
                        string code = drawText.Split('-')[0].Trim();
                        if (drawText.Length > 5 && adminList.Contains(code))
                        {
                            drawText = "AUTHENTICATED";
                            authenticatedUser = nface.Fullname;
                            pen = new Pen(Color.LightGreen);
                            DialogResult = DialogResult.OK;

                            frmHome.beginLoginTime = DateTime.Now.TimeOfDay;

                            this.Close();
                        }
                        else
                        {
                            drawText = "FAILED";
                        }

                        StringFormat format = new StringFormat();
                        format.Alignment = StringAlignment.Center;
                        gr.DrawString(drawText, new System.Drawing.Font("Times New Roman", 18),
                            new System.Drawing.SolidBrush(pen.Color),
                            fr.FacePosition.xc, top + w + 5, format);
                    }

                    // draw curved rectangle
                    // left top
                    gr.DrawLines(pen, new PointF[] { new PointF(left + w / 3, top), new PointF(left, top),
                        new PointF(left, top), new PointF(left, top + w / 3)
                    });
                    // left bottom
                    gr.DrawLines(pen, new PointF[] { new PointF(left, top + 2 * w / 3), new PointF(left, top + w),
                        new PointF(left, top + w), new PointF(left + w / 3, top + w)
                    });
                    // right bottom
                    gr.DrawLines(pen, new PointF[] { new PointF(left + 2 * w / 3, top + w), new PointF(left + w, top + w),
                        new PointF(left + w, top + w), new PointF(left + w, top + 2 * w / 3)
                    });
                    // right top
                    gr.DrawLines(pen, new PointF[] { new PointF(left + w, top + w / 3), new PointF(left + w, top),
                        new PointF(left + w, top), new PointF(left + 2 * w / 3, top)
                    });

                    // display current frame
                    pbCamera.Image = frameImage;

                    GC.Collect(); // collect the garbage after the deletion

                    // make UI controls accessible (to find if the user clicked on a face)
                    Application.DoEvents();
                }
                catch
                {
                    if (MessageBox.Show("Error", "Do you want to try again?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        break;
                }
            }

            pbCamera.Image = null;
            pbCamera.Refresh();

            if (mSettings.UseDCAM710 || cameraName == "PicoZense RGBD Camera")
            {
                //====== DCAM 710 ========
                psReturnstatus = Ps2_CloseDevice(ref pdevicde);
                psReturnstatus = Ps2_Shutdown();
            }
            else
            {
                FSDKCam.CloseVideoCamera(cameraHandle);
            }
        }

        public static Image ImageFromRawBgraArray(byte[] arr, int width, int height)
        {
            var output = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, output.PixelFormat);
            var ptr = bmpData.Scan0;
            Marshal.Copy(arr, 0, ptr, arr.Length);
            output.UnlockBits(bmpData);
            return output;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (username == "admin" && password == DateTime.Now.ToString("MMddyy"))
            {
                authenticatedUser = username;
                DialogResult = DialogResult.OK;

                frmHome.beginLoginTime = DateTime.Now.TimeOfDay;

                this.Close();
            }
            else
            {
                SetMessage("AUTHENTICATED FAILED!", Color.Red);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetMessage(string text, Color color)
        {
            lblMessage.Text = text;
            lblMessage.ForeColor = color;
            lblMessage.Refresh();
        }

        private List<TFaceRecord> LoadAdminFaceList(string listUserAdmin)
        {
            List<TFaceRecord> lsTFace = new List<TFaceRecord>();

            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {
                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + frmMain.SQLServerAddress + "; initial catalog=" + frmMain.DatabaseName + "; User Id=" + frmMain.DBUid + "; Pwd=" + frmMain.DBPwd);
                sqlConnect.Open();

                System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand("SELECT [Code], [Fullname], [Group], [FacePositionXc], [FacePositionYc], [FacePositionW], [FacePositionAngle], [Eye1X], [Eye1Y], [Eye2X], [Eye2Y], [Template], [SimilarThreshold] FROM FaceList WHERE Code IN (" + listUserAdmin + ")", sqlConnect);

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
    }
}