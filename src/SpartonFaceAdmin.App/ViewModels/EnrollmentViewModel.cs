using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SpartonFaceAdmin.App.Services;
using SpartonFaceAdmin.Contracts;
using SpartonFaceAdmin.Core.Models;
using SpartonFaceAdmin.Core.Utilities;
using SpartonFaceAdmin.Infrastructure.Bridge;
using SpartonFaceAdmin.Infrastructure.Camera;
using SpartonFaceAdmin.Infrastructure.Data;

namespace SpartonFaceAdmin.App.ViewModels;

public sealed partial class EnrollmentViewModel : ObservableObject
{
    private readonly AppStateService _appState;
    private readonly ToastService _toast;
    private readonly FaceRepository _faceRepo;
    private readonly LuxandBridgeClient _bridgeClient;
    private readonly Dispatcher _dispatcher;

    private CameraSessionRunner? _cameraRunner;
    private CancellationTokenSource? _detectLoopCts;
    private CancellationTokenSource? _faceListLoadCts;
    private WriteableBitmap? _liveWriteableBitmap;
    private byte[]? _loadedFileBytes;
    private bool _suppressScalePersist;
    private int _uiFramePending;

    [ObservableProperty]
    private ImageSource? _previewSource;

    [ObservableProperty]
    private double _framePixelWidth = 1280;

    [ObservableProperty]
    private double _framePixelHeight = 720;

    [ObservableProperty]
    private bool _isCameraRunning;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasPreviewContent;

    [ObservableProperty]
    private bool _isFitMode = true;

    [ObservableProperty]
    private int _scalePercent = 100;

    [ObservableProperty]
    private bool _hasDetectedFace;

    [ObservableProperty]
    private double _faceLeft;

    [ObservableProperty]
    private double _faceTop;

    [ObservableProperty]
    private double _faceSize;

    [ObservableProperty]
    private string _employeeSearchText = string.Empty;

    [ObservableProperty]
    private EmployeeItem? _selectedEmployee;

    [ObservableProperty]
    private string _selectedGroup = UserGroupOption.Office;

    [ObservableProperty]
    private bool _hasFaceMask;

    [ObservableProperty]
    private string _similarThresholdText = "0.9988";

    [ObservableProperty]
    private IReadOnlyList<EmployeeItem> _filteredEmployees = Array.Empty<EmployeeItem>();

    [ObservableProperty]
    private string _manualEntryBadge = string.Empty;

    public bool HasManualEntry => !string.IsNullOrEmpty(ManualEntryBadge);

    public ObservableCollection<FaceRecordSummary> SelectedEmployeeFaces { get; } = new();
    public IReadOnlyList<string> Groups => UserGroupOption.All;

    public double ScaleFactor => ScalePercent / 100.0;
    public double ScaledViewportWidth => FramePixelWidth * ScaleFactor;
    public double ScaledViewportHeight => FramePixelHeight * ScaleFactor;
    public string ScaleBadgeText => IsFitMode ? "Fit" : $"{ScalePercent}%";

    public EnrollmentViewModel(
        AppStateService appState,
        ToastService toast,
        FaceRepository faceRepo,
        LuxandBridgeClient bridgeClient)
    {
        _appState = appState;
        _toast = toast;
        _faceRepo = faceRepo;
        _bridgeClient = bridgeClient;
        _dispatcher = Application.Current.Dispatcher;

        _suppressScalePersist = true;
        IsFitMode = _appState.Settings.IsFitPreviewScale;
        ScalePercent = Math.Clamp(_appState.Settings.PreviewScalePercent, 50, 150);
        _suppressScalePersist = false;

        _appState.EmployeesUpdated += OnEmployeesUpdated;
        ApplyEmployeeFilter();
    }

