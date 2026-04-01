using EasyModbus;
using Luxand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FaceDetection.DCAM710Handler;

namespace FaceDetection
{
    public partial class frmRecognizeTask : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int IN_MODE = 1;
        private const int OUT_MODE = 2;

        // percent of smile will be passed
        int smilePassed;

        bool useEmoji = false;
        bool needClose = false;
        int inOutMode;

        int cameraHandle = 0;
        String cameraName;
        String TrackerMemoryFile = "tracker70.dat";
        Stopwatch timer = new Stopwatch();
        // the timer calculate duration how long is a person detecting
        Stopwatch durationTimer = new Stopwatch();
        string lastDetected;
        Properties.Settings mSettings;

        System.Timers.Timer doorTimer;
        System.Media.SoundPlayer doorSound = new System.Media.SoundPlayer("Resources/access-door.wav");

        // prevent insert multiple times at the same time
        List<KeyValuePair<String, DateTime>> insertedList = new List<KeyValuePair<string, DateTime>>();
        List<Models.User> mUsers;
        string mPermission;
        bool isReported = false;
        string reportPath = null;
        object mFileLocker = new object();

        ModbusClient modbusClient;

        TFaceRecord fr = new TFaceRecord();
        public List<TFaceRecord> FaceList = new List<TFaceRecord>();
        public DateTime currenttime = DateTime.Now;
        public List<TFaceRecord> FaceListTest = new List<TFaceRecord>();
        List<string> userListTest = new List<string>();

        bool doubleCheck = false;
        string doubleCheckValue = "";
        bool allowHandleDetection = false;

        public bool openDoorTime = false;

        public bool IsShown { get; set; } = false;

        // WinAPI procedure to release HBITMAP handles returned by FSDKCam.GrabFrame
        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        //=== DCAM 710 ===
        public PsReturnStatus psReturnstatus;
        public PsDeviceHandle deviceHandle = new PsDeviceHandle();
        public IntPtr pdevicde;
        public UInt32 sessionIndex = 0;

        public frmRecognizeTask()
        {
            InitializeComponent();
            mSettings = Properties.Settings.Default;
            doorTimer = new System.Timers.Timer(mSettings.DoorTimeout);
            doorTimer.Elapsed += (o, e) =>
            {
                CloseDoor();
                doorTimer.Stop();
            };
            
            Config();

            // Add camera's images as a top layer of background image
            pbBackground.Controls.Add(pbCamera);
            // Set gif image as background
            pbBackground.BackColor = Color.Black;
            pbBackground.SizeMode = PictureBoxSizeMode.Zoom;
            pbBackground.Image = Properties.Resources.spartronics_welcome_bg;

            txtBarcodeScanner.Visible = false;
        }

        private void frmRecognizeTask_Load(object sender, EventArgs e)
        {
            //===============
            //this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

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
            if (0 == count && !mSettings.UseCameraIP)
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

            mUsers = DB_TimeLog.GetUsers();
            FaceList = frmMain.LoadFaceList();

            //=== FaceList Test ===
            userListTest = frmMain.LoadFaceListTest();
            if (userListTest.Count > 0)
                FaceList = FaceList.Where(x => userListTest.Contains(x.Code) == false || (userListTest.Contains(x.Code) && x.HasFaceMask == false)).ToList();
            //=== End FaceList Test ===


            txtBarcodeScanner.Top = -10000;
            txtBarcodeScanner.Left = -10000;
            txtBarcodeScanner.Width = 10;
            txtBarcodeScanner.Height = 10;
            txtBarcodeScanner.Focus();

            //=== Check Face Mask ====
            lblSmile.Visible = !mSettings.AllowFaceMask;
            lblSmileTitle.Visible = !mSettings.AllowFaceMask;
        }

