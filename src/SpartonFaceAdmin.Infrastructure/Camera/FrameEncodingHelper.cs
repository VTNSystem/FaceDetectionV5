using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpartonFaceAdmin.Infrastructure.Camera;

public static class FrameEncodingHelper
{
    public static byte[] EncodeBgr24ToJpeg(CameraFrameData frame, int quality = 90)
    {
        var bmp = BitmapSource.Create(
            frame.Width,
            frame.Height,
            96,
            96,
            PixelFormats.Bgr24,
            null,
            frame.Bgr24Bytes,
            frame.Stride);

        var encoder = new JpegBitmapEncoder { QualityLevel = quality };
        encoder.Frames.Add(BitmapFrame.Create(bmp));

        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    public static byte[] EncodeBgr24ToDownscaledJpeg(CameraFrameData frame, int maxWidth = 480)
    {
        BitmapSource src = BitmapSource.Create(
            frame.Width,
            frame.Height,
            96,
            96,
            PixelFormats.Bgr24,
            null,
            frame.Bgr24Bytes,
            frame.Stride);

        if (frame.Width > maxWidth)
        {
            double scale = (double)maxWidth / frame.Width;
            src = new TransformedBitmap(src, new ScaleTransform(scale, scale));
        }

        var encoder = new JpegBitmapEncoder { QualityLevel = 75 };
        encoder.Frames.Add(BitmapFrame.Create(src));

        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }
}
