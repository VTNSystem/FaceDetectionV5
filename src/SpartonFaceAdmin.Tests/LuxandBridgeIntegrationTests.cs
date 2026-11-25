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
}
