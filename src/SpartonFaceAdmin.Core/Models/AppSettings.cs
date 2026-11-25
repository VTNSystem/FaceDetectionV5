namespace SpartonFaceAdmin.Core.Models;

public enum CameraSourceType
{
    Usb = 0,
    IpCamera = 1,
    Dcam710 = 2
}

public sealed class DatabaseConnectionSettings
{
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ProtectedPassword { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 3;
}

public sealed class CameraSettings
{
    public CameraSourceType SourceType { get; set; } = CameraSourceType.Usb;
    public int UsbCameraIndex { get; set; }
    public int UsbWidth { get; set; } = 1280;
    public int UsbHeight { get; set; } = 720;
    public string IpCameraUrl { get; set; } = "172.16.64.123/streaming/channels/102/preview";
    public string IpCameraUsername { get; set; } = "admin";
    public string IpCameraProtectedPassword { get; set; } = string.Empty;
    public int IpCameraTimeoutSeconds { get; set; } = 3;
}

public sealed class AppSettings
{
    public DatabaseConnectionSettings HrDatabase { get; set; } = new()
    {
        Server = "10.64.8.60",
        Database = "abriDBHRPro7_Ver3_SPARTON",
        Username = "sa",
        TimeoutSeconds = 3
    };

    public DatabaseConnectionSettings FaceDatabase { get; set; } = new()
    {
        Server = "10.64.8.150",
        Database = "FaceRecognition",
        Username = "Face",
        TimeoutSeconds = 3
    };

    public CameraSettings Camera { get; set; } = new();

    public bool IsFitPreviewScale { get; set; } = true;
    public int PreviewScalePercent { get; set; } = 100;
    public string ExportImageFolder { get; set; } = string.Empty;
    public string ExportExcelPath { get; set; } = string.Empty;
}
