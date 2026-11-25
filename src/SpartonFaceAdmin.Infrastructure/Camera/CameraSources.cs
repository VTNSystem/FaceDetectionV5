using System.Runtime.InteropServices;
using OpenCvSharp;
using SpartonFaceAdmin.Core.Models;
using SpartonFaceAdmin.Infrastructure.Configuration;

namespace SpartonFaceAdmin.Infrastructure.Camera;

public sealed record CameraFrameData(int Width, int Height, int Stride, byte[] Bgr24Bytes);

public interface ICameraCaptureSource : IDisposable
{
    void Open();
    bool TryReadFrame(out CameraFrameData? frame);
}

internal sealed class OpenCvCameraSource : ICameraCaptureSource
{
    private readonly CameraSettings _settings;
    private VideoCapture? _capture;
    private Mat? _frameMat;
    private Mat? _bgrMat;

    public OpenCvCameraSource(CameraSettings settings)
    {
        _settings = settings;
    }

    public void Open()
    {
        if (_settings.SourceType == CameraSourceType.IpCamera)
        {
            string url = BuildIpCameraUrl(_settings);
            _capture = new VideoCapture();
            int timeoutMs = Math.Clamp(_settings.IpCameraTimeoutSeconds, 1, 30) * 1000;
            // CAP_PROP_OPEN_TIMEOUT_MSEC (53) and CAP_PROP_READ_TIMEOUT_MSEC (54)
            _capture.Set((VideoCaptureProperties)53, timeoutMs);
            _capture.Set((VideoCaptureProperties)54, timeoutMs);
            _capture.Open(url, VideoCaptureAPIs.FFMPEG);
            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = null;
                throw new InvalidOperationException($"Cannot connect to IP Camera: {_settings.IpCameraUrl}");
            }
        }
        else
        {
            _capture = new VideoCapture(_settings.UsbCameraIndex, VideoCaptureAPIs.DSHOW);
            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = new VideoCapture(_settings.UsbCameraIndex, VideoCaptureAPIs.ANY);
            }

            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = null;
                throw new InvalidOperationException($"Cannot open USB Camera #{_settings.UsbCameraIndex}.");
            }

            _capture.Set(VideoCaptureProperties.FrameWidth, _settings.UsbWidth);
            _capture.Set(VideoCaptureProperties.FrameHeight, _settings.UsbHeight);
        }

        _frameMat = new Mat();
        _bgrMat = new Mat();
    }

    public bool TryReadFrame(out CameraFrameData? frame)
    {
        frame = null;
        if (_capture == null || _frameMat == null || _bgrMat == null)
            return false;

        if (!_capture.Read(_frameMat) || _frameMat.Empty())
            return false;

        Mat source = _frameMat;
        if (_frameMat.Channels() == 4)
        {
            Cv2.CvtColor(_frameMat, _bgrMat, ColorConversionCodes.BGRA2BGR);
            source = _bgrMat;
        }
        else if (_frameMat.Channels() == 1)
        {
            Cv2.CvtColor(_frameMat, _bgrMat, ColorConversionCodes.GRAY2BGR);
            source = _bgrMat;
        }

        if (!source.IsContinuous())
        {
            source.CopyTo(_bgrMat);
            source = _bgrMat;
        }

        int stride = (int)source.Step();
        int byteLen = stride * source.Rows;
        byte[] buffer = new byte[byteLen];
        Marshal.Copy(source.Data, buffer, 0, byteLen);

        frame = new CameraFrameData(source.Cols, source.Rows, stride, buffer);
        return true;
    }

    public static string BuildIpCameraUrl(CameraSettings settings)
    {
        string raw = (settings.IpCameraUrl ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(raw))
            throw new InvalidOperationException("IP Camera URL is empty.");

        if (!raw.Contains("://", StringComparison.Ordinal))
            raw = "http://" + raw;

        var uri = new UriBuilder(raw);
        if (string.IsNullOrEmpty(uri.UserName) && !string.IsNullOrWhiteSpace(settings.IpCameraUsername))
        {
            uri.UserName = settings.IpCameraUsername.Trim();
            uri.Password = DpapiSettingsStore.UnprotectSecret(settings.IpCameraProtectedPassword);
        }
        return uri.Uri.AbsoluteUri;
    }

    public void Dispose()
    {
        _bgrMat?.Dispose();
        _frameMat?.Dispose();
        _capture?.Release();
        _capture?.Dispose();
    }
}

