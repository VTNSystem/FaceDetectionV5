using System.Windows;
using System.Windows.Controls;
using SpartonFaceAdmin.App.ViewModels;

namespace SpartonFaceAdmin.App.Views;

public partial class SettingsView : UserControl
{
    private bool _syncing;

    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SettingsViewModel oldVm)
            oldVm.PasswordFieldsReloaded -= SyncPasswordsFromVm;
        if (e.NewValue is SettingsViewModel newVm)
        {
            newVm.PasswordFieldsReloaded += SyncPasswordsFromVm;
            SyncPasswordsFromVm();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => SyncPasswordsFromVm();

    private void SyncPasswordsFromVm()
    {
        if (DataContext is not SettingsViewModel vm)
            return;

        _syncing = true;
        try
        {
            if (PbHrPassword.Password != vm.HrPassword)
                PbHrPassword.Password = vm.HrPassword;
            if (PbFacePassword.Password != vm.FacePassword)
                PbFacePassword.Password = vm.FacePassword;
            if (PbIpCamPassword.Password != vm.IpCameraPassword)
                PbIpCamPassword.Password = vm.IpCameraPassword;
        }
        finally
        {
            _syncing = false;
        }
    }

    private void PbHrPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (!_syncing && DataContext is SettingsViewModel vm)
            vm.HrPassword = PbHrPassword.Password;
    }

    private void PbFacePassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (!_syncing && DataContext is SettingsViewModel vm)
            vm.FacePassword = PbFacePassword.Password;
    }

    private void PbIpCamPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (!_syncing && DataContext is SettingsViewModel vm)
            vm.IpCameraPassword = PbIpCamPassword.Password;
    }
}
