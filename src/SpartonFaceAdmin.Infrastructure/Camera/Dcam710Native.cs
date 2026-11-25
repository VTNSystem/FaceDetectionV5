using System.Runtime.InteropServices;

namespace SpartonFaceAdmin.Infrastructure.Camera;

internal static class Dcam710Native
{
    public enum PsReturnStatus
    {
        PsRetOK = 0,
        PsRetNoDeviceConnected = -1,
        PsRetReadNextFrameTimeOut = -11
    }

    public enum PsDeviceType
    {
        NONE,
        DCAM710 = 710
    }

    public enum PsConnectStatus
    {
        ConnectUNKNOWN,
        Unconnected,
        Connected,
        Opened
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PsDeviceInfo
    {
        public int SessionCount;
        public PsDeviceType devicetype;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string uri;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string fw;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string alias;
        public PsConnectStatus status;
    }

    public enum PsFrameType
    {
        PsDepthFrame = 0,
        PsIRFrame = 1,
        PsRGBFrame = 3
    }

    public enum PsPixelFormat
    {
        PsPixelFormatDepthMM16,
        PsPixelFormatGray16,
        PsPixelFormatGray8,
        PsPixelFormatRGB888,
        PsPixelFormatBGR888
    }

    public enum PsDepthRange
    {
        PsUnknown = -1,
        PsNearRange = 0
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PsTimeStamp
    {
        public ushort tm_sec;
        public ushort tm_min;
        public ushort tm_hour;
        public ushort tm_msec;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct PsFrameReady
    {
        public uint depth;
        public uint ir;
        public uint rgb;
        public uint mappedRGB;
        public uint mappedDepth;
        public uint mappedIR;
        public uint confidence;
        public uint wdrDepth;
        public uint reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PsFrame
    {
        public uint frameIndex;
        public PsFrameType frameType;
        public PsPixelFormat pixelFormat;
        public byte imuFrameNo;
        public IntPtr pFrameData;
        public uint dataLen;
        public float exposureTime;
        public PsDepthRange depthRange;
        public ushort width;
        public ushort height;
        public PsTimeStamp timestamp;
    }

    public enum PsDataMode
    {
        PsDepthAndRGB_30 = 0
    }

    public enum PsResolution
    {
        PsRGB_Resolution_1280_720 = 1,
        PsRGB_Resolution_640_480 = 2
    }

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_Initialize();

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_GetDeviceCount(ref uint deviceCount);

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_GetDeviceInfo(ref PsDeviceInfo pDevices, uint deviceIndex);

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_OpenDevice(string pDeviceListInfo_uri, ref IntPtr deviceHandle);

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_CloseDevice(ref IntPtr device);

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_Shutdown();

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_StartStream(IntPtr device, uint sessionIndex);

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_SetDataMode(IntPtr device, uint sessionIndex, PsDataMode dataMode);

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_SetRGBResolution(IntPtr device, uint sessionIndex, PsResolution resolution);

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_ReadNextFrame(IntPtr device, uint sessionIndex, ref PsFrameReady frameReady);

    [DllImport("vzense_api.dll")]
    public static extern PsReturnStatus Ps2_GetFrame(IntPtr device, uint sessionIndex, PsFrameType frameType, ref PsFrame rgbFrame);
}
