using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpartonFaceAdmin.Core.Models;

public sealed class EmployeeItem : INotifyPropertyChanged
{
    private int _faceCount;

    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AsciiFullName { get; set; } = string.Empty;
    public bool Active { get; set; }
    public bool IsManualOnly { get; set; }

    public int FaceCount
    {
        get => _faceCount;
        set
        {
            if (_faceCount == value)
                return;
            _faceCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayLabel));
        }
    }

    public string DisplayLabel
    {
        get
        {
            string tag = IsManualOnly ? " [Chưa có HRM]" : string.Empty;
            return FaceCount > 0
                ? $"{EmployeeCode} - {FullName}{tag} ({FaceCount})"
                : $"{EmployeeCode} - {FullName}{tag}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public static class UserGroupOption
{
    public const string Office = "Office";
    public const string Operator = "Operator";
    public const string Security = "Security";
    public const string OutsideServices = "Outside Services";

    public static IReadOnlyList<string> All { get; } =
        new[] { Office, Operator, Security, OutsideServices };
}