    private void OnEmployeesUpdated()
    {
        _dispatcher.Invoke(() =>
        {
            string? currentCode = SelectedEmployee?.EmployeeCode;
            ApplyEmployeeFilter();
            if (!string.IsNullOrEmpty(currentCode))
            {
                SelectedEmployee = FilteredEmployees.FirstOrDefault(x =>
                    string.Equals(x.EmployeeCode, currentCode, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedEmployee != null)
            {
                _ = LoadFacesByCodeAsync(SelectedEmployee.EmployeeCode);
            }
            else
            {
                SelectedEmployeeFaces.Clear();
                UpdateManualEntryState();
            }
        });
    }

    partial void OnManualEntryBadgeChanged(string value) => OnPropertyChanged(nameof(HasManualEntry));

    partial void OnEmployeeSearchTextChanged(string value)
    {
        if (SelectedEmployee != null &&
            VietnameseTextHelper.TryParseManualEmployeeInput(value, out string typedCode, out _, out _) &&
            !string.Equals(typedCode, SelectedEmployee.EmployeeCode, StringComparison.OrdinalIgnoreCase))
        {
            SelectedEmployee = null;
        }

        ApplyEmployeeFilter();
        UpdateManualEntryState();
    }

    partial void OnSelectedEmployeeChanged(EmployeeItem? value)
    {
        if (value != null)
        {
            ManualEntryBadge = string.Empty;
            _ = LoadFacesByCodeAsync(value.EmployeeCode);
        }
        else
        {
            UpdateManualEntryState();
        }
    }

    partial void OnHasFaceMaskChanged(bool value)
    {
        SimilarThresholdText = value ? "0.9994" : "0.9988";
    }

    partial void OnIsFitModeChanged(bool value)
    {
        OnPropertyChanged(nameof(ScaleBadgeText));
        PersistScaleSettings();
    }

    partial void OnScalePercentChanged(int value)
    {
        int clamped = Math.Clamp(value, 50, 150);
        if (clamped != value)
        {
            ScalePercent = clamped;
            return;
        }

        if (!_suppressScalePersist && IsFitMode)
            IsFitMode = false;

        OnPropertyChanged(nameof(ScaleFactor));
        OnPropertyChanged(nameof(ScaledViewportWidth));
        OnPropertyChanged(nameof(ScaledViewportHeight));
        OnPropertyChanged(nameof(ScaleBadgeText));
        PersistScaleSettings();
    }

    partial void OnFramePixelWidthChanged(double value) => OnPropertyChanged(nameof(ScaledViewportWidth));
    partial void OnFramePixelHeightChanged(double value) => OnPropertyChanged(nameof(ScaledViewportHeight));

    private void PersistScaleSettings()
    {
        if (_suppressScalePersist)
            return;

        _appState.Settings.IsFitPreviewScale = IsFitMode;
        _appState.Settings.PreviewScalePercent = ScalePercent;
        _appState.SaveSettings();
    }

    private void ApplyEmployeeFilter()
    {
        string q = (EmployeeSearchText ?? string.Empty).Trim();
        string qAscii = VietnameseTextHelper.VniToEng(q);

        IEnumerable<EmployeeItem> source = _appState.Employees;
        if (!string.IsNullOrEmpty(q))
        {
            source = source.Where(e =>
                e.EmployeeCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.AsciiFullName.Contains(qAscii, StringComparison.OrdinalIgnoreCase));
        }

        FilteredEmployees = source.ToList();
    }

    private void UpdateManualEntryState()
    {
        if (SelectedEmployee == null &&
            VietnameseTextHelper.TryParseManualEmployeeInput(EmployeeSearchText, out string code, out _, out string ascii))
        {
            ManualEntryBadge = $"Nhập tay: {code} - {ascii}";
            _ = LoadFacesByCodeAsync(code);
        }
        else
        {
            ManualEntryBadge = string.Empty;
            if (SelectedEmployee == null)
            {
                _faceListLoadCts?.Cancel();
                SelectedEmployeeFaces.Clear();
            }
        }
    }

    [RelayCommand]
    private void SetFitMode() => IsFitMode = true;

    [RelayCommand]
    private void SetScalePreset(string? percentStr)
    {
        if (int.TryParse(percentStr, out int p))
        {
            IsFitMode = false;
            ScalePercent = p;
        }
    }

    public void AdjustZoomByWheel(int delta)
    {
        int step = delta > 0 ? 10 : -10;
        IsFitMode = false;
        ScalePercent = Math.Clamp(ScalePercent + step, 50, 150);
    }

    [RelayCommand]
    private async Task ToggleCameraAsync()
    {
        if (IsCameraRunning)
            await StopCameraIfRunningAsync();
        else
            await StartCameraAsync();
    }

    private async Task StartCameraAsync()
    {
        await StopCameraIfRunningAsync();
        _loadedFileBytes = null;
        ClearFaceOverlay();

        _cameraRunner = new CameraSessionRunner();
        _cameraRunner.FrameReceived += OnCameraFrameReceived;
        _cameraRunner.CameraError += OnCameraError;

        IsCameraRunning = true;
        HasPreviewContent = true;

        await _cameraRunner.StartAsync(_appState.Settings.Camera);
        StartDetectionLoop(_cameraRunner);
    }

    public async Task StopCameraIfRunningAsync()
    {
        _detectLoopCts?.Cancel();
        _detectLoopCts?.Dispose();
        _detectLoopCts = null;

        if (_cameraRunner != null)
        {
            _cameraRunner.FrameReceived -= OnCameraFrameReceived;
            _cameraRunner.CameraError -= OnCameraError;
            await _cameraRunner.DisposeAsync();
            _cameraRunner = null;
        }

        IsCameraRunning = false;
        if (_loadedFileBytes == null)
        {
            HasPreviewContent = false;
            PreviewSource = null;
            _liveWriteableBitmap = null;
        }
        ClearFaceOverlay();
    }

    private void OnCameraError(string message)
    {
        _dispatcher.BeginInvoke(() =>
        {
            _toast.Show($"Lỗi camera: {message}", ToastKind.Error, 3500);
            _ = StopCameraIfRunningAsync();
        });
    }

    private void OnCameraFrameReceived(CameraFrameData frame)
    {
        if (Interlocked.Exchange(ref _uiFramePending, 1) == 1)
            return;

        _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
        {
            try
            {
                if (!IsCameraRunning)
                    return;

                if (_liveWriteableBitmap == null ||
                    _liveWriteableBitmap.PixelWidth != frame.Width ||
                    _liveWriteableBitmap.PixelHeight != frame.Height)
                {
                    _liveWriteableBitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr24, null);
                    PreviewSource = _liveWriteableBitmap;
                    FramePixelWidth = frame.Width;
                    FramePixelHeight = frame.Height;
                }

                _liveWriteableBitmap.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height),
                    frame.Bgr24Bytes,
                    frame.Stride,
                    0);
            }
            finally
            {
                Interlocked.Exchange(ref _uiFramePending, 0);
            }
        });
    }

    private void StartDetectionLoop(CameraSessionRunner runner)
    {
        _detectLoopCts = new CancellationTokenSource();
        CancellationToken ct = _detectLoopCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await foreach (CameraFrameData frame in runner.DetectionFrames.ReadAllAsync(ct))
                {
                    if (!IsCameraRunning)
                        break;

                    byte[] previewJpeg = FrameEncodingHelper.EncodeBgr24ToDownscaledJpeg(frame, maxWidth: 480);
                    BridgeResponse res = await _bridgeClient.DetectOnlyAsync(previewJpeg, 5, ct);

                    await _dispatcher.InvokeAsync(() =>
                    {
                        if (!IsCameraRunning)
                            return;

                        if (res.Success && res.FaceDetected && res.FaceBox != null && res.ImageWidth > 0 && res.ImageHeight > 0)
                        {
                            double scaleX = FramePixelWidth / res.ImageWidth;
                            double scaleY = FramePixelHeight / res.ImageHeight;
                            double size = res.FaceBox.W * 1.2 * scaleX;
                            FaceLeft = (res.FaceBox.Xc * scaleX) - (size / 2.0);
                            FaceTop = (res.FaceBox.Yc * scaleY) - (size / 2.0);
                            FaceSize = size;
                            HasDetectedFace = true;
                        }
                        else
                        {
                            HasDetectedFace = false;
                        }
                    });
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }, ct);
    }

    [RelayCommand]
    private async Task LoadImageFileAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Chọn ảnh khuôn mặt",
            Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*"
        };

        if (dlg.ShowDialog() != true)
            return;

        await StopCameraIfRunningAsync();

        try
        {
            byte[] bytes = await File.ReadAllBytesAsync(dlg.FileName);
            var bmp = new BitmapImage();
            using (var ms = new MemoryStream(bytes))
            {
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
            }

            _loadedFileBytes = bytes;
            PreviewSource = bmp;
            FramePixelWidth = bmp.PixelWidth;
            FramePixelHeight = bmp.PixelHeight;
            HasPreviewContent = true;

            BridgeResponse res = await _bridgeClient.DetectOnlyAsync(bytes, 5);
            if (res.Success && res.FaceDetected && res.FaceBox != null)
            {
                double size = res.FaceBox.W * 1.2;
                FaceLeft = res.FaceBox.Xc - (size / 2.0);
                FaceTop = res.FaceBox.Yc - (size / 2.0);
                FaceSize = size;
                HasDetectedFace = true;
            }
            else
            {
                ClearFaceOverlay();
                _toast.Show("Không tìm thấy khuôn mặt trong ảnh đã chọn.", ToastKind.Warning);
            }
        }
        catch (Exception ex)
        {
            _toast.Show($"Không đọc được ảnh: {ex.Message}", ToastKind.Error);
        }
    }

    [RelayCommand]
    private async Task CaptureAndSaveAsync()
    {
        if (IsBusy)
            return;

        string targetCode;
        string targetDisplayName;
        string targetAsciiName;
        bool isManualEntry = false;

        if (SelectedEmployee != null)
        {
            targetCode = SelectedEmployee.EmployeeCode;
            targetDisplayName = SelectedEmployee.FullName;
            targetAsciiName = SelectedEmployee.AsciiFullName;
        }
        else if (VietnameseTextHelper.TryParseManualEmployeeInput(
                     EmployeeSearchText,
                     out targetCode,
                     out targetDisplayName,
                     out targetAsciiName))
        {
            isManualEntry = true;
        }
        else
        {
            _toast.Show("Chọn nhân viên hoặc nhập: MãNV - Tên không dấu", ToastKind.Warning);
            return;
        }

        if (!float.TryParse(SimilarThresholdText.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float threshold))
        {
            threshold = HasFaceMask ? 0.9994f : 0.9988f;
        }

        byte[]? payloadBytes = null;
        if (IsCameraRunning && _cameraRunner?.LatestFrame is { } latest)
        {
            payloadBytes = FrameEncodingHelper.EncodeBgr24ToJpeg(latest, quality: 92);
        }
        else if (_loadedFileBytes != null)
        {
            payloadBytes = _loadedFileBytes;
        }

        if (payloadBytes == null || payloadBytes.Length == 0)
        {
            _toast.Show("Hãy bật Camera hoặc mở ảnh trước.", ToastKind.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            BridgeResponse extracted = await _bridgeClient.ExtractTemplateAsync(payloadBytes, 5);
            if (!extracted.Success)
            {
                _toast.Show($"Lỗi nhận diện: {extracted.ErrorMessage}", ToastKind.Error, 3500);
                return;
            }

            if (!extracted.FaceDetected || extracted.FaceBox == null || extracted.Template == null ||
                extracted.FullImageJpeg == null || extracted.FaceImageJpeg == null)
            {
                _toast.Show("Không phát hiện khuôn mặt rõ nét. Hãy căn mặt vào khung.", ToastKind.Warning);
                return;
            }

            var insertReq = new FaceRecordInsertRequest
            {
                Code = targetCode,
                Fullname = targetAsciiName,
                Group = SelectedGroup,
                FacePositionXc = extracted.FaceBox.Xc,
                FacePositionYc = extracted.FaceBox.Yc,
                FacePositionW = extracted.FaceBox.W,
                FacePositionAngle = extracted.FaceBox.Angle,
                Eye1X = extracted.Eye1?.X ?? 0,
                Eye1Y = extracted.Eye1?.Y ?? 0,
                Eye2X = extracted.Eye2?.X ?? 0,
                Eye2Y = extracted.Eye2?.Y ?? 0,
                Template = extracted.Template,
                Image = extracted.FullImageJpeg,
                FaceImage = extracted.FaceImageJpeg,
                SimilarThreshold = threshold,
                HasFaceMask = HasFaceMask
            };

            await _faceRepo.InsertFaceRecordAsync(_appState.Settings.FaceDatabase, insertReq);
            _ = _faceRepo.WriteAuditLogAsync(_appState.Settings.FaceDatabase, $"Added user ({targetCode} - {targetAsciiName})");

            if (isManualEntry)
            {
                var manualEmp = _appState.UpsertManualEmployee(targetCode, targetDisplayName, targetAsciiName);
                manualEmp.FaceCount += 1;
                SelectedEmployee = manualEmp;
            }
            else if (SelectedEmployee != null)
            {
                SelectedEmployee.FaceCount += 1;
            }

            await LoadFacesByCodeAsync(targetCode);
            _toast.Show($"Đã lưu khuôn mặt cho {targetCode} - {targetDisplayName}", ToastKind.Success);
        }
        catch (Exception ex)
        {
            _toast.Show($"Lưu thất bại: {ex.Message}", ToastKind.Error, 3500);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteFaceRecordAsync(FaceRecordSummary? record)
    {
        if (record == null)
            return;

        if (MessageBox.Show($"Xóa mẫu khuôn mặt #{record.Id} của {record.Code}?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _faceRepo.DeleteByIdAsync(_appState.Settings.FaceDatabase, record.Id);
            if (SelectedEmployee != null && SelectedEmployee.FaceCount > 0)
                SelectedEmployee.FaceCount -= 1;
            await LoadFacesByCodeAsync(record.Code);
            _toast.Show($"Đã xóa mẫu #{record.Id}", ToastKind.Info);
        }
        catch (Exception ex)
        {
            _toast.Show($"Xóa thất bại: {ex.Message}", ToastKind.Error);
        }
    }

    private async Task LoadFacesByCodeAsync(string? code)
    {
        _faceListLoadCts?.Cancel();
        _faceListLoadCts?.Dispose();
        _faceListLoadCts = new CancellationTokenSource();
        CancellationToken ct = _faceListLoadCts.Token;

        SelectedEmployeeFaces.Clear();
        if (string.IsNullOrWhiteSpace(code) || !_appState.IsFaceConnected)
            return;

        try
        {
            var rows = await _faceRepo.GetFaceSummariesByCodeAsync(_appState.Settings.FaceDatabase, code, ct);
            if (ct.IsCancellationRequested)
                return;

            foreach (var r in rows)
                SelectedEmployeeFaces.Add(r);
        }
        catch (OperationCanceledException) { }
        catch
        {
            // Keep UI smooth if FaceDB is temporarily unreachable.
        }
    }

    private void ClearFaceOverlay()
    {
        HasDetectedFace = false;
        FaceLeft = 0;
        FaceTop = 0;
        FaceSize = 0;
    }

    public async Task ShutdownAsync()
    {
        _loadedFileBytes = null;
        await StopCameraIfRunningAsync();
    }
}
