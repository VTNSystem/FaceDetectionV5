using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using SpartonFaceAdmin.Contracts;

namespace SpartonFaceAdmin.Infrastructure.Bridge;

/// <summary>
/// Manages the hidden .NET 4.8 LuxandBridge.exe worker process and serializes IPC calls.
/// Automatically restarts the bridge if it crashes or the pipe disconnects.
/// </summary>
public sealed class LuxandBridgeClient : IAsyncDisposable
{
    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private readonly string _pipeName = $"SpartonFaceAdmin.Bridge.{Environment.ProcessId}.{Guid.NewGuid():N}";

    private Process? _bridgeProcess;
    private NamedPipeClientStream? _pipe;
    private bool _disposed;

    public async Task<BridgeResponse> DetectOnlyAsync(byte[] imageBytes, int threshold = 5, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            new BridgeRequest
            {
                Command = BridgeCommand.DetectOnly,
                ImageBytes = imageBytes,
                DetectionThreshold = threshold
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<BridgeResponse> ExtractTemplateAsync(byte[] imageBytes, int threshold = 5, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            new BridgeRequest
            {
                Command = BridgeCommand.ExtractTemplate,
                ImageBytes = imageBytes,
                DetectionThreshold = threshold
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(new BridgeRequest { Command = BridgeCommand.Ping }, cancellationToken).ConfigureAwait(false);
        return response.Success;
    }

    private async Task<BridgeResponse> SendAsync(BridgeRequest request, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _ioLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
                    await BridgePipeProtocol.WriteRequestAsync(_pipe!, request, cancellationToken).ConfigureAwait(false);
                    BridgeResponse? response = await BridgePipeProtocol
                        .ReadResponseAsync(_pipe!, cancellationToken)
                        .ConfigureAwait(false);

                    if (response != null)
                        return response;

                    ResetConnection();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (attempt == 0)
                {
                    ResetConnection();
                    Serilog.Log.Warning(ex, "LuxandBridge IPC failed on first attempt; restarting bridge.");
                }
            }

            return new BridgeResponse
            {
                Success = false,
                ErrorMessage = "LuxandBridge did not return a valid response."
            };
        }
        finally
        {
            _ioLock.Release();
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_pipe is { IsConnected: true } && _bridgeProcess is { HasExited: false })
            return;

        ResetConnection();

        string bridgeExe = ResolveBridgeExecutablePath();
        var psi = new ProcessStartInfo
        {
            FileName = bridgeExe,
            Arguments = $"--pipe \"{_pipeName}\" --parent-pid {Environment.ProcessId}",
            WorkingDirectory = Path.GetDirectoryName(bridgeExe) ?? AppContext.BaseDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        _bridgeProcess = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start LuxandBridge.exe.");

        _pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));
        await _pipe.ConnectAsync(timeoutCts.Token).ConfigureAwait(false);
    }

    public static string ResolveBridgeExecutablePath()
    {
        string localBridge = Path.Combine(AppContext.BaseDirectory, "bridge", "SpartonFaceAdmin.Bridge.exe");
        if (File.Exists(localBridge))
            return localBridge;

        string adjacent = Path.Combine(AppContext.BaseDirectory, "SpartonFaceAdmin.Bridge.exe");
        if (File.Exists(adjacent))
            return adjacent;

        throw new FileNotFoundException(
            $"LuxandBridge executable not found at '{localBridge}'. Rebuild solution to stage bridge binaries.");
    }

    private void ResetConnection()
    {
        try { _pipe?.Dispose(); } catch { }
        _pipe = null;

        if (_bridgeProcess != null)
        {
            try
            {
                if (!_bridgeProcess.HasExited)
                    _bridgeProcess.Kill(entireProcessTree: true);
            }
            catch { }
            _bridgeProcess.Dispose();
            _bridgeProcess = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;

        await _ioLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_pipe is { IsConnected: true })
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                    await BridgePipeProtocol.WriteRequestAsync(
                        _pipe,
                        new BridgeRequest { Command = BridgeCommand.Shutdown },
                        cts.Token).ConfigureAwait(false);
                }
                catch { }
            }
            ResetConnection();
        }
        finally
        {
            _ioLock.Release();
            _ioLock.Dispose();
        }
    }
}
