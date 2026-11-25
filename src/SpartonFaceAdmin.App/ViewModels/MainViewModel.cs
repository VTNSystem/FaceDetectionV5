using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpartonFaceAdmin.App.Services;
using SpartonFaceAdmin.Core.Models;
using SpartonFaceAdmin.Core.Utilities;
using SpartonFaceAdmin.Infrastructure.Data;

namespace SpartonFaceAdmin.App.ViewModels;

public enum NavSection
{
    Enrollment = 0,
    FaceList = 1,
    Settings = 2
}

public sealed partial class MainViewModel : ObservableObject
{
    private readonly AppStateService _appState;
    private readonly HrRepository _hrRepo;
    private readonly FaceRepository _faceRepo;

    [ObservableProperty]
    private NavSection _activeSection = NavSection.Enrollment;

    [ObservableProperty]
    private object _currentViewModel;

    public EnrollmentViewModel EnrollmentVm { get; }
    public FaceListViewModel FaceListVm { get; }
    public SettingsViewModel SettingsVm { get; }
    public ToastService Toast { get; }
    public AppStateService AppState => _appState;

    public MainViewModel(
        AppStateService appState,
        ToastService toast,
        HrRepository hrRepo,
        FaceRepository faceRepo,
        EnrollmentViewModel enrollmentVm,
        FaceListViewModel faceListVm,
        SettingsViewModel settingsVm)
    {
        _appState = appState;
        Toast = toast;
        _hrRepo = hrRepo;
        _faceRepo = faceRepo;
        EnrollmentVm = enrollmentVm;
        FaceListVm = faceListVm;
        SettingsVm = settingsVm;
        _currentViewModel = enrollmentVm;

        _appState.SettingsSavedAsync += RefreshEmployeesAndHealthAsync;
    }

    public async Task InitializeAsync()
    {
        await RefreshEmployeesAndHealthAsync();
    }

    [RelayCommand]
    private async Task NavigateAsync(NavSection section)
    {
        if (ActiveSection == section)
            return;

        if (ActiveSection == NavSection.Enrollment && section != NavSection.Enrollment)
        {
            await EnrollmentVm.StopCameraIfRunningAsync();
        }

        ActiveSection = section;
        CurrentViewModel = section switch
        {
            NavSection.Enrollment => EnrollmentVm,
            NavSection.FaceList => FaceListVm,
            NavSection.Settings => SettingsVm,
            _ => EnrollmentVm
        };

        if (section == NavSection.FaceList)
            await FaceListVm.OnNavigatedToAsync();
        else if (section == NavSection.Settings)
            SettingsVm.ReloadFromAppState();
    }

    [RelayCommand]
    public async Task RefreshEmployeesAndHealthAsync()
    {
        var hrTask = SqlConnectionFactory.TestConnectionAsync(_appState.Settings.HrDatabase);
        var faceTask = SqlConnectionFactory.TestConnectionAsync(_appState.Settings.FaceDatabase);

        var (hrOk, _) = await hrTask;
        var (faceOk, _) = await faceTask;

        _appState.IsHrConnected = hrOk;
        _appState.IsFaceConnected = faceOk;

        var merged = new List<EmployeeItem>();

        try
        {
            Dictionary<string, FaceRepository.FaceCodeAggregate>? aggregates = null;
            if (faceOk)
            {
                aggregates = await _faceRepo.GetEmployeeFaceAggregatesAsync(_appState.Settings.FaceDatabase);
            }

            if (hrOk)
            {
                var hrEmployees = await _hrRepo.GetEmployeesAsync(_appState.Settings.HrDatabase, activeOnly: true);
                var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var emp in hrEmployees)
                {
                    seenCodes.Add(emp.EmployeeCode);
                    if (aggregates != null && aggregates.TryGetValue(emp.EmployeeCode, out var agg))
                        emp.FaceCount = agg.Total;
                    merged.Add(emp);
                }

                if (aggregates != null)
                {
                    foreach (var agg in aggregates.Values.OrderBy(a => a.Code, StringComparer.OrdinalIgnoreCase))
                    {
                        if (seenCodes.Contains(agg.Code))
                            continue;

                        merged.Add(new EmployeeItem
                        {
                            EmployeeCode = agg.Code,
                            FullName = agg.Fullname,
                            AsciiFullName = VietnameseTextHelper.VniToEng(agg.Fullname),
                            Active = true,
                            IsManualOnly = true,
                            FaceCount = agg.Total
                        });
                    }
                }
            }
            else if (aggregates != null)
            {
                foreach (var agg in aggregates.Values.OrderBy(a => a.Code, StringComparer.OrdinalIgnoreCase))
                {
                    merged.Add(new EmployeeItem
                    {
                        EmployeeCode = agg.Code,
                        FullName = agg.Fullname,
                        AsciiFullName = VietnameseTextHelper.VniToEng(agg.Fullname),
                        Active = true,
                        IsManualOnly = true,
                        FaceCount = agg.Total
                    });
                }
            }

            _appState.ReplaceEmployees(merged);
        }
        catch (Exception ex)
        {
            _appState.ReplaceEmployees(Array.Empty<EmployeeItem>());
            Toast.Show($"Không tải được danh sách nhân viên: {ex.Message}", ToastKind.Warning);
        }
    }
}
