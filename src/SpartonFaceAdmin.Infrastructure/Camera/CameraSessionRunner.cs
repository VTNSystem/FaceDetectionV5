using System.Threading.Channels;
using SpartonFaceAdmin.Core.Models;

namespace SpartonFaceAdmin.Infrastructure.Camera;

/// <summary>
/// Runs camera capture on a dedicated background thread and publishes frames safely
/// without blocking the WPF UI thread or leaking GDI+ handles.
/// </summary>
public sealed class CameraSessionRunner : IAsyncDisposable
{
    private readonly Channel<CameraFrameData> _detectChannel = Channel.CreateBounded<CameraFrameData>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });

    private CancellationTokenSource? _cts;
    private Task? _captureTask;
    private volatile CameraFrameData? _latestFrame;

    public event Action<CameraFrameData>? FrameReceived;
    public event Action<string>? CameraError;

    public bool IsRunning => _captureTask is { IsCompleted: false };

    public CameraFrameData? LatestFrame => _latestFrame;

    public ChannelReader<CameraFrameData> DetectionFrames => _detectChannel.Reader;

    public Task StartAsync(CameraSettings settings)
    {
        if (IsRunning)
            return Task.CompletedTask;

        _cts = new CancellationTokenSource();
        CancellationToken ct = _cts.Token;

        _captureTask = Task.Factory.StartNew(() =>
        {
            using ICameraCaptureSource source = CameraSourceFactory.Create(settings);
            try
            {
                source.Open();
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(ex.Message);
                return;
            }

            long lastDetectEnqueueTick = 0;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (source.TryReadFrame(out CameraFrameData? frame) && frame != null)
                    {
                        _latestFrame = frame;
                        FrameReceived?.Invoke(frame);

                        long now = Environment.TickCount64;
                        if (now - lastDetectEnqueueTick >= 180)
                        {
                            lastDetectEnqueueTick = now;
                            _detectChannel.Writer.TryWrite(frame);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    CameraError?.Invoke(ex.Message);
                    Thread.Sleep(250);
                }
            }
        }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cts == null)
            return;

        _cts.Cancel();
        if (_captureTask != null)
        {
            try { await _captureTask.ConfigureAwait(false); } catch { }
        }
        _cts.Dispose();
        _cts = null;
        _captureTask = null;
        _latestFrame = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _detectChannel.Writer.TryComplete();
    }
}