internal sealed class Dcam710CameraSource : ICameraCaptureSource
{
    private IntPtr _deviceHandle = IntPtr.Zero;
    private bool _sdkInitialized;

    public void Open()
    {
        if (Dcam710Native.Ps2_Initialize() != Dcam710Native.PsReturnStatus.PsRetOK)
            throw new InvalidOperationException("DCAM710 SDK initialization failed.");
        _sdkInitialized = true;

        uint count = 0;
        Dcam710Native.Ps2_GetDeviceCount(ref count);
        if (count == 0)
            throw new InvalidOperationException("No DCAM710 device connected.");

        var devInfo = new Dcam710Native.PsDeviceInfo();
        Dcam710Native.Ps2_GetDeviceInfo(ref devInfo, 0);

        _deviceHandle = Marshal.AllocHGlobal(64);
        var openStatus = Dcam710Native.Ps2_OpenDevice(devInfo.uri, ref _deviceHandle);
        if (openStatus != Dcam710Native.PsReturnStatus.PsRetOK)
            throw new InvalidOperationException($"DCAM710 open failed ({openStatus}).");

        Dcam710Native.Ps2_StartStream(_deviceHandle, 0);
        Dcam710Native.Ps2_SetDataMode(_deviceHandle, 0, Dcam710Native.PsDataMode.PsDepthAndRGB_30);
        Dcam710Native.Ps2_SetRGBResolution(_deviceHandle, 0, Dcam710Native.PsResolution.PsRGB_Resolution_1280_720);
    }

    public bool TryReadFrame(out CameraFrameData? frame)
    {
        frame = null;
        if (_deviceHandle == IntPtr.Zero)
            return false;

        var ready = new Dcam710Native.PsFrameReady();
        if (Dcam710Native.Ps2_ReadNextFrame(_deviceHandle, 0, ref ready) != Dcam710Native.PsReturnStatus.PsRetOK)
            return false;

        var rgb = new Dcam710Native.PsFrame();
        if (Dcam710Native.Ps2_GetFrame(_deviceHandle, 0, Dcam710Native.PsFrameType.PsRGBFrame, ref rgb) != Dcam710Native.PsReturnStatus.PsRetOK ||
            rgb.pFrameData == IntPtr.Zero ||
            rgb.dataLen == 0)
        {
            return false;
        }

        byte[] buffer = new byte[rgb.dataLen];
        Marshal.Copy(rgb.pFrameData, buffer, 0, (int)rgb.dataLen);
        frame = new CameraFrameData(rgb.width, rgb.height, rgb.width * 3, buffer);
        return true;
    }

    public void Dispose()
    {
        if (_deviceHandle != IntPtr.Zero)
        {
            try { Dcam710Native.Ps2_CloseDevice(ref _deviceHandle); } catch { }
            if (_deviceHandle != IntPtr.Zero)
            {
                try { Marshal.FreeHGlobal(_deviceHandle); } catch { }
                _deviceHandle = IntPtr.Zero;
            }
        }
        if (_sdkInitialized)
        {
            try { Dcam710Native.Ps2_Shutdown(); } catch { }
            _sdkInitialized = false;
        }
    }
}

public static class CameraSourceFactory
{
    public static ICameraCaptureSource Create(CameraSettings settings) =>
        settings.SourceType == CameraSourceType.Dcam710
            ? new Dcam710CameraSource()
            : new OpenCvCameraSource(settings);
}
