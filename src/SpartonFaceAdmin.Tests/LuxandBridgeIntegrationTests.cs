using System.Windows.Media;
using System.Windows.Media.Imaging;
using SpartonFaceAdmin.Infrastructure.Bridge;
using SpartonFaceAdmin.Infrastructure.Camera;
using Xunit;

namespace SpartonFaceAdmin.Tests;

public sealed class LuxandBridgeIntegrationTests
{
    [Fact]
    public async Task BridgeProcess_StagesAlongsideApp_ActivatesFaceSdk_AndDetectsCleanly()
    {
        string bridgePath = LuxandBridgeClient.ResolveBridgeExecutablePath();
        Assert.True(File.Exists(bridgePath), $"Missing staged bridge executable at {bridgePath}");

        await using var client = new LuxandBridgeClient();
        bool pingOk = await client.PingAsync();
        Assert.True(pingOk, "LuxandBridge failed PingAsync (license activation or pipe handshake failed).");

        // Send a blank 320x240 JPEG to verify roundtrip image decoding & FaceSDK invocation without leaks/crash.
        var frame = new CameraFrameData(320, 240, 320 * 3, new byte[320 * 240 * 3]);
        byte[] jpeg = FrameEncodingHelper.EncodeBgr24ToJpeg(frame);

        var response = await client.DetectOnlyAsync(jpeg);
        Assert.True(response.Success, response.ErrorMessage);
        Assert.False(response.FaceDetected);
        Assert.Equal(320, response.ImageWidth);
        Assert.Equal(240, response.ImageHeight);
    }

    [Fact]
    public void FrameEncodingHelper_ProducesValidJpegFromBgr24()
    {
        var frame = new CameraFrameData(160, 120, 160 * 3, new byte[160 * 120 * 3]);
        byte[] jpeg = FrameEncodingHelper.EncodeBgr24ToJpeg(frame);

        Assert.True(jpeg.Length > 100);
        Assert.Equal(0xFF, jpeg[0]);
        Assert.Equal(0xD8, jpeg[1]);
    }

    [Fact]
    public async Task BackgroundThread_DownscaledEncoding_AndBridgeDetection_Succeeds()
    {
        await using var client = new LuxandBridgeClient();
        var frame = new CameraFrameData(1280, 720, 1280 * 3, new byte[1280 * 720 * 3]);

        // Run on MTA threadpool thread exactly like StartDetectionLoop to ensure no WPF/WIC STA issues.
        var response = await Task.Run(async () =>
        {
            byte[] jpeg = FrameEncodingHelper.EncodeBgr24ToDownscaledJpeg(frame, maxWidth: 640, quality: 85);
            return await client.DetectOnlyAsync(jpeg, threshold: 3);
        });

        Assert.True(response.Success, response.ErrorMessage);
        Assert.False(response.FaceDetected);
        Assert.Equal(640, response.ImageWidth);
        Assert.Equal(360, response.ImageHeight);
    }
}
