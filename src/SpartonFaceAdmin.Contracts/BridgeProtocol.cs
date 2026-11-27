using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpartonFaceAdmin.Contracts;

public enum BridgeCommand
{
    Ping = 0,
    DetectOnly = 1,
    ExtractTemplate = 2,
    Shutdown = 3
}

public sealed class BridgeRequest
{
    public BridgeCommand Command { get; set; }
    public byte[]? ImageBytes { get; set; }
    public int DetectionThreshold { get; set; } = 5;
}

public sealed class FaceBoxDto
{
    public int Xc { get; set; }
    public int Yc { get; set; }
    public int W { get; set; }
    public float Angle { get; set; }
}

public sealed class EyePointDto
{
    public int X { get; set; }
    public int Y { get; set; }
}

public sealed class BridgeResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool FaceDetected { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public FaceBoxDto? FaceBox { get; set; }
    public EyePointDto? Eye1 { get; set; }
    public EyePointDto? Eye2 { get; set; }
    public byte[]? Template { get; set; }
    public byte[]? FullImageJpeg { get; set; }
    public byte[]? FaceImageJpeg { get; set; }
}

/// <summary>
/// Zero-dependency length-prefixed binary framing over NamedPipeStream.
/// Avoids System.Text.Json / NuGet binding redirects in the .NET Framework 4.8 bridge
/// so ClickOnce and folder publishes never fail with "Pipe is broken".
/// </summary>
public static class BridgePipeProtocol
{
    private const int MaxPayloadBytes = 64 * 1024 * 1024;

    public static async Task WriteRequestAsync(Stream stream, BridgeRequest request, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
        {
            bw.Write((int)request.Command);
            bw.Write(request.DetectionThreshold);
            WriteBytes(bw, request.ImageBytes);
        }

        await WriteFrameAsync(stream, ms.ToArray(), cancellationToken).ConfigureAwait(false);
    }

    public static async Task<BridgeRequest?> ReadRequestAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[]? body = await ReadFrameAsync(stream, cancellationToken).ConfigureAwait(false);
        if (body == null)
            return null;

        using var ms = new MemoryStream(body);
        using var br = new BinaryReader(ms, Encoding.UTF8, leaveOpen: false);

        return new BridgeRequest
        {
            Command = (BridgeCommand)br.ReadInt32(),
            DetectionThreshold = br.ReadInt32(),
            ImageBytes = ReadBytes(br)
        };
    }

    public static async Task WriteResponseAsync(Stream stream, BridgeResponse response, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
        {
            bw.Write(response.Success);
            WriteNullableString(bw, response.ErrorMessage);
            bw.Write(response.FaceDetected);
            bw.Write(response.ImageWidth);
            bw.Write(response.ImageHeight);

            bw.Write(response.FaceBox != null);
            if (response.FaceBox != null)
            {
                bw.Write(response.FaceBox.Xc);
                bw.Write(response.FaceBox.Yc);
                bw.Write(response.FaceBox.W);
                bw.Write(response.FaceBox.Angle);
            }

            bw.Write(response.Eye1 != null);
            if (response.Eye1 != null)
            {
                bw.Write(response.Eye1.X);
                bw.Write(response.Eye1.Y);
            }

            bw.Write(response.Eye2 != null);
            if (response.Eye2 != null)
            {
                bw.Write(response.Eye2.X);
                bw.Write(response.Eye2.Y);
            }

            WriteBytes(bw, response.Template);
            WriteBytes(bw, response.FullImageJpeg);
            WriteBytes(bw, response.FaceImageJpeg);
        }

        await WriteFrameAsync(stream, ms.ToArray(), cancellationToken).ConfigureAwait(false);
    }

    public static async Task<BridgeResponse?> ReadResponseAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[]? body = await ReadFrameAsync(stream, cancellationToken).ConfigureAwait(false);
        if (body == null)
            return null;

        using var ms = new MemoryStream(body);
        using var br = new BinaryReader(ms, Encoding.UTF8, leaveOpen: false);

        var res = new BridgeResponse
        {
            Success = br.ReadBoolean(),
            ErrorMessage = ReadNullableString(br),
            FaceDetected = br.ReadBoolean(),
            ImageWidth = br.ReadInt32(),
            ImageHeight = br.ReadInt32()
        };

        if (br.ReadBoolean())
        {
            res.FaceBox = new FaceBoxDto
            {
                Xc = br.ReadInt32(),
                Yc = br.ReadInt32(),
                W = br.ReadInt32(),
                Angle = br.ReadSingle()
            };
        }

        if (br.ReadBoolean())
        {
            res.Eye1 = new EyePointDto
            {
                X = br.ReadInt32(),
                Y = br.ReadInt32()
            };
        }

        if (br.ReadBoolean())
        {
            res.Eye2 = new EyePointDto
            {
                X = br.ReadInt32(),
                Y = br.ReadInt32()
            };
        }

        res.Template = ReadBytes(br);
        res.FullImageJpeg = ReadBytes(br);
        res.FaceImageJpeg = ReadBytes(br);
        return res;
    }

    private static void WriteBytes(BinaryWriter bw, byte[]? data)
    {
        if (data == null)
        {
            bw.Write(-1);
            return;
        }
        bw.Write(data.Length);
        if (data.Length > 0)
            bw.Write(data);
    }

    private static byte[]? ReadBytes(BinaryReader br)
    {
        int len = br.ReadInt32();
        if (len < 0)
            return null;
        if (len == 0)
            return Array.Empty<byte>();
        return br.ReadBytes(len);
    }

    private static void WriteNullableString(BinaryWriter bw, string? value)
    {
        bw.Write(value != null);
        if (value != null)
            bw.Write(value);
    }

    private static string? ReadNullableString(BinaryReader br)
    {
        return br.ReadBoolean() ? br.ReadString() : null;
    }

    private static async Task WriteFrameAsync(Stream stream, byte[] body, CancellationToken cancellationToken)
    {
        byte[] lenPrefix = BitConverter.GetBytes(body.Length);
        await stream.WriteAsync(lenPrefix, 0, 4, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(body, 0, body.Length, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<byte[]?> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] lenPrefix = new byte[4];
        if (!await ReadExactAsync(stream, lenPrefix, 4, cancellationToken).ConfigureAwait(false))
            return null;

        int length = BitConverter.ToInt32(lenPrefix, 0);
        if (length <= 0 || length > MaxPayloadBytes)
            throw new InvalidDataException($"Invalid IPC message length: {length}");

        byte[] body = new byte[length];
        if (!await ReadExactAsync(stream, body, length, cancellationToken).ConfigureAwait(false))
            return null;

        return body;
    }

    private static async Task<bool> ReadExactAsync(Stream stream, byte[] buffer, int count, CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer, offset, count - offset, cancellationToken).ConfigureAwait(false);
            if (read == 0)
                return false;
            offset += read;
        }
        return true;
    }
}
