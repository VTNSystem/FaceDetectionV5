using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using SpartonFaceAdmin.Core.Models;

namespace SpartonFaceAdmin.Infrastructure.Configuration;

public sealed class DpapiSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _settingsFilePath;

    public DpapiSettingsStore(string? settingsFilePath = null)
    {
        string appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SpartonFaceAdmin");
        _settingsFilePath = settingsFilePath ?? Path.Combine(appDataDir, "appsettings.json");
    }

    public AppSettings Load()
    {
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                string json = File.ReadAllText(_settingsFilePath, Encoding.UTF8);
                AppSettings? loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (loaded != null)
                    return loaded;
            }
            catch
            {
                // Fall back to default/legacy import if config is corrupted.
            }
        }

        var defaults = new AppSettings();
        TrySeedFromLegacyAppConfig(defaults);
        return defaults;
    }

    public void Save(AppSettings settings)
    {
        string? dir = Path.GetDirectoryName(_settingsFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsFilePath, json, Encoding.UTF8);
    }

    public static string ProtectSecret(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        byte[] protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public static string UnprotectSecret(string? cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            return string.Empty;

        try
        {
            byte[] protectedBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void TrySeedFromLegacyAppConfig(AppSettings settings)
    {
        string[] candidates =
        {
            Path.Combine(AppContext.BaseDirectory, "FaceDetection.exe.config"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "FaceDetection", "App.config"))
        };

        string? legacyConfig = candidates.FirstOrDefault(File.Exists);
        if (legacyConfig == null)
            return;

        try
        {
            XDocument doc = XDocument.Load(legacyConfig);
            var map = doc.Descendants("setting")
                .Where(e => e.Attribute("name") != null)
                .ToDictionary(
                    e => e.Attribute("name")!.Value,
                    e => e.Element("value")?.Value ?? string.Empty);

            if (map.TryGetValue("Server", out string? hrServer) && !string.IsNullOrWhiteSpace(hrServer))
                settings.HrDatabase.Server = hrServer;
            if (map.TryGetValue("Database", out string? hrDb) && !string.IsNullOrWhiteSpace(hrDb))
                settings.HrDatabase.Database = hrDb;
            if (map.TryGetValue("Username", out string? hrUser) && !string.IsNullOrWhiteSpace(hrUser))
                settings.HrDatabase.Username = hrUser;
            if (map.TryGetValue("Password", out string? hrPwdB64) && !string.IsNullOrWhiteSpace(hrPwdB64))
                settings.HrDatabase.ProtectedPassword = ProtectSecret(DecodeLegacyBase64(hrPwdB64));
            if (map.TryGetValue("DatabaseTimeout", out string? hrTo) && int.TryParse(hrTo, out int hrTimeout))
                settings.HrDatabase.TimeoutSeconds = Math.Max(1, hrTimeout);

            if (map.TryGetValue("FaceServer", out string? faceServer) && !string.IsNullOrWhiteSpace(faceServer))
                settings.FaceDatabase.Server = faceServer;
            if (map.TryGetValue("FaceDatabase", out string? faceDb) && !string.IsNullOrWhiteSpace(faceDb))
                settings.FaceDatabase.Database = faceDb;
            if (map.TryGetValue("FaceUsername", out string? faceUser) && !string.IsNullOrWhiteSpace(faceUser))
                settings.FaceDatabase.Username = faceUser;
            if (map.TryGetValue("FacePassword", out string? facePwdB64) && !string.IsNullOrWhiteSpace(facePwdB64))
                settings.FaceDatabase.ProtectedPassword = ProtectSecret(DecodeLegacyBase64(facePwdB64));
            if (map.TryGetValue("FaceDatabaseTimeout", out string? faceTo) && int.TryParse(faceTo, out int faceTimeout))
                settings.FaceDatabase.TimeoutSeconds = Math.Max(1, faceTimeout);

            if (map.TryGetValue("UseDCAM710", out string? useDcam) && bool.TryParse(useDcam, out bool isDcam) && isDcam)
                settings.Camera.SourceType = CameraSourceType.Dcam710;
            else if (map.TryGetValue("UseCameraIP", out string? useIp) && bool.TryParse(useIp, out bool isIp) && isIp)
                settings.Camera.SourceType = CameraSourceType.IpCamera;

            if (map.TryGetValue("AttachedCamera", out string? camIdx) && int.TryParse(camIdx, out int idx))
                settings.Camera.UsbCameraIndex = Math.Max(0, idx);
            if (map.TryGetValue("CameraIP_URL", out string? ipUrl) && !string.IsNullOrWhiteSpace(ipUrl))
                settings.Camera.IpCameraUrl = ipUrl;
            if (map.TryGetValue("CameraIP_Username", out string? ipUser) && !string.IsNullOrWhiteSpace(ipUser))
                settings.Camera.IpCameraUsername = ipUser;
            if (map.TryGetValue("CameraIP_Password", out string? ipPwd) && !string.IsNullOrWhiteSpace(ipPwd))
                settings.Camera.IpCameraProtectedPassword = ProtectSecret(ipPwd);
            if (map.TryGetValue("CameraIP_Timeout", out string? ipTo) && int.TryParse(ipTo, out int ipTimeout))
                settings.Camera.IpCameraTimeoutSeconds = Math.Max(1, ipTimeout);
        }
        catch
        {
            // Ignore malformed legacy config and keep built-in defaults.
        }
    }

    private static string DecodeLegacyBase64(string value)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch
        {
            return string.Empty;
        }
    }
}
