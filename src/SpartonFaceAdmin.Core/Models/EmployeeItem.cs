namespace SpartonFaceAdmin.Core.Models;

public sealed class EmployeeItem
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AsciiFullName { get; set; } = string.Empty;
    public bool Active { get; set; }
    public int FaceCount { get; set; }

    public string DisplayLabel =>
        FaceCount > 0
            ? $"{EmployeeCode} - {FullName} ({FaceCount})"
            : $"{EmployeeCode} - {FullName}";
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