        private void frmRecognizeTask_Shown(object sender, EventArgs e)
        {
            IsShown = true;
            tableLayoutTitle.BackgroundImage = Properties.Resources.hexagon_bg;
            tableLayoutInfo.BackColor = Color.LightCoral;

            // get config face data path
            string dataPath = Properties.Settings.Default.FaceDataPath;

            // path used to store data that user is not recognized
            reportPath = Path.Combine(FaceDataHandler.GetReportPath(), DateTime.Now.ToString("yyyyMMdd"));

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (_, __) =>
            {
                int result = FSDK.FSDKE_FAILED;

                //===FaceMask-On from SDK===
                /*int err = 0;
                FSDK.SetFaceDetectionParameters(true, true, 512);
                bool allowFaceMask = mSettings.AllowFaceMask;
                if (FSDK.FSDKE_OK != FSDK.SetParameters("FaceDetectionModel=fd_masks1.bin;TrimFacesWithUncertainFacialFeatures=" + (!allowFaceMask).ToString().ToLower(), ref err))
                {
                    MessageBox.Show("Error loading face detection model!");
                }*/

                int err = 0;
                FSDK.SetFaceDetectionThreshold(3);
                FSDK.SetFaceDetectionParameters(true, false, 350);
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
                            psReturnstatus = Ps2_SetRGBResolution(pdevicde, sessionIndex, PsResolution.PsRGB_Resolution_1280_720);

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
                    if (mSettings.UseCameraIP)
                    {
                        string url = mSettings.CameraIP_URL;
                        string user = mSettings.CameraIP_Username;
                        string password = mSettings.CameraIP_Password;
                        int timeout = mSettings.CameraIP_Timeout;
                        result = FSDKCam.OpenIPVideoCamera(FSDKCam.FSDK_VIDEOCOMPRESSIONTYPE.FSDK_MJPEG, url, user, password, timeout, ref cameraHandle);
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
            worker.RunWorkerCompleted += (_, __) =>
            {
                bool? result = __.Result as bool?;
                if (result.HasValue && result.Value == false)
                {
                    MessageBox.Show("Error opening the first camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
                else
                {
                    RunSingle();
                }
            };

            pbReport.Enabled = false;
            worker.RunWorkerAsync();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCamera();
            IsShown = false;
        }

        private void CloseCamera()
        {
            needClose = true;
        }

        private void CreateModbus()
        {
            string ip = Properties.Settings.Default.IPAddress;
            int port = Properties.Settings.Default.Port;
            int timeout = Properties.Settings.Default.ModuleTimeout;
            // Connect to door module
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                    modbusClient.Disconnect();

                modbusClient = new ModbusClient(ip, port);    //Ip-Address and Port of Modbus-TCP-Server
                modbusClient.ConnectionTimeout = timeout;
                modbusClient.Connect();
            }
            catch { }
        }

        private bool OpenDoor()
        {
            //===================
            //return true;

            try
            {
                /*if (!modbusClient.Connected)
                {
                    int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Modbus disconnected!", "Info");
                    if (res == 0)
                    {
                        log.Info("Modbus disconnected!");
                    }
                    modbusClient.Connect();
                }*/
                modbusClient.Connect();
                modbusClient.WriteMultipleCoils(29, new bool[] { true, true, true });
                modbusClient.Disconnect();

                // close door interval
                doorTimer.Start();
                return true;
            }
            catch (Exception ex)
            {
                int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Modbus error!", "Info");
                int res1 = frmMain.SaveLogsToDB($"[{ Environment.MachineName}] {ex.Message}", "Error");
                if (res + res1 == 0)
                {
                    log.Info("Modbus error!");
                    log.Error(ex.Message);
                }
                // Create new instance when module cannot be written
                CreateModbus();
            }

            return false;
        }

        private bool CloseDoor()
        {
            // Config Open Door Time
            if (openDoorTime)
                return true;

            try
            {
                modbusClient.Connect();
                modbusClient.WriteMultipleCoils(29, new bool[] { false, false, false });
                modbusClient.Disconnect();

                return true;
            }
            catch { }

            return false;
        }

        private void RunSingle()
        {
            timer.Start();
            durationTimer.Start();
            pbReport.Enabled = true;
            short errorInterval = 0;


            /*int tracker = 0;    // creating a Tracker
            if (FSDK.FSDKE_OK != FSDK.LoadTrackerMemoryFromFile(ref tracker, TrackerMemoryFile)) // try to load saved tracker state
                FSDK.CreateTracker(ref tracker); // if could not be loaded, create a new tracker

            int err = 0; // set realtime face detection parameters
            FSDK.SetTrackerMultipleParameters(tracker, "HandleArbitraryRotations=false; DetermineFaceRotationAngle=false; FaceDetectionThreshold=5; RecognizeFaces=true;", ref err);*/


            while (!needClose)
            {
                try
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
                        //image = new FSDK.CImage(@"C:\Users\phuong.docongdong\Desktop\LiveRecognition Test\Lookup\qqq.jpg");
                        image = new FSDK.CImage(imageHandle);
                        frameImage = image.ToCLRImage();
                    }


                    /*long[] IDs;
                    long faceCount = 0;
                    FSDK.FeedFrame(tracker, 0, image.ImageHandle, ref faceCount, out IDs, sizeof(long) * 256); // maximum of 256 faces detected
                    Array.Resize(ref IDs, (int)faceCount);
                    if (faceCount > 0)
                        MessageBox.Show(faceCount.ToString());*/


                    Graphics gr = Graphics.FromImage(frameImage);
                    string detectedName = String.Empty;
                    int detectedSmile = 0, detectedEye = 0;
                    float distance = 0;
                    int left = 0, top = 0, w = 0, h = 0;

                    //New detection
                    fr.FacePosition = new FSDK.TFacePosition();
                    fr.FacialFeatures = new FSDK.TPoint[2];
                    fr.Template = new byte[FSDK.TemplateSize];
                    fr.image = image;
                    fr.FacePosition = fr.image.DetectFace();
                    fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition.xc - Math.Round(fr.FacePosition.w * 1.0)), (int)(fr.FacePosition.yc - Math.Round(fr.FacePosition.w * 1.0)), (int)(fr.FacePosition.xc + Math.Round(fr.FacePosition.w * 1.0)), (int)(fr.FacePosition.yc + Math.Round(fr.FacePosition.w * 1.0)));

                    fr.FacialFeatures = fr.image.DetectFacialFeaturesInRegion(ref fr.FacePosition);
                    fr.Template = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition); // get template with higher precision

                    // Detect expression
                    String AttributeValues = "";
                    int smilePercent = 0, eyePercent = 0;

                    //=== Check Face Mask ====
                    if (!mSettings.AllowFaceMask)
                    {
                        if (FSDK.FSDKE_OK == FSDK.DetectFacialAttributeUsingFeatures(imageHandle, ref fr.FacialFeatures, "Expression", out AttributeValues, 1024))
                        {
                            float ConfidenceSmile = 0.0f;
                            float ConfidenceEyesOpen = 0.0f;
                            FSDK.GetValueConfidence(AttributeValues, "Smile", ref ConfidenceSmile);
                            FSDK.GetValueConfidence(AttributeValues, "EyesOpen", ref ConfidenceEyesOpen);
                            smilePercent = (int)(ConfidenceSmile * 100);
                            eyePercent = (int)(ConfidenceEyesOpen * 100);

                            //Console.WriteLine($"Smile Percent: {smilePercent}, Eye Percent: {eyePercent}");
                            //Console.WriteLine($"ImageHandle : {imageHandle}");
                        }
                    }
                    else
                    {
                        smilePercent = 100;
                        eyePercent = 100;
                    }

                    // adding face background
                    if (FaceList.Count == 0)
                    {
                        pbCamera.Image = null;
                        //FaceList = frmMain.LoadFaceList();
                    }

                    TFaceRecord nface = new TFaceRecord();
                    string messageSimilarThreshold = "";
                    // find the nearest detected person
                    if (distance < fr.FacePosition.w)
                    {
                        // face position
                        left = fr.FacePosition.xc - (int)(fr.FacePosition.w * 1);
                        top = fr.FacePosition.yc - (int)(fr.FacePosition.w * 0.8);
                        w = (int)(fr.FacePosition.w * 2);
                        h = (int)(fr.FacePosition.w * 2.5);
                    }

                    //set nearest detected face on picture box
                    if (w > 250 || h > 250)
                    {
                        //Get the most similar face
                        nface = frmMain.GoSearchFace(fr, FaceList);
                        messageSimilarThreshold = nface.SimilarThreshold.ToString();

                        if (nface.Code.Length > 0)
                        {
                            //=========
                            //Double check before go to detect
                            /*if (!doubleCheck)
                            {
                                doubleCheck = true;
                                doubleCheckValue = nface.Code;
                            }
                            else
                            {
                                if (nface.Code == doubleCheckValue)
                                {
                                    doubleCheck = false;
                                    allowHandleDetection = true;
                                }
                                else
                                {
                                    doubleCheckValue = nface.Code;
                                }
                            }*/

                            // draw name
                            string drawText = nface.Code + " - " + nface.Fullname;
                            if (drawText.Length <= 0)
                                drawText = "Unknown";

                            StringFormat format = new StringFormat();
                            format.Alignment = StringAlignment.Center;
                            gr.DrawString(drawText, new System.Drawing.Font("Times New Roman", 18), new System.Drawing.SolidBrush(System.Drawing.Color.LightSeaGreen), fr.FacePosition.xc, top + w + 5, format);
                        }

                        // face data
                        detectedName = nface.Code;
                        detectedSmile = smilePercent;
                        detectedEye = eyePercent;
                        distance = fr.FacePosition.w;

                        //set nearest detected face on picture box
                        Image faceImage = image.CopyRectReplicateBorder(Math.Max(left, 0), Math.Max(top, 0), Math.Min(left + w, image.Width), Math.Min(top + h, image.Height)).Resize(1.2).ToCLRImage();
                        Graphics faceGraphic = Graphics.FromImage(faceImage);
                        Color penColor = Color.Red;
                        StringFormat format1 = new StringFormat();
                        format1.Alignment = StringAlignment.Center;

                        // set text color based on smile rate
                        if (detectedSmile > smilePassed / 2 && detectedSmile < smilePassed)
                            penColor = Color.Yellow;
                        else if (detectedSmile >= smilePassed)
                            penColor = Color.Green;

                        //=== Check Face Mask ====
                        if (!mSettings.AllowFaceMask)
                        {
                            faceGraphic.DrawString($"Smile: {detectedSmile}%", new System.Drawing.Font("Times New Roman", 30, FontStyle.Bold), new System.Drawing.SolidBrush(penColor), faceImage.Width / 2, faceImage.Height * 0.8f, format1);
                        }

                        pbCamera.Image = faceImage;
                    }
                    else
                    {
                        pbCamera.Image = null;
                    }


                    // display emoji
                    Image faceIcon = null;
                    if (useEmoji)
                    {
                        try
                        {
                            if (detectedSmile < smilePassed / 4)
                            {
                                faceIcon = Image.FromFile("Resources/sad-emoji.png");
                            }
                            else if (detectedSmile < smilePassed / 2)
                            {
                                faceIcon = Image.FromFile("Resources/normal-emoji.png");
                            }
                            else if (detectedSmile < smilePassed)
                            {
                                faceIcon = Image.FromFile("Resources/calm-emoji.png");
                            }
                            else
                            {
                                faceIcon = Image.FromFile("Resources/smile-emoji.png");
                            }
                        }
                        catch { }

                        pbEmoji.Image = faceIcon;
                    }

                    //===Double check====
                    /*if (allowHandleDetection)
                    {
                        allowHandleDetection = false;

                        // handle detection data
                        bool success = HandleDetection(nface.Code ?? "", nface.Fullname ?? "", nface.Group ?? "", detectedSmile, detectedEye, messageSimilarThreshold, nface.ID, nface.HasFaceMask);
                        
                        // save evidence data
                        if (success)
                        {
                            string fileName = System.IO.Path.Combine($"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{detectedName.Split('-')[0].Trim()}_{nface.Fullname.Replace(" ", "_")}_{detectedSmile}.jpg");
                            //SaveEvidence(image, fileName);
                            SaveEvidence(fr.image, fileName);
                        }
                    }*/

                    // handle detection data
                    bool success = HandleDetection(nface.Code ?? "", nface.Fullname ?? "", nface.Group ?? "", detectedSmile, detectedEye, messageSimilarThreshold, nface.ID, nface.HasFaceMask);

                    // save evidence data
                    if (success)
                    {
                        string fileName = System.IO.Path.Combine($"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{detectedName.Split('-')[0].Trim()}_{nface.Fullname.Replace(" ", "_")}_{detectedSmile}.jpg");
                        //SaveEvidence(image, fileName);
                        SaveEvidence(fr.image, fileName);
                    }

                    // clear if not any detected within 1 second
                    if (!timerBlockUI.Enabled && timer.ElapsedMilliseconds > 1000)
                    {
                        lblWelcome.Text = "Welcome to Spartronics Vietnam";
                        tableLayoutInfo.BackColor = Color.DarkRed;
                        SetMessage(lblCode, "Unknown", Color.Black);
                        SetMessage(lblName, "Unknown", Color.Black);
                        lblSmile.Text = "Unknown";
                        pbEmoji.Image = null;

                        if (mSettings.TurnOffCamera)
                        {
                            // turn off after 23 p.m
                            DateTime now = DateTime.Now;
                            if ((now.Hour >= 23 || now.Hour < 5) && now.Second == 0)
                            {
                                // automatically turn off camera if nobody detected within 120 seconds
                                if (timer.ElapsedMilliseconds > 120 * 1000)
                                {
                                    CloseCamera();
                                }
                            }
                        }
                    }

                    if (isReported)
                    {
                        string errorPath = Path.Combine(reportPath, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg");
                        isReported = false;
                        //image.Save(errorPath);
                        fr.image.Save(errorPath);
                        MessageBox.Show("Phản hồi thành công và đang chờ xử lý!", "Success");
                    }

                    GC.Collect(); // collect the garbage after the deletion

                    // make UI controls accessible (to find if the user clicked on a face)
                    Application.DoEvents();
                }
                catch
                {
                    errorInterval++;

                    if (errorInterval > 2)
                    {
                        errorInterval = 0;
                        DialogResult continueResult = MessageBox.Show("Lỗi camera! Bạn có muốn thử lại?", "Camera", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (continueResult != DialogResult.Yes)
                        {
                            CloseCamera();
                        }
                    }
                }
            }

            pbCamera.SizeMode = PictureBoxSizeMode.CenterImage;
            pbCamera.Image = Properties.Resources.loader;
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

            this.Close();
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

        private bool HandleDetection(string code, string fullname, string group, int smile, int eye, string similarity, int id, bool hasfacemask)
        {
            bool success = false;

            if (!timerBlockUI.Enabled && code.Length >= 1 && fullname.Length >= 1 && group.Length >= 1)
            {
                string smilePercent = smile.ToString() + "%";
                string eyePercent = eye.ToString() + "%";
                Models.User user = mUsers.FirstOrDefault(u => u.Username == code);
                bool active = user != null ? user.Active : true;

                // Display user information
                tableLayoutInfo.BackColor = Color.LightCoral;
                SetMessage(lblCode, code, Color.Black);
                SetMessage(lblName, fullname, Color.Black);
                lblSmile.Text = smilePercent;

                // Check for user's permission to access the door
                if (String.IsNullOrEmpty(group) || !mPermission.Contains(group))
                {
                    lblWelcome.Text = $"Access denied for {group}!";
                }
                else
                {
                    string welcomeText = "Smile Again!" + Environment.NewLine + "Chưa được, cười lại nha!";
                    if (lblWelcome.Text != welcomeText)
                        lblWelcome.Text = welcomeText;

                    if (lastDetected != code)
                    {
                        durationTimer.Reset();
                        durationTimer.Start();
                    }

                    if (active && smile >= smilePassed)
                    {
                        tableLayoutInfo.BackColor = Color.LightGreen;
                        SetMessage(lblCode, code, Color.Black);
                        SetMessage(lblName, fullname, Color.Black);

                        lblSmile.Text = smilePercent;
                        bool allowInsert = false;
                        // restart open door timeout
                        if (!doorTimer.Enabled)
                        {
                            //PlaySound();
                            if (!OpenDoor())
                            {
                                // try to open door again
                                OpenDoor();
                            }
                        }
                        PlaySound();

                        string resultText;
                        // Welcome text for In Mode
                        if (inOutMode == OUT_MODE)
                        {
                            resultText = "\u263A See you again! \u263A";
                            resultText += Environment.NewLine + "Hẹn gặp lại!";
                        }
                        else
                        {
                            resultText = "\u263A Have a nice day! \u263A";
                            resultText += Environment.NewLine + "Xin mời bạn vào!";
                        }
                        if (resultText != lblWelcome.Text)
                            lblWelcome.Text = resultText;

                        timerBlockUI.Start();

                        // find the inserted code position in list
                        int index = insertedList.FindIndex(d => d.Key == code);

                        // insert code and timestamp to list if not exists
                        if (index < 0)
                        {
                            insertedList.Add(new KeyValuePair<string, DateTime>(code, DateTime.Now));
                            allowInsert = true;
                        }
                        else
                        {
                            int durationSeconds = (int)(DateTime.Now - insertedList[index].Value).TotalSeconds;
                            // prevent inserting duplicated data within 300 seconds
                            if (durationSeconds > 300)
                            {
                                insertedList[index] = new KeyValuePair<string, DateTime>(code, DateTime.Now);
                                allowInsert = true;
                            }
                        }

                        // Only office and operator user can insert into database
                        allowInsert = allowInsert && (group == Models.UserGroup.OFFICE || group == Models.UserGroup.OPERATOR);

                        if (allowInsert)
                        {
                            // insert data asynchronously
                            Task.Factory.StartNew(() =>
                            {
                                DateTime now = DateTime.Now;
                                try
                                {
                                    if (!DB_TimeLog.InsertData(code, now, inOutMode))
                                        throw new Exception("Inserted failed!");
                                    
                                    int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Inserted data for {fullname} (Code: {code}, Date: {now}, inout: {inOutMode}) [Similarity: {similarity}] [FaceID: {id}] [HasFaceMask: {hasfacemask}]", "Info");
                                    if (res == 0)
                                    {
                                        log.Info($"[{Environment.MachineName}] Inserted data for {fullname} (Code: {code}, Date: {now}, inout: {inOutMode}) [Similarity: {similarity}] [FaceID: {id}] [HasFaceMask: {hasfacemask}]");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Insert failed for {fullname} (code: {code}, date: {now}, inout: {inOutMode}) - {ex.Message}", "Error");
                                    if (res == 0)
                                    {
                                        log.Error($"[{Environment.MachineName}] Insert failed for {fullname} (code: {code}, date: {now}, inout: {inOutMode}) - {ex.Message}");
                                    }
                                    // write failed data to file
                                    WriteToFile(FaceDataHandler.FAILED_DATA_PATH, $"{code};{now};{inOutMode}");
                                }
                            });
                        }

                        success = true;
                    }
                }

                lastDetected = code;
                timer.Restart();
            }

            return success;
        }

        private bool HandleDetectionFromBarcode(string code)
        {
            bool success = false;

            if (!timerBlockUI.Enabled && code.Length >= 1)
            {
                //string smilePercent = smile.ToString() + "%";
                //string eyePercent = eye.ToString() + "%";
                Models.User user = mUsers.FirstOrDefault(u => u.Username == code);
                bool active = user != null ? user.Active : true;

                // Display user information
                tableLayoutInfo.BackColor = Color.LightCoral;
                SetMessage(lblCode, code, Color.Black);
                SetMessage(lblName, user.NickName, Color.Black);
                //lblSmile.Text = smilePercent;

                TFaceRecord fuserent = getListFaceByUsername(code).FirstOrDefault();
                string group = fuserent.Group;

                // Check for user's permission to access the door
                if (String.IsNullOrEmpty(group) || !mPermission.Contains(group))
                {
                    lblWelcome.Text = $"Access denied for {group}!";
                }
                else
                {
                    /*string welcomeText = "Smile Again!" + Environment.NewLine + "Chưa được, cười lại nha!";
                    if (lblWelcome.Text != welcomeText)
                        lblWelcome.Text = welcomeText;*/

                    if (lastDetected != code)
                    {
                        durationTimer.Reset();
                        durationTimer.Start();
                    }

                    if (active)
                    {
                        tableLayoutInfo.BackColor = Color.LightGreen;
                        SetMessage(lblCode, code, Color.Black);
                        SetMessage(lblName, user.NickName, Color.Black);

                        //lblSmile.Text = smilePercent;
                        bool allowInsert = false;
                        // restart open door timeout
                        if (!doorTimer.Enabled)
                        {
                            //PlaySound();
                            if (!OpenDoor())
                            {
                                // try to open door again
                                OpenDoor();
                            }
                        }
                        PlaySound();

                        string resultText;
                        // Welcome text for In Mode
                        if (inOutMode == OUT_MODE)
                        {
                            resultText = "\u263A See you again! \u263A";
                            resultText += Environment.NewLine + "Hẹn gặp lại!";
                        }
                        else
                        {
                            resultText = "\u263A Have a nice day! \u263A";
                            resultText += Environment.NewLine + "Xin mời bạn vào!";
                        }
                        if (resultText != lblWelcome.Text)
                            lblWelcome.Text = resultText;

                        timerBlockUI.Start();

                        // find the inserted code position in list
                        int index = insertedList.FindIndex(d => d.Key == code);

                        // insert code and timestamp to list if not exists
                        if (index < 0)
                        {
                            insertedList.Add(new KeyValuePair<string, DateTime>(code, DateTime.Now));
                            allowInsert = true;
                        }
                        else
                        {
                            int durationSeconds = (int)(DateTime.Now - insertedList[index].Value).TotalSeconds;
                            // prevent inserting duplicated data within 300 seconds
                            if (durationSeconds > 300)
                            {
                                insertedList[index] = new KeyValuePair<string, DateTime>(code, DateTime.Now);
                                allowInsert = true;
                            }
                        }

                        // Only office and operator user can insert into database
                        allowInsert = allowInsert && (group == Models.UserGroup.OFFICE || group == Models.UserGroup.OPERATOR);

                        if (allowInsert)
                        {
                            // insert data asynchronously
                            Task.Factory.StartNew(() =>
                            {
                                DateTime now = DateTime.Now;
                                try
                                {
                                    if (!DB_TimeLog.InsertData(code, now, inOutMode))
                                        throw new Exception("Inserted failed!");

                                    int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Inserted data for {user.NickName} (code: {code}, date: {now}, inout: {inOutMode})", "Info");
                                    if (res == 0)
                                    {
                                        log.Info($"[{Environment.MachineName}] Inserted data for {user.NickName} (code: {code}, date: {now}, inout: {inOutMode})");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Insert failed for {user.NickName} (code: {code}, date: {now}, inout: {inOutMode}) - {ex.Message}", "Error");
                                    if (res == 0)
                                    {
                                        log.Error($"[{Environment.MachineName}] Insert failed for {user.NickName} (code: {code}, date: {now}, inout: {inOutMode}) - {ex.Message}");
                                    }
                                    // write failed data to file
                                    WriteToFile(FaceDataHandler.FAILED_DATA_PATH, $"{code};{now};{inOutMode}");
                                }
                            });
                        }

                        success = true;
                    }
                }

                txtBarcodeScanner.Text = "";
                lastDetected = code;
                timer.Restart();
            }

            return success;
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

        private void SetMessage(Label label, string text, Color color)
        {
            label.Text = text;
            label.ForeColor = color;
        }

        private void PlaySound()
        {
            try
            {
                if (File.Exists(doorSound.SoundLocation))
                {
                    doorSound.Play();
                    return;
                }
            }
            catch { }
            Console.Beep(2000, 200);
        }

        private void CreateHiddenDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                DirectoryInfo dirInfo = Directory.CreateDirectory(dir);
                dirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }

        private void SaveEvidence(FSDK.CImage image, string fileName)
        {
            try
            {
                DateTime now = DateTime.Now;
                string[] hiddenDirs = new string[] { ".appdata", "evidence", now.Year.ToString(), now.Month.ToString(), now.Day.ToString() };
                string dir = "";
                foreach (string hidden in hiddenDirs)
                {
                    dir = System.IO.Path.Combine(dir, hidden);
                    CreateHiddenDirectory(dir);
                }
                string filePath = System.IO.Path.Combine(dir, fileName);
                image.Save(filePath);
            }
            catch (Exception ex)
            {
                int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] {ex.ToString()}", "Error");
                if (res == 0)
                {
                    log.Error(ex);
                }
            }
        }

        private void WriteToFile(string filePath, string text)
        {
            try
            {
                lock (mFileLocker)
                {
                    using (StreamWriter file = new StreamWriter(filePath, true))
                    {
                        file.WriteLine(text);
                    }
                }
            }
            catch (Exception ex)
            {
                int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Failed write to file ({text}) - {ex.Message}", "Error");
                if (res == 0)
                {
                    log.Error($"Failed write to file ({text}) - {ex.Message}");
                }
            }
        }

        private void Config()
        {
            int doorTimeout = Properties.Settings.Default.DoorTimeout; // in seconds

            smilePassed = mSettings.SmilePassed;
            doorTimer.Interval = doorTimeout * 1000;
            useEmoji = Properties.Settings.Default.UseEmoji;
            // In mode is false, Out mode is true
            inOutMode = Properties.Settings.Default.InOutMode ? OUT_MODE : IN_MODE;
            mPermission = Properties.Settings.Default.Permission;

            // Connect to SQL
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (_, __) =>
            {
                CreateModbus();
            };
            worker.RunWorkerCompleted += (_, __) =>
            {
                worker.Dispose();
            };
            worker.RunWorkerAsync();
        }

        private void pbReport_Click(object sender, EventArgs e)
        {
            if (reportPath != null)
            {
                // create path if not exists
                if (!Directory.Exists(reportPath))
                {
                    try
                    {
                        Directory.CreateDirectory(reportPath);
                    }
                    catch
                    {
                        reportPath = null;
                        return;
                    }
                }

                DialogResult result = MessageBox.Show("Không thể nhận dạng dữ liệu hoặc nhận dạng không đúng?", "Report", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    isReported = true;
                }
            }
            else
            {
                MessageBox.Show("Không thể kết nối đến máy chủ!", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pbClose_Click(object sender, EventArgs e)
        {
            CloseCamera();
        }

        private void timerBlockUI_Tick(object sender, EventArgs e)
        {
            timerBlockUI.Stop();
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            lblClock.Text = $"{DateTime.Now:HH:mm:ss}";
            lblClock.Refresh();

            // Sync failed data every hours
            if (DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                List<string> syncData = FailedDataHandler.Read();
                if(syncData.Count > 0)
                    FailedDataHandler.Sync();
            }
        }

        private void timerDoor_Tick(object sender, EventArgs e)
        {
            txtBarcodeScanner.Focus();

            if (!mSettings.AlwayCloseDoor && mSettings.OptionalOpenDoor)
            {
                string ftime1 = mSettings.FromTime1;
                string ttime1 = mSettings.ToTime1;
                DateTime fresult1 = Convert.ToDateTime(ftime1);
                DateTime tresult1 = Convert.ToDateTime(ttime1);
                string ftime2 = mSettings.FromTime2;
                string ttime2 = mSettings.ToTime2;
                DateTime fresult2 = Convert.ToDateTime(ftime2);
                DateTime tresult2 = Convert.ToDateTime(ttime2);
                string ftime3 = mSettings.FromTime3;
                string ttime3 = mSettings.ToTime3;
                DateTime fresult3 = Convert.ToDateTime(ftime3);
                DateTime tresult3 = Convert.ToDateTime(ttime3);
                string ftime4 = mSettings.FromTime4;
                string ttime4 = mSettings.ToTime4;
                DateTime fresult4 = Convert.ToDateTime(ftime4);
                DateTime tresult4 = Convert.ToDateTime(ttime4);

                TimeSpan ftm1 = fresult1.TimeOfDay;
                TimeSpan ttm1 = tresult1.TimeOfDay;
                TimeSpan ftm2 = fresult2.TimeOfDay;
                TimeSpan ttm2 = tresult2.TimeOfDay;
                TimeSpan ftm3 = fresult3.TimeOfDay;
                TimeSpan ttm3 = tresult3.TimeOfDay;
                TimeSpan ftm4 = fresult4.TimeOfDay;
                TimeSpan ttm4 = tresult4.TimeOfDay;

                TimeSpan ctm = DateTime.Now.TimeOfDay;
                bool inOfRange1 = false;
                bool inOfRange2 = false;
                bool inOfRange3 = false;
                bool inOfRange4 = false;

                if (mSettings.Range1 && !inOfRange2 && !inOfRange3 && !inOfRange4)
                {
                    if (ftm1 < ctm && ctm < ttm1)
                    {
                        if (inOfRange1 == false)
                            inOfRange1 = true;

                        if (openDoorTime == false)
                        {
                            openDoorTime = true;
                            OpenDoor();
                            //lblClock.Text = "Opening Door";
                            //lblClock.Refresh();
                        }
                    }
                    else
                    {
                        if (inOfRange1 == true)
                            inOfRange1 = false;

                        if (openDoorTime == true)
                        {
                            openDoorTime = false;
                            CloseDoor();
                            //lblClock.Text = "Closing Door";
                            //lblClock.Refresh();
                        }
                    }
                }
                
                if (mSettings.Range2 && !inOfRange1 && !inOfRange3 && !inOfRange4)
                {
                    if (ftm2 < ctm && ctm < ttm2)
                    {
                        if (inOfRange2 == false)
                            inOfRange2 = true;

                        if (openDoorTime == false)
                        {
                            openDoorTime = true;
                            OpenDoor();
                            //lblClock.Text = "Opening Door";
                            //lblClock.Refresh();
                        }
                    }
                    else
                    {
                        if (inOfRange2 == true)
                            inOfRange2 = false;

                        if (openDoorTime == true)
                        {
                            openDoorTime = false;
                            CloseDoor();
                            //lblClock.Text = "Closing Door";
                            //lblClock.Refresh();
                        }
                    }
                }

                if (mSettings.Range3 && !inOfRange1 && !inOfRange2 && !inOfRange4)
                {
                    if (ftm3 < ctm && ctm < ttm3)
                    {
                        if (inOfRange3 == false)
                            inOfRange3 = true;

                        if (openDoorTime == false)
                        {
                            openDoorTime = true;
                            OpenDoor();
                            //lblClock.Text = "Opening Door";
                            //lblClock.Refresh();
                        }
                    }
                    else
                    {
                        if (inOfRange3 == true)
                            inOfRange3 = false;

                        if (openDoorTime == true)
                        {
                            openDoorTime = false;
                            CloseDoor();
                            //lblClock.Text = "Closing Door";
                            //lblClock.Refresh();
                        }
                    }
                }
                
                if (mSettings.Range4 && !inOfRange3 && !inOfRange2 && !inOfRange1)
                {
                    if (ftm4 < ctm && ctm < ttm4)
                    {
                        if (inOfRange4 == false)
                            inOfRange4 = true;

                        if (openDoorTime == false)
                        {
                            openDoorTime = true;
                            OpenDoor();
                            //lblClock.Text = "Opening Door";
                            //lblClock.Refresh();
                        }
                    }
                    else
                    {
                        if (inOfRange4 == true)
                            inOfRange4 = false;

                        if (openDoorTime == true)
                        {
                            openDoorTime = false;
                            CloseDoor();
                            //lblClock.Text = "Closing Door";
                            //lblClock.Refresh();
                        }
                    }
                }
            }
        }

        private void txtBarcodeScanner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //MessageBox.Show("Enter Key Pressed " + txtBarcodeScanner.Text);
                //HandleDetectionFromBarcode(txtBarcodeScanner.Text);
            }
        }

        private bool checkDoorConnection(string IP, int Port, int TimeOut)
        {
            bool result = false;
            try
            {
                ModbusClient modbusClient = new ModbusClient(IP, Port);
                modbusClient.ConnectionTimeout = TimeOut;
                modbusClient.Connect();
                result = true;
            }
            catch (EasyModbus.Exceptions.ConnectionException ex)
            {
                result = false;
            }
            return result;
        }

    }
}