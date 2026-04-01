using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FaceDetection
{
    public class DCAM710Handler
    {
        public enum PsReturnStatus
        {
            PsRetOK = 0,   //!< The function completed successfully.
            PsRetNoDeviceConnected = -1,   //!< There is no depth camera connected or the camera has not been connected correctly. Check the hardware connection or try unplugging and re-plugging the USB cable.
            PsRetInvalidDeviceIndex = -2,   //!< The input device index is invalid.
            PsRetDevicePointerIsNull = -3,   //!< The device structure pointer is null.
            PsRetInvalidFrameType = -4,   //!< The input frame type is invalid.
            PsRetFramePointerIsNull = -5,   //!< The output frame buffer is null.
            PsRetNoPropertyValueGet = -6,   //!< Cannot get the value for the specified property.
            PsRetNoPropertyValueSet = -7,   //!< Cannot set the value for the specified property.
            PsRetPropertyPointerIsNull = -8,   //!< The input property value buffer pointer is null.
            PsRetPropertySizeNotEnough = -9,   //!< The input property value buffer size is too small to store the specified property value.
            PsRetInvalidDepthRange = -10,  //!< The input depth range mode is invalid.
            PsRetReadNextFrameTimeOut = -11,  //!< Capture the next image frame time out.
            PsRetInputPointerIsNull = -12,  //!< An input pointer parameter is null.
            PsRetCameraNotOpened = -13,  //!< The camera has not been opened.
            PsRetInvalidCameraType = -14,  //!< The specified type of camera is invalid.
            PsRetInvalidParams = -15,  //!< One or more of the parameter values provided are invalid.
            PsRetCurrentVersionNotSupport = -16,  //!< This feature is not supported in the current version.
            PsRetUpgradeImgError = -17,  //!< There is an error in the upgrade file.
            PsRetUpgradeImgPathTooLong = -18,  //!< Upgrade file path length greater than 260.
            PsRetUpgradeCallbackNotSet = -19,  //!< Ps2_SetUpgradeStatusCallback is not called.
            PsRetNoAdapterConnected = -100, //!< There is no adapter connected
            PsRetReInitialized = -101,  //!< The SDK has been Initialized
            PsRetNoInitialized = -102,  //!< The SDK has bot been Initialized
            PsRetCameraOpened = -103,   //!< The camera has been opened.
            PsRetCmdError = -104,   //!< Set/Get cmd control error
            PsRetCmdSyncTimeOut = -105, //!< Set cmd ok.but time out for the sync return 

            PsRetOthers = -255,	             //!< An unknown error occurred.
        }

        public enum PsDeviceType
        {
            NONE,
            DCAM305 = 305,
            DCAM500 = 500,
            CSI100 = 501,
            DCAM510 = 510,
            DCAM550U = 550,
            DCAM550P = 551,
            DCAM550E = 552,
            DCAM560 = 560,
            DCAM560CPRO = 561,
            DCAM560CLITE = 562,
            DCAM710 = 710,
            DCAM800 = 800,
            DCAM_MIPI = 801,
            DCAM800LITE = 802,
            DCAM800LITEUSB = 803,
            DCAM101 = 804,

            MAX,
        }

        public enum PsConnectStatus
        {
            ConnectUNKNOWN,
            Unconnected,
            Connected,
            Opened,
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //C# structure
        public struct PsDeviceHandle
        {

        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //C# structure
        public struct PsCameraParameters
        {
            public double fx;  //!< Focal length x (pixel)
            public double fy;  //!< Focal length y (pixel)
            public double cx;  //!< Principal point x (pixel)
            public double cy;  //!< Principal point y (pixel)
            public double k1;  //!< Radial distortion coefficient, 1st-order
            public double k2;  //!< Radial distortion coefficient, 2nd-order
            public double p1;  //!< Tangential distortion coefficient
            public double p2;  //!< Tangential distortion coefficient
            public double k3;  //!< Radial distortion coefficient, 3rd-order
            public double k4;  //!< Radial distortion coefficient, 4st-order
            public double k5;  //!< Radial distortion coefficient, 5nd-order
            public double k6;  //!< Radial distortion coefficient, 6rd-order
        }

        public enum PsSensorType
        {
            PsDepthSensor = 0x01,          //!< Depth camera.
            PsRgbSensor = 0x02             //!< Color (RGB) camera.
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //C# structure
        public struct PsDeviceInfo
        {
            public int SessionCount;
            public PsDeviceType devicetype;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string uri; //DeviceType:sn=xxxxx eg:DCAM305:sn=xxxxxxx
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string fw;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string alias;
            public PsConnectStatus status;
        }

        public enum PsFrameType
        {
            PsDepthFrame = 0,              //!< Depth frame with 16 bits per pixel in millimeters.
            PsIRFrame = 1,                 //!< IR frame with 16 bits per pixel.
            PsRGBFrame = 3,                //!< RGB frame with 24 bits per pixel in RGB/BGR format.
            PsMappedRGBFrame = 4,          //!< RGB frame with 24 bits per pixel in RGB/BGR format, that is mapped to depth camera space where the resolution is the same as the depth frame's resolution.\n 
                                           //!< This frame type can be enabled using ::PsSetMapperEnabledDepthToRGB().
            PsMappedDepthFrame = 5,        //!< Depth frame with 16 bits per pixel, in millimeters, that is mapped to RGB camera space where the resolution is same as the RGB frame's resolution.\n 
                                           //!< This frame type can be enabled using ::PsSetMapperEnabledRGBToDepth().
            PsMappedIRFrame = 6,           //!< IR frame with 16 bits per pixel that is mapped to RGB camera space where the resolution is the same as the RGB frame's resolution.\n 
                                           //!< This frame type can be enabled using ::PsSetMapperEnabledRGBToIR().
            PsWDRDepthFrame = 9            //!< WDR depth frame with 16 bits per pixel in millimeters. This only takes effect when the data mode set to ::PsWDR_Depth.
        }

        public enum PsPixelFormat
        {
            PsPixelFormatDepthMM16,        //!< Depth image pixel format, 16 bits per pixel in mm.
            PsPixelFormatGray16,           //!< IR image pixel format, 16 bits per pixel.
            PsPixelFormatGray8,            //!< Gray image pixel format, 8 bits per pixel.

            //Color
            PsPixelFormatRGB888,           //!< Color image pixel format, 24 bits per pixel RGB format.
            PsPixelFormatBGR888            //!< Color image pixel format, 24 bits per pixel BGR format.
        }

        public enum PsDepthRange
        {
            PsUnknown = -1,
            PsNearRange = 0,
            PsMidRange = 1,
            PsFarRange = 2,
            PsXNearRange = 3,
            PsXMidRange = 4,
            PsXFarRange = 5,
            PsXXNearRange = 6,
            PsXXMidRange = 7,
            PsXXFarRange = 8
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] //C# structure
        public struct PsTimeStamp
        {
            public UInt16 tm_sec;   // seconds after the minute - [0, 60] including leap second
            public UInt16 tm_min;   // minutes after the hour - [0, 59]
            public UInt16 tm_hour;  // hours since midnight - [0, 23]
            public UInt16 tm_msec;  // millisecond after the second - [0, 999]
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)] //C# structure
        public struct PsFrameReady
        {
            public UInt32 depth;
            public UInt32 ir;
            public UInt32 rgb;
            public UInt32 mappedRGB;
            public UInt32 mappedDepth;
            public UInt32 mappedIR;
            public UInt32 confidence;
            public UInt32 wdrDepth;
            public UInt32 reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct PsFrame
        {
            public UInt32 frameIndex;    //!< The index of the frame.
            public PsFrameType frameType;     //!< The type of frame. See ::PsFrameType for more information.
            public PsPixelFormat pixelFormat;   //!< The pixel format used by a frame. See ::PsPixelFormat for more information.
            public byte imuFrameNo;    //!< Used to synchronize with IMU, in the range of 0 to 255.
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public IntPtr pFrameData;
            //public byte[] pFrameData;    //!< A buffer containing the frame’s image data.
            public UInt32 dataLen;       //!< The length of pFrame, in bytes.
            public float exposureTime;  //!< The exposure time, in milliseconds.
            public PsDepthRange depthRange;    //!< The depth range mode of the current frame. Used only for depth frames.
            public UInt16 width;       //!< The width of the frame, in pixels.
            public UInt16 height;        //!< The height of the frame, in pixels.
            public PsTimeStamp timestamp;    //!< The timestamp of the frame.
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct PsCameraExtrinsicParameters
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public double[] rotation;     //!< Orientation stored as an array of 9 double representing a 3x3 rotation matrix.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public double[] translation;  //!< Location stored as an array of 3 double representing a 3-D translation vector.
        }

        public enum PsDataMode
        {
            PsDepthAndRGB_30 = 0,           //!< Output both depth and RGB frames at 30 fps. The resolution of a depth frame is 640*480.\n 
                                            //!< The resolution of an RGB frame can be set using ::PsSetFrameMode(), which supports 1920*1080/1280*720/640*360.
            PsIRAndRGB_30 = 1,              //!< Outputs both IR and RGB frames at 30 fps. The resolution of an IR frame is 640*480.\n 
                                            //!< The resolution of an RGB frame can be set using ::PsSetFrameMode(), which supports 1920*1080/1280*720/640*360.
            PsDepthAndIR_30 = 2,            //!< Outputs both depth and IR frames at 30 fps. The resolution for both depth and IR frames is 640*480.
            PsNoCCD_30 = 4,                 //!< Reserved for internal use.
            PsDepthAndIR_15_RGB_30 = 10,    //!< Outputs depth and IR frames at 15 fps, alternating between the two. The resolution of both depth and IR frames is 640*480.\n 
                                            //!< The resolution of an RGB frame can be set using ::PsSetFrameMode(), which supports 1920*1080/1280*720/640*360.
            PsWDR_Depth = 11,               //!< WDR (Wide Dynamic Range) depth mode. Supports alternating multi-range depth frame output (e.g. Near/Far/Near/Far/Near).
            PsWDR_IR = 12,                  //!< WDR (Wide Dynamic Range) IR mode. Not currently implemented.
            PsWDR_DepthAndIR = 13,          //!< WDR (Wide Dynamic Range) Depth and IR mode. Not currently implemented.
        }

        public enum PsResolution
        {
            PsRGB_Resolution_1920_1080 = 0,
            PsRGB_Resolution_1280_720 = 1,
            PsRGB_Resolution_640_480 = 2,
            PsRGB_Resolution_640_360 = 3,
        }


        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_Initialize();
        public delegate void HotPlugStateCallback(PsDeviceInfo pInfo, int param);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_SetHotPlugStatusCallback(HotPlugStateCallback func);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_GetDeviceCount(ref UInt32 deviceCount);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_GetDeviceInfo(ref PsDeviceInfo pDevices, UInt32 deviceIndex);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_GetDeviceListInfo(PsDeviceInfo[] pDeviceListInfo, UInt32 deviceCount);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_OpenDevice(string pDeviceListInfo_uri, ref IntPtr deviceHandle);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_CloseDevice(ref IntPtr device);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_Shutdown();
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_StartStream(IntPtr device, UInt32 sessionIndex);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_GetCameraParameters(IntPtr device, UInt32 sessionIndex, PsSensorType sensorType, ref PsCameraParameters cameraParameters);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_GetCameraExtrinsicParameters(IntPtr device, UInt32 sessionIndex, ref PsCameraExtrinsicParameters CameraExtrinsicParameters);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_ReadNextFrame(IntPtr device, UInt32 sessionIndex, ref PsFrameReady frameReady);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_GetDataMode(IntPtr device, UInt32 sessionIndex, ref PsDataMode dataMode);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_SetDataMode(IntPtr device, UInt32 sessionIndex, PsDataMode dataMode);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_GetFrame(IntPtr device, UInt32 sessionIndex, PsFrameType psRGBFrame, ref PsFrame rgbFrame);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_GetThreshold(IntPtr device, UInt32 sessionIndex, ref UInt16 pThreshold);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_SetThreshold(IntPtr device, UInt32 sessionIndex, UInt16 pThreshold);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_SetRGBResolution(IntPtr device, UInt32 sessionIndex, PsResolution resolution);
        [DllImport("vzense_api.dll")]
        static public extern PsReturnStatus Ps2_SetColorPixelFormat(IntPtr device, UInt32 sessionIndex, PsPixelFormat pixelFormat);
    }
}
