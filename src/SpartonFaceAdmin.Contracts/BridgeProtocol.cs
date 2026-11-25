using System;
using System.IO;
using System.Text;
using System.Text.Json;
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
/// Length-prefixed UTF-8 JSON framing over NamedPipeStream to prevent partial-read corruption.
/// </summary>
public static class LengthPrefixedJsonPipe
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task WriteMessageAsync<T>(Stream stream, T payload, CancellationToken cancellationToken)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(payload, SerializerOptions);
        byte[] lenPrefix = BitConverter.GetBytes(body.Length);
        await stream.WriteAsync(lenPrefix, 0, 4, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(body, 0, body.Length, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<T?> ReadMessageAsync<T>(Stream stream, CancellationToken cancellationToken)
    {
        byte[] lenPrefix = new byte[4];
        if (!await ReadExactAsync(stream, lenPrefix, 4, cancellationToken).ConfigureAwait(false))
            return default;

        int length = BitConverter.ToInt32(lenPrefix, 0);
        if (length <= 0 || length > 64 * 1024 * 1024)
            throw new InvalidDataException($"Invalid IPC message length: {length}");

        byte[] body = new byte[length];
        if (!await ReadExactAsync(stream, body, length, cancellationToken).ConfigureAwait(false))
            return default;

        return JsonSerializer.Deserialize<T>(body, SerializerOptions);
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
