using System.Runtime.InteropServices;
using OpenCvSharp;

namespace SpartonFaceAdmin.Infrastructure.Camera;

public static class FrameEncodingHelper
{
    public static byte[] EncodeBgr24ToJpeg(CameraFrameData frame, int quality = 90)
    {
        return EncodeBgr24Core(frame, maxWidth: null, quality: quality);
    }

    public static byte[] EncodeBgr24ToDownscaledJpeg(CameraFrameData frame, int maxWidth = 640, int quality = 85)
    {
        return EncodeBgr24Core(frame, maxWidth: maxWidth, quality: quality);
    }

    private static byte[] EncodeBgr24Core(CameraFrameData frame, int? maxWidth, int quality)
    {
        var pinned = GCHandle.Alloc(frame.Bgr24Bytes, GCHandleType.Pinned);
        try
        {
            using var src = Mat.FromPixelData(frame.Height, frame.Width, MatType.CV_8UC3, pinned.AddrOfPinnedObject(), frame.Stride);
            if (maxWidth.HasValue && frame.Width > maxWidth.Value)
            {
                int targetWidth = maxWidth.Value;
                int targetHeight = Math.Max(1, (int)Math.Round(frame.Height * ((double)targetWidth / frame.Width)));
                using var resized = new Mat();
                Cv2.Resize(src, resized, new Size(targetWidth, targetHeight), 0, 0, InterpolationFlags.Area);
                Cv2.ImEncode(".jpg", resized, out byte[] resizedJpeg, new ImageEncodingParam(ImwriteFlags.JpegQuality, quality));
                return resizedJpeg;
            }

            Cv2.ImEncode(".jpg", src, out byte[] jpeg, new ImageEncodingParam(ImwriteFlags.JpegQuality, quality));
            return jpeg;
        }
        finally
        {
            if (pinned.IsAllocated)
                pinned.Free();
        }
    }
}
