using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SpartonFaceAdmin.App.Services;
using SpartonFaceAdmin.App.ViewModels;
using SpartonFaceAdmin.App.Views;
using SpartonFaceAdmin.Infrastructure.Bridge;
using SpartonFaceAdmin.Infrastructure.Configuration;
using SpartonFaceAdmin.Infrastructure.Data;
using SpartonFaceAdmin.Infrastructure.Export;

namespace SpartonFaceAdmin.App;

public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(true, "SpartonFaceAdmin_SingleInstance_Mutex", out bool isFirst);
        if (!isFirst)
        {
            MessageBox.Show("Sparton Face Admin đang chạy.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        string logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SpartonFaceAdmin",
            "logs");
        Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(logDir, "admin-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
            .CreateLogger();

        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        MainWindow window = _services.GetRequiredService<MainWindow>();
        window.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DpapiSettingsStore>();
        services.AddSingleton<AppStateService>();
        services.AddSingleton<ToastService>();
        services.AddSingleton<HrRepository>();
        services.AddSingleton<FaceRepository>();
        services.AddSingleton<ExcelCardExporter>();
        services.AddSingleton<LuxandBridgeClient>();

        services.AddSingleton<EnrollmentViewModel>();
        services.AddSingleton<FaceListViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_services != null)
        {
            try
            {
                var enrollment = _services.GetRequiredService<EnrollmentViewModel>();
                await enrollment.ShutdownAsync();
                var bridge = _services.GetRequiredService<LuxandBridgeClient>();
                await bridge.DisposeAsync();
            }
            catch { }

            await _services.DisposeAsync();
        }

        Log.CloseAndFlush();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
