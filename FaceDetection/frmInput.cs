using Luxand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static FaceDetection.DCAM710Handler;

namespace FaceDetection
{
    public partial class frmInput : Form
    {
        public List<TFaceRecord> FaceList = new List<TFaceRecord>();
        TFaceRecord fr = new TFaceRecord();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // program states: whether we recognize faces, or user has clicked a face
        enum ProgramState { psRemember, psRecognize }
        ProgramState programState = ProgramState.psRecognize;

        String cameraName;
        int mouseX = 0;
        int mouseY = 0;

        // whether is camera started
        bool isStarted = false;

        bool isRemove = false;

        Properties.Settings mSettings;
        //=== DCAM 710 ===
        public PsReturnStatus psReturnstatus;
        public PsDeviceHandle deviceHandle = new PsDeviceHandle();
        public IntPtr pdevicde;
        public UInt32 sessionIndex = 0;

        public frmInput()
        {
            InitializeComponent();
            mSettings = Properties.Settings.Default;
        }

        private void frmInput_Load(object sender, EventArgs e)
        {
            if (FSDK.FSDKE_OK != FSDK.ActivateLibrary(FaceDataHandler.LUXAND_LICENSE))
            {
                MessageBox.Show("Please run the License Key Wizard (Start - Luxand - FaceSDK - License Key Wizard)", "Error activating FaceSDK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            FSDK.InitializeLibrary();
            FSDKCam.InitializeCapturing();

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

            List<Models.User> users = DB_TimeLog.GetUsers();
            foreach (var user in users)
            {
                cbbUsers.Items.Add($"{user.Username} - {user.NickName}");
            }
            cbbGroup.SelectedIndex = 0;

            nudSimilarThreshold.Value = (decimal)Properties.Settings.Default.Threshold;
            nudSimilarThreshold.Minimum = (decimal)Properties.Settings.Default.Threshold;
            
            FaceList = frmMain.LoadFaceList();
        }

        private void frmInput_Shown(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources.technology_bg;
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void Run()
        {
            int cameraHandle = 0;
            int result = FSDK.FSDKE_FAILED;

            //===FaceMask-On from SDK===
            int err = 0;
            FSDK.SetFaceDetectionThreshold(5);
            FSDK.SetFaceDetectionParameters(true, true, 512);
            if (FSDK.FSDKE_OK != FSDK.SetParameters("TrimFacesWithUncertainFacialFeatures=false", ref err))
            {
                MessageBox.Show("Error loading face detection model!");
            }

            // Open video camera
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
                if (Properties.Settings.Default.UseCameraIP)
                {
                    string url = Properties.Settings.Default.CameraIP_URL;
                    string user = Properties.Settings.Default.CameraIP_Username;
                    string password = Properties.Settings.Default.CameraIP_Password;
                    int timeout = Properties.Settings.Default.CameraIP_Timeout;
                    result = FSDKCam.OpenIPVideoCamera(FSDKCam.FSDK_VIDEOCOMPRESSIONTYPE.FSDK_MJPEG, url, user, password, timeout, ref cameraHandle);
                }

                if (result != FSDK.FSDKE_OK)
                {
                    result = FSDKCam.OpenVideoCamera(ref cameraName, ref cameraHandle);
                }
            }

            if (result != FSDK.FSDKE_OK)
            {
                MessageBox.Show("Error opening the first camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            SetMessage("Please select an user and click on the face", Color.Orange);

            while (isStarted)
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

                Graphics gr = Graphics.FromImage(frameImage);
                string code = String.Empty;
                string fullname = String.Empty;

                //New detection
                fr.Code = String.Empty;
                fr.Fullname = String.Empty;
                fr.Group = String.Empty;
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

                TFaceRecord nface = frmMain.GoSearchFace(fr, FaceList);
                if (nface.Code.Length > 0)
                { // draw name
                    string drawText = nface.Fullname;
                    if (drawText.Length <= 0)
                        drawText = "Unknown";

                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    gr.DrawString(drawText, new System.Drawing.Font("Times New Roman", 18),
                        new System.Drawing.SolidBrush(System.Drawing.Color.LightSeaGreen),
                        fr.FacePosition.xc, top + w + 5, format);
                }

                Pen pen = Pens.LightGreen;
                if (mouseX >= left && mouseX <= left + w && mouseY >= top && mouseY <= top + w)
                {
                    pen = Pens.Blue;
                    if (isRemove)
                        pen = Pens.OrangeRed;
                    if (ProgramState.psRemember == programState)
                    {
                        //needSave = true;
                        string keyName = cbbUsers.Text.Trim();
                        string group = cbbGroup.Text;
                        code = keyName.Split('-')[0].Trim();
                        fullname = keyName.Split('-')[1].Trim();

                        if (!String.IsNullOrEmpty(keyName) && !String.IsNullOrEmpty(group))
                        {
                            fr.Code = code;
                            fr.Fullname = fullname;
                            fr.Group = group;
                            //fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition.xc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.xc + Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc + Math.Round(fr.FacePosition.w * 0.5)));
                            //fr.FacialFeatures = fr.image.DetectFacialFeaturesInRegion(ref fr.FacePosition);
                            //fr.Template = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition); // get template with higher precision
                            //===========

                            if (fr.FacePosition.xc > 0 && fr.FacePosition.yc > 0)
                            {
                                this.SaveFaceInDB(fr);
                                int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Added user ({keyName})", "Info");
                                if (res == 0)
                                {
                                    log.Info($"Added user ({keyName})");
                                }
                                SetMessage($"Added new user {keyName}!", Color.Green);
                            }
                        }
                        else
                        {
                            SetMessage("Please select an user and group then click on the face!", Color.Red);
                        }
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
                }

                programState = ProgramState.psRecognize;

                // display current frame
                pbCamera.Image = frameImage;
                GC.Collect(); // collect the garbage after the deletion

                // make UI controls accessible (to find if the user clicked on a face)
                Application.DoEvents();
            }

            if (mSettings.UseDCAM710 || cameraName == "PicoZense RGBD Camera")
            {
                //====== DCAM 710 ========
                psReturnstatus = Ps2_CloseDevice(ref pdevicde);
                psReturnstatus = Ps2_Shutdown();
            }
            else
            {
                FSDKCam.CloseVideoCamera(cameraHandle);
                FSDKCam.FinalizeCapturing();
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

        private void SaveImage(FSDK.CImage image, int left, int top, int w)
        {
            int head = (int)(w * 0.1);
            FSDK.CImage faceImage = image.CopyRect(left - head, top - head, left + w,  top + w);
        }

        private void frmInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            isStarted = false;
        }

        private void pbCamera_MouseLeave(object sender, EventArgs e)
        {
            mouseX = 0;
            mouseY = 0;
        }

        private void pbCamera_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
        }

        private void pbCamera_MouseUp(object sender, MouseEventArgs e)
        {
            programState = ProgramState.psRemember;
        }

        private void SetMessage(string text, Color color)
        {
            lblMessage.ForeColor = color;
            lblMessage.Text = text;
            this.Refresh();
        }

        private void lblMessage_Click(object sender, EventArgs e)
        {
            if (!isStarted)
            {
                SetMessage("Starting camera...", Color.Orange);
                isStarted = true;
                try
                {
                    Run();
                }
                catch
                {
                    isStarted = false;
                    SetMessage("Failed in running camera! Please try again!", Color.Red);
                }
                cbbUsers.Focus();
            }
            else
            {
                MessageBox.Show(lblMessage.Text, "Info");
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            string keyName = cbbUsers.Text.Trim();
            string code = keyName.Split('-')[0].Trim();
            string fullname = keyName.Split('-')[1].Trim();
            DialogResult result = MessageBox.Show("Do you want to delete face list of " + code + "-" + fullname + "?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                if (String.IsNullOrEmpty(keyName))
                {
                    SetMessage("Select user to remove first", Color.Red);
                    return;
                }
                else
                {
                    this.RemoveFaceInDB(code);
                    int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Removed user ({fullname})", "Info");
                    if (res == 0)
                    {
                        log.Info($"Removed user ({fullname})");
                    }
                    SetMessage($"Removed user {fullname}!", Color.DarkRed);
                    cbbUsers.Text = "";
                }
            }
        }

        private void SaveFaceInDB(TFaceRecord fr)
        {
            //======
            int hasFaceMask = rdHasFaceMask.Checked ? 1 : 0;
            System.Data.SqlClient.SqlConnection sqlConnect = null;
            try
            {
                //preparing FaceRecord to save
                Image img = null;
                Image img_face = null;
                MemoryStream strm = new MemoryStream();
                MemoryStream strm_face = new MemoryStream();
                img = fr.image.ToCLRImage();
                img_face = fr.faceImage.ToCLRImage();
                img.Save(strm, System.Drawing.Imaging.ImageFormat.Jpeg);
                img_face.Save(strm_face, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] img_array = new byte[strm.Length];
                byte[] img_face_array = new byte[strm_face.Length];
                strm.Position = 0;
                strm.Read(img_array, 0, img_array.Length);
                strm_face.Position = 0;
                strm_face.Read(img_face_array, 0, img_face_array.Length);

                //connect to Microsoft SQL Server and save FaceRecord
                sqlConnect = new System.Data.SqlClient.SqlConnection("server=" + frmMain.SQLServerAddress + "; initial catalog=" + frmMain.DatabaseName + "; User Id=" + frmMain.DBUid + "; Pwd=" + frmMain.DBPwd);
                sqlConnect.Open();
                System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand("INSERT INTO FaceList([Code], [Fullname], [Group], [FacePositionXc], [FacePositionYc], [FacePositionW], [FacePositionAngle], [Eye1X], [Eye1Y], [Eye2X], [Eye2Y], [Template], [Image], [FaceImage], [SimilarThreshold], [HasFaceMask], [CreatedDate]) " +
                     " values(@Code, @Fullname, @Group, @FacePositionXc, @FacePositionYc, @FacePositionW, @FacePositionAngle, @Eye1X, @Eye1Y, @Eye2X, @Eye2Y, @Template, @Image, @FaceImage, @SimilarThreshold , " + hasFaceMask + ", GETDATE())",
                     sqlConnect);

                sqlCmd.Parameters.Add("@Code", System.Data.SqlDbType.VarChar, 50);
                sqlCmd.Parameters.Add("@Fullname", System.Data.SqlDbType.VarChar, 260);
                sqlCmd.Parameters.Add("@Group", System.Data.SqlDbType.VarChar, 50);
                sqlCmd.Parameters.Add("@FacePositionXc", System.Data.SqlDbType.Int);
                sqlCmd.Parameters.Add("@FacePositionYc", System.Data.SqlDbType.Int);
                sqlCmd.Parameters.Add("@FacePositionW", System.Data.SqlDbType.Int);
                sqlCmd.Parameters.Add("@FacePositionAngle", System.Data.SqlDbType.Real);
                sqlCmd.Parameters.Add("@Eye1X", System.Data.SqlDbType.Int);
                sqlCmd.Parameters.Add("@Eye1Y", System.Data.SqlDbType.Int);
                sqlCmd.Parameters.Add("@Eye2X", System.Data.SqlDbType.Int);
                sqlCmd.Parameters.Add("@Eye2Y", System.Data.SqlDbType.Int);
                sqlCmd.Parameters.Add("@Template", System.Data.SqlDbType.VarBinary);
                sqlCmd.Parameters.Add("@Image", System.Data.SqlDbType.VarBinary);
                sqlCmd.Parameters.Add("@FaceImage", System.Data.SqlDbType.VarBinary);
                sqlCmd.Parameters.Add("@SimilarThreshold", System.Data.SqlDbType.Real);

                sqlCmd.Parameters["@Code"].Value = fr.Code;
                sqlCmd.Parameters["@Fullname"].Value = fr.Fullname;
                sqlCmd.Parameters["@Group"].Value = fr.Group;
                sqlCmd.Parameters["@FacePositionXc"].Value = fr.FacePosition.xc;
                sqlCmd.Parameters["@FacePositionYc"].Value = fr.FacePosition.yc;
                sqlCmd.Parameters["@FacePositionW"].Value = fr.FacePosition.w;
                sqlCmd.Parameters["@FacePositionAngle"].Value = (float)fr.FacePosition.angle;
                sqlCmd.Parameters["@Eye1X"].Value = fr.FacialFeatures[0].x;
                sqlCmd.Parameters["@Eye1Y"].Value = fr.FacialFeatures[0].y;
                sqlCmd.Parameters["@Eye2X"].Value = fr.FacialFeatures[1].x;
                sqlCmd.Parameters["@Eye2Y"].Value = fr.FacialFeatures[1].y;
                sqlCmd.Parameters["@Template"].Value = fr.Template;
                sqlCmd.Parameters["@Image"].Value = img_array;
                sqlCmd.Parameters["@FaceImage"].Value = img_face_array;
                sqlCmd.Parameters["@SimilarThreshold"].Value = nudSimilarThreshold.Value;

                int iresult = sqlCmd.ExecuteNonQuery();

                img.Dispose();
                img_face.Dispose();
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
        }

        private void RemoveFaceInDB(string Code)
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
                //MessageBox.Show(ex.Message, "Exception on delete Face from database");
            }
            finally
            {
                if (sqlConnect != null)
                    sqlConnect.Close();
            }
        }

        private void rdHasFaceMask_CheckedChanged(object sender, EventArgs e)
        {
            nudSimilarThreshold.Value = (decimal)0.9994;
        }

        private void rdNoFaceMask_CheckedChanged(object sender, EventArgs e)
        {
            nudSimilarThreshold.Value = (decimal)0.9988;
        }
    }
}
