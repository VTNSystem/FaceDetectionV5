using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SpartonFaceAdmin.App.Services;
using SpartonFaceAdmin.Core.Models;
using SpartonFaceAdmin.Core.Utilities;
using SpartonFaceAdmin.Infrastructure.Data;
using SpartonFaceAdmin.Infrastructure.Export;

namespace SpartonFaceAdmin.App.ViewModels;

public sealed partial class FaceListViewModel : ObservableObject
{
    private readonly AppStateService _appState;
    private readonly ToastService _toast;
    private readonly FaceRepository _faceRepo;
    private readonly ExcelCardExporter _exporter;

    private CancellationTokenSource? _previewCts;
    private byte[]? _currentFullImageBytes;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private EmployeeItem? _selectedEmployee;

    [ObservableProperty]
    private FaceRecordSummary? _selectedRecord;

    [ObservableProperty]
    private ImageSource? _previewFullImage;

    [ObservableProperty]
    private ImageSource? _previewFaceImage;

    [ObservableProperty]
    private string _saveFolderPath = string.Empty;

    [ObservableProperty]
    private string _excelFilePath = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private IReadOnlyList<EmployeeItem> _filteredEmployees = Array.Empty<EmployeeItem>();

    public ObservableCollection<FaceRecordSummary> FaceRecords { get; } = new();

    public FaceListViewModel(
        AppStateService appState,
        ToastService toast,
        FaceRepository faceRepo,
        ExcelCardExporter exporter)
    {
        _appState = appState;
        _toast = toast;
        _faceRepo = faceRepo;
        _exporter = exporter;

        SaveFolderPath = _appState.Settings.ExportImageFolder;
        ExcelFilePath = _appState.Settings.ExportExcelPath;

        _appState.EmployeesUpdated += OnEmployeesUpdated;
        ApplyFilter();
    }

