using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SpartonFaceAdmin.Core.Models;
using SpartonFaceAdmin.Infrastructure.Configuration;

namespace SpartonFaceAdmin.App.Services;

public sealed partial class AppStateService : ObservableObject
{
    private readonly DpapiSettingsStore _settingsStore;

    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private bool _isHrConnected;

    [ObservableProperty]
    private bool _isFaceConnected;

    public ObservableCollection<EmployeeItem> Employees { get; } = new();

    public event Action? EmployeesUpdated;

    public AppStateService(DpapiSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _settings = _settingsStore.Load();
    }

    public void SaveSettings() => _settingsStore.Save(Settings);

    public void ReplaceEmployees(IReadOnlyList<EmployeeItem> items)
    {
        Employees.Clear();
        foreach (var item in items)
            Employees.Add(item);
        EmployeesUpdated?.Invoke();
    }
}

public enum ToastKind
{
    Info,
    Success,
    Warning,
    Error
}

public sealed partial class ToastService : ObservableObject
{
    private CancellationTokenSource? _dismissCts;

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private ToastKind _kind = ToastKind.Info;

    public async void Show(string message, ToastKind kind = ToastKind.Info, int durationMs = 2600)
    {
        _dismissCts?.Cancel();
        _dismissCts?.Dispose();
        _dismissCts = new CancellationTokenSource();
        var ct = _dismissCts.Token;

        Message = message;
        Kind = kind;
        IsVisible = true;

        try
        {
            await Task.Delay(durationMs, ct);
            IsVisible = false;
        }
        catch (OperationCanceledException)
        {
            // Replaced by newer toast.
        }
    }

    public void Hide()
    {
        _dismissCts?.Cancel();
        IsVisible = false;
    }
}
