using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpartonFaceAdmin.App.Services;
using SpartonFaceAdmin.Core.Models;
using SpartonFaceAdmin.Infrastructure.Camera;
using SpartonFaceAdmin.Infrastructure.Configuration;
using SpartonFaceAdmin.Infrastructure.Data;

namespace SpartonFaceAdmin.App.ViewModels;

public sealed record ResolutionOption(string Label, int Width, int Height);

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly AppStateService _appState;
    private readonly ToastService _toast;

    // HR Database
    [ObservableProperty] private string _hrServer = string.Empty;
    [ObservableProperty] private string _hrDatabase = string.Empty;
    [ObservableProperty] private string _hrUsername = string.Empty;
    [ObservableProperty] private string _hrPassword = string.Empty;
    [ObservableProperty] private int _hrTimeout = 3;

    // Face Database
    [ObservableProperty] private string _faceServer = string.Empty;
    [ObservableProperty] private string _faceDatabase = string.Empty;
    [ObservableProperty] private string _faceUsername = string.Empty;
    [ObservableProperty] private string _facePassword = string.Empty;
    [ObservableProperty] private int _faceTimeout = 3;

    // Camera
    [ObservableProperty] private CameraSourceType _cameraSourceType = CameraSourceType.Usb;
    [ObservableProperty] private CameraDeviceInfo? _selectedUsbCamera;
    [ObservableProperty] private ResolutionOption? _selectedResolution;
    [ObservableProperty] private string _ipCameraUrl = string.Empty;
    [ObservableProperty] private string _ipCameraUsername = string.Empty;
    [ObservableProperty] private string _ipCameraPassword = string.Empty;
    [ObservableProperty] private int _ipCameraTimeout = 3;

    [ObservableProperty] private bool _isTesting;

    public ObservableCollection<CameraDeviceInfo> UsbCameras { get; } = new();

    public IReadOnlyList<ResolutionOption> Resolutions { get; } = new[]
    {
        new ResolutionOption("640 x 480", 640, 480),
        new ResolutionOption("1280 x 720 (HD)", 1280, 720),
        new ResolutionOption("1920 x 1080 (Full HD)", 1920, 1080)
    };

    public event Action? PasswordFieldsReloaded;

    public SettingsViewModel(AppStateService appState, ToastService toast)
    {
        _appState = appState;
        _toast = toast;
        RefreshCameraDevices();
        ReloadFromAppState();
    }

    [RelayCommand]
    private void RefreshCameraDevices()
    {
        UsbCameras.Clear();
        var detected = CameraDeviceEnumerator.GetUsbCameras();
        if (detected.Count == 0)
        {
            UsbCameras.Add(new CameraDeviceInfo(0, "Default Camera (Index 0)"));
        }
        else
        {
            foreach (var cam in detected)
                UsbCameras.Add(cam);
        }
    }

    public void ReloadFromAppState()
    {
        AppSettings s = _appState.Settings;

        HrServer = s.HrDatabase.Server;
        HrDatabase = s.HrDatabase.Database;
        HrUsername = s.HrDatabase.Username;
        HrPassword = DpapiSettingsStore.UnprotectSecret(s.HrDatabase.ProtectedPassword);
        HrTimeout = s.HrDatabase.TimeoutSeconds;

        FaceServer = s.FaceDatabase.Server;
        FaceDatabase = s.FaceDatabase.Database;
        FaceUsername = s.FaceDatabase.Username;
        FacePassword = DpapiSettingsStore.UnprotectSecret(s.FaceDatabase.ProtectedPassword);
        FaceTimeout = s.FaceDatabase.TimeoutSeconds;

        CameraSourceType = s.Camera.SourceType;
        SelectedUsbCamera = UsbCameras.FirstOrDefault(c => c.Index == s.Camera.UsbCameraIndex) ?? UsbCameras.FirstOrDefault();
        SelectedResolution = Resolutions.FirstOrDefault(r => r.Width == s.Camera.UsbWidth && r.Height == s.Camera.UsbHeight)
                             ?? Resolutions[1];

        IpCameraUrl = s.Camera.IpCameraUrl;
        IpCameraUsername = s.Camera.IpCameraUsername;
        IpCameraPassword = DpapiSettingsStore.UnprotectSecret(s.Camera.IpCameraProtectedPassword);
        IpCameraTimeout = s.Camera.IpCameraTimeoutSeconds;

        PasswordFieldsReloaded?.Invoke();
    }

    [RelayCommand]
    private async Task TestHrConnectionAsync()
    {
        IsTesting = true;
        try
        {
            var cfg = new DatabaseConnectionSettings
            {
                Server = HrServer,
                Database = HrDatabase,
                Username = HrUsername,
                TimeoutSeconds = HrTimeout
            };
            var (ok, err) = await SqlConnectionFactory.TestConnectionAsync(cfg, HrPassword);
            _toast.Show(ok ? "Kết nối HR SQL thành công!" : $"Lỗi HR SQL: {err}", ok ? ToastKind.Success : ToastKind.Error, 3200);
        }
        finally
        {
            IsTesting = false;
        }
    }

    [RelayCommand]
    private async Task TestFaceConnectionAsync()
    {
        IsTesting = true;
        try
        {
            var cfg = new DatabaseConnectionSettings
            {
                Server = FaceServer,
                Database = FaceDatabase,
                Username = FaceUsername,
                TimeoutSeconds = FaceTimeout
            };
            var (ok, err) = await SqlConnectionFactory.TestConnectionAsync(cfg, FacePassword);
            _toast.Show(ok ? "Kết nối Face SQL thành công!" : $"Lỗi Face SQL: {err}", ok ? ToastKind.Success : ToastKind.Error, 3200);
        }
        finally
        {
            IsTesting = false;
        }
    }

    [RelayCommand]
    private async Task TestCameraAsync()
    {
        IsTesting = true;
        try
        {
            var camCfg = BuildCameraSettings();
            await Task.Run(() =>
            {
                using var source = CameraSourceFactory.Create(camCfg);
                source.Open();
            });
            _toast.Show("Kết nối Camera thành công!", ToastKind.Success);
        }
        catch (Exception ex)
        {
            _toast.Show($"Lỗi Camera: {ex.Message}", ToastKind.Error, 3500);
        }
        finally
        {
            IsTesting = false;
        }
    }

    [RelayCommand]
    private void Save()
    {
        AppSettings s = _appState.Settings;

        s.HrDatabase.Server = HrServer.Trim();
        s.HrDatabase.Database = FaceStringOrEmpty(HrDatabase);
        s.HrDatabase.Username = HrUsername.Trim();
        s.HrDatabase.ProtectedPassword = DpapiSettingsStore.ProtectSecret(HrPassword);
        s.HrDatabase.TimeoutSeconds = Math.Clamp(HrTimeout, 1, 60);

        s.FaceDatabase.Server = FaceServer.Trim();
        s.FaceDatabase.Database = FaceStringOrEmpty(FaceDatabase);
        s.FaceDatabase.Username = FaceUsername.Trim();
        s.FaceDatabase.ProtectedPassword = DpapiSettingsStore.ProtectSecret(FacePassword);
        s.FaceDatabase.TimeoutSeconds = Math.Clamp(FaceTimeout, 1, 60);

        s.Camera = BuildCameraSettings();
        _appState.SaveSettings();
        _toast.Show("Đã lưu cài đặt.", ToastKind.Success);
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        if (MessageBox.Show("Khôi phục toàn bộ cài đặt về mặc định ban đầu?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        _appState.Settings = new AppSettings();
        _appState.SaveSettings();
        ReloadFromAppState();
        _toast.Show("Đã khôi phục mặc định.", ToastKind.Info);
    }

    private CameraSettings BuildCameraSettings()
    {
        var res = SelectedResolution ?? Resolutions[1];
        return new CameraSettings
        {
            SourceType = CameraSourceType,
            UsbCameraIndex = SelectedUsbCamera?.Index ?? 0,
            UsbWidth = res.Width,
            UsbHeight = res.Height,
            IpCameraUrl = IpCameraUrl.Trim(),
            IpCameraUsername = IpCameraUsername.Trim(),
            IpCameraProtectedPassword = DpapiSettingsStore.ProtectSecret(IpCameraPassword),
            IpCameraTimeoutSeconds = Math.Clamp(IpCameraTimeout, 1, 30)
        };
    }

    private static string FaceStringOrEmpty(string? s) => s?.Trim() ?? string.Empty;
}