    private void OnEmployeesUpdated()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ApplyFilter();
            if (SelectedEmployee != null)
            {
                _ = LoadEmployeeFacesAsync(SelectedEmployee);
            }
            else
            {
                FaceRecords.Clear();
                ClearPreviews();
            }
        });
    }

    public async Task OnNavigatedToAsync()
    {
        ApplyFilter();
        if (SelectedEmployee != null)
            await LoadEmployeeFacesAsync(SelectedEmployee);
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedEmployeeChanged(EmployeeItem? value)
    {
        _ = LoadEmployeeFacesAsync(value);
    }

    partial void OnSelectedRecordChanged(FaceRecordSummary? value)
    {
        _ = LoadRecordImagesAsync(value);
    }

    partial void OnSaveFolderPathChanged(string value)
    {
        _appState.Settings.ExportImageFolder = value?.Trim() ?? string.Empty;
        _appState.SaveSettings();
    }

    partial void OnExcelFilePathChanged(string value)
    {
        _appState.Settings.ExportExcelPath = value?.Trim() ?? string.Empty;
        _appState.SaveSettings();
    }

    private void ApplyFilter()
    {
        string? prevCode = SelectedEmployee?.EmployeeCode;
        string q = (SearchText ?? string.Empty).Trim();
        string qAscii = VietnameseTextHelper.VniToEng(q);

        IEnumerable<EmployeeItem> source = _appState.Employees;
        if (!string.IsNullOrEmpty(q))
        {
            source = source.Where(e =>
                e.EmployeeCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.AsciiFullName.Contains(qAscii, StringComparison.OrdinalIgnoreCase));
        }

        var updated = source.ToList();
        FilteredEmployees = updated;

        if (!string.IsNullOrEmpty(prevCode))
        {
            SelectedEmployee = updated.FirstOrDefault(x =>
                string.Equals(x.EmployeeCode, prevCode, StringComparison.OrdinalIgnoreCase));
        }
    }

    private async Task LoadEmployeeFacesAsync(EmployeeItem? employee)
    {
        FaceRecords.Clear();
        ClearPreviews();

        if (employee == null)
            return;

        IsLoading = true;
        try
        {
            var rows = await _faceRepo.GetFaceSummariesByCodeAsync(_appState.Settings.FaceDatabase, employee.EmployeeCode);
            foreach (var r in rows)
                FaceRecords.Add(r);

            SelectedRecord = FaceRecords.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _toast.Show($"Không tải được danh sách ảnh: {ex.Message}", ToastKind.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRecordImagesAsync(FaceRecordSummary? record)
    {
        _previewCts?.Cancel();
        _previewCts?.Dispose();
        _previewCts = new CancellationTokenSource();
        CancellationToken ct = _previewCts.Token;

        ClearPreviews();
        if (record == null || !record.HasStoredImage)
            return;

        try
        {
            FaceRecordImages? images = await _faceRepo.GetFaceImagesByIdAsync(_appState.Settings.FaceDatabase, record.Id, ct);
            if (images == null || ct.IsCancellationRequested)
                return;

            _currentFullImageBytes = images.FullImageBytes;
            PreviewFullImage = DecodeImage(images.FullImageBytes);
            PreviewFaceImage = DecodeImage(images.FaceImageBytes);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _toast.Show($"Không tải được ảnh xem trước: {ex.Message}", ToastKind.Warning);
        }
    }

    private void ClearPreviews()
    {
        _currentFullImageBytes = null;
        PreviewFullImage = null;
        PreviewFaceImage = null;
    }

    private static BitmapImage? DecodeImage(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        using var ms = new MemoryStream(bytes);
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    [RelayCommand]
    private async Task DeleteSingleAsync(FaceRecordSummary? record)
    {
        record ??= SelectedRecord;
        if (record == null)
            return;

        if (MessageBox.Show($"Xóa mẫu khuôn mặt #{record.Id} ({record.Code} - {record.Fullname})?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _faceRepo.DeleteByIdAsync(_appState.Settings.FaceDatabase, record.Id);
            if (SelectedEmployee != null && SelectedEmployee.FaceCount > 0)
                SelectedEmployee.FaceCount -= 1;
            await LoadEmployeeFacesAsync(SelectedEmployee);
            _toast.Show($"Đã xóa mẫu #{record.Id}", ToastKind.Info);
        }
        catch (Exception ex)
        {
            _toast.Show($"Xóa thất bại: {ex.Message}", ToastKind.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteAllForEmployeeAsync()
    {
        if (SelectedEmployee == null)
            return;

        var emp = SelectedEmployee;
        if (MessageBox.Show($"Xóa TẤT CẢ dữ liệu khuôn mặt của {emp.EmployeeCode} - {emp.FullName}?",
                "Xác nhận xóa tất cả", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _faceRepo.DeleteByCodeAsync(_appState.Settings.FaceDatabase, emp.EmployeeCode);
            _ = _faceRepo.WriteAuditLogAsync(_appState.Settings.FaceDatabase, $"Removed user ({emp.EmployeeCode} - {emp.FullName})");
            emp.FaceCount = 0;
            await LoadEmployeeFacesAsync(emp);
            _toast.Show($"Đã xóa toàn bộ ảnh của {emp.EmployeeCode}", ToastKind.Info);
        }
        catch (Exception ex)
        {
            _toast.Show($"Xóa thất bại: {ex.Message}", ToastKind.Error);
        }
    }

    [RelayCommand]
    private void BrowseSaveFolder()
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Chọn thư mục lưu ảnh",
            InitialDirectory = Directory.Exists(SaveFolderPath) ? SaveFolderPath : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };
        if (dlg.ShowDialog() == true)
            SaveFolderPath = dlg.FolderName;
    }

    [RelayCommand]
    private void BrowseExcelFile()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Chọn file Excel (.xlsx)",
            Filter = "Excel workbook (*.xlsx)|*.xlsx",
            OverwritePrompt = false,
            FileName = string.IsNullOrWhiteSpace(ExcelFilePath) ? "FaceExports.xlsx" : ExcelFilePath
        };
        if (dlg.ShowDialog() == true)
            ExcelFilePath = dlg.FileName;
    }

    [RelayCommand]
    private async Task ExportSelectedAsync()
    {
        if (SelectedRecord == null)
        {
            _toast.Show("Hãy chọn một dòng trong bảng.", ToastKind.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(SaveFolderPath))
        {
            _toast.Show("Hãy chọn thư mục lưu ảnh.", ToastKind.Warning);
            return;
        }

        byte[]? bytes = _currentFullImageBytes;
        if (bytes == null)
        {
            var imgs = await _faceRepo.GetFaceImagesByIdAsync(_appState.Settings.FaceDatabase, SelectedRecord.Id);
            bytes = imgs?.FullImageBytes;
        }

        if (bytes == null || bytes.Length == 0)
        {
            _toast.Show("Bản ghi này không có ảnh gốc để lưu.", ToastKind.Warning);
            return;
        }

        string empFullName = SelectedEmployee?.FullName ?? SelectedRecord.Fullname;
        await RunExportAsync(new[]
        {
            new CardExportItem(SelectedRecord.Id, SelectedRecord.Code, empFullName, bytes)
        });
    }

    [RelayCommand]
    private async Task ExportAllOfEmployeeAsync()
    {
        if (FaceRecords.Count == 0)
        {
            _toast.Show("Nhân viên chưa có dữ liệu ảnh.", ToastKind.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(SaveFolderPath))
        {
            _toast.Show("Hãy chọn thư mục lưu ảnh.", ToastKind.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var items = new List<CardExportItem>();
            string empFullName = SelectedEmployee?.FullName ?? FaceRecords[0].Fullname;
            foreach (var r in FaceRecords)
            {
                if (!r.HasStoredImage)
                    continue;
                var imgs = await _faceRepo.GetFaceImagesByIdAsync(_appState.Settings.FaceDatabase, r.Id);
                if (imgs?.FullImageBytes is { Length: > 0 } fullBytes)
                    items.Add(new CardExportItem(r.Id, r.Code, empFullName, fullBytes));
            }

            if (items.Count == 0)
            {
                _toast.Show("Không có bản ghi nào chứa ảnh gốc.", ToastKind.Warning);
                return;
            }

            await RunExportAsync(items);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RunExportAsync(IReadOnlyList<CardExportItem> items)
    {
        try
        {
            var saved = await _exporter.ExportAsync(items, SaveFolderPath.Trim(), ExcelFilePath);
            string excelNote = string.IsNullOrWhiteSpace(ExcelFilePath) ? "" : " & ghi Excel";
            _toast.Show($"Đã xuất {saved.Count} ảnh{excelNote}.", ToastKind.Success);
        }
        catch (Exception ex)
        {
            _toast.Show($"Xuất thất bại: {ex.Message}", ToastKind.Error, 3500);
        }
    }
}
