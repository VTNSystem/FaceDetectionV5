using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpartonFaceAdmin.App.Services;
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

        if (!hrOk)
            return;

        try
        {
            var employees = await _hrRepo.GetEmployeesAsync(_appState.Settings.HrDatabase, activeOnly: true);
            if (faceOk)
            {
                var counts = await _faceRepo.GetEmployeeFaceCountsAsync(_appState.Settings.FaceDatabase);
                foreach (var emp in employees)
                {
                    if (counts.TryGetValue(emp.EmployeeCode, out int c))
                        emp.FaceCount = c;
                }
            }
            _appState.ReplaceEmployees(employees);
        }
        catch (Exception ex)
        {
            Toast.Show($"Không tải được danh sách nhân viên: {ex.Message}", ToastKind.Warning);
        }
    }
}
