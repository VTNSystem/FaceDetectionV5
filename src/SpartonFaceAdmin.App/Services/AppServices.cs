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
    public event Func<Task>? SettingsSavedAsync;

    public AppStateService(DpapiSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _settings = _settingsStore.Load();
    }

    public void SaveSettings() => _settingsStore.Save(Settings);

    public async Task NotifySettingsSavedAsync()
    {
        if (SettingsSavedAsync != null)
        {
            foreach (Func<Task> handler in SettingsSavedAsync.GetInvocationList().Cast<Func<Task>>())
            {
                await handler();
            }
        }
    }

    public void ReplaceEmployees(IReadOnlyList<EmployeeItem> items)
    {
        Employees.Clear();
        foreach (var item in items)
            Employees.Add(item);
        EmployeesUpdated?.Invoke();
    }

    public EmployeeItem UpsertManualEmployee(string code, string displayName, string asciiFullName)
    {
        var existing = Employees.FirstOrDefault(e =>
            string.Equals(e.EmployeeCode, code, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            return existing;

        var added = new EmployeeItem
        {
            EmployeeCode = code,
            FullName = displayName,
            AsciiFullName = asciiFullName,
            Active = true,
            IsManualOnly = true,
            FaceCount = 0
        };
        Employees.Insert(0, added);
        EmployeesUpdated?.Invoke();
        return added;
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
