using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using SpartonFaceAdmin.Contracts;

namespace SpartonFaceAdmin.Bridge;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        string pipeName = "SpartonFaceAdmin.LuxandBridge";
        int? parentPid = null;

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--pipe")
                pipeName = args[i + 1];
            else if (args[i] == "--parent-pid" && int.TryParse(args[i + 1], out int parsedPid))
                parentPid = parsedPid;
        }

        using var cts = new CancellationTokenSource();
        WatchParentProcess(parentPid, cts);

        using var engine = new LuxandEngineService();
        try
        {
            engine.Initialize();
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Luxand initialization failed: {ex}");
            return 1;
        }

        RunServerLoopAsync(pipeName, engine, cts.Token).GetAwaiter().GetResult();
        return 0;
    }

    private static void WatchParentProcess(int? parentPid, CancellationTokenSource cts)
    {
        if (!parentPid.HasValue)
            return;

        Task.Run(async () =>
        {
            try
            {
                using var parent = Process.GetProcessById(parentPid.Value);
                while (!cts.IsCancellationRequested && !parent.HasExited)
                {
                    await Task.Delay(1000, cts.Token).ConfigureAwait(false);
                }
            }
            catch
            {
                // Parent exited or is inaccessible; shut down bridge cleanly.
            }
            cts.Cancel();
        });
    }

    private static async Task RunServerLoopAsync(string pipeName, LuxandEngineService engine, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var server = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                await server.WaitForConnectionAsync(ct).ConfigureAwait(false);
                while (server.IsConnected && !ct.IsCancellationRequested)
                {
                    BridgeRequest? req = await BridgePipeProtocol
                        .ReadRequestAsync(server, ct)
                        .ConfigureAwait(false);

                    if (req == null)
                        break;

                    if (req.Command == BridgeCommand.Shutdown)
                    {
                        await BridgePipeProtocol.WriteResponseAsync(
                            server,
                            new BridgeResponse { Success = true },
                            ct).ConfigureAwait(false);
                        return;
                    }

                    BridgeResponse res = engine.Handle(req);
                    await BridgePipeProtocol.WriteResponseAsync(server, res, ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Bridge pipe session ended: {ex}");
            }
        }
    }
}
