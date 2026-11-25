using System.Globalization;
using System.Text;

namespace SpartonFaceAdmin.Core.Utilities;

public static class VietnameseTextHelper
{
    /// <summary>
    /// Converts Vietnamese accented text to ASCII while preserving legacy V5 mapping for FaceList.Fullname.
    /// </summary>
    public static string VniToEng(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string decomposed = text.Normalize(NormalizationForm.FormKD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c switch
                {
                    'đ' => 'd',
                    'Đ' => 'D',
                    _ => c
                });
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Formats employee full name as "LastWord FirstWord" (e.g. "Bui Hong Thanh Thien" -> "Thien Bui").
    /// </summary>
    public static string FormatEmpDisplayName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;

        string[] parts = fullName.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return string.Empty;
        if (parts.Length == 1)
            return parts[0];
        return $"{parts[^1]} {parts[0]}";
    }

    public static string SanitizeFileNamePart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unknown";

        char[] invalid = Path.GetInvalidFileNameChars();
        string trimmed = value.Trim();
        var sb = new StringBuilder(trimmed.Length);
        foreach (char c in trimmed)
        {
            sb.Append(Array.IndexOf(invalid, c) >= 0 || c < 32 ? '_' : c);
        }

        string cleaned = sb.ToString().Trim();
        return string.IsNullOrEmpty(cleaned) ? "Unknown" : cleaned;
    }

    /// <summary>
    /// Parses manual employee input in legacy format "Code - Name" (e.g. "0999 - Nguyen Van A").
    /// </summary>
    public static bool TryParseManualEmployeeInput(
        string? rawInput,
        out string code,
        out string displayName,
        out string asciiFullName)
    {
        code = string.Empty;
        displayName = string.Empty;
        asciiFullName = string.Empty;

        if (string.IsNullOrWhiteSpace(rawInput))
            return false;

        int sepIdx = rawInput.IndexOf('-');
        if (sepIdx <= 0 || sepIdx >= rawInput.Length - 1)
            return false;

        string parsedCode = rawInput[..sepIdx].Trim();
        string parsedName = rawInput[(sepIdx + 1)..].Trim();
        string ascii = VniToEng(parsedName);

        if (string.IsNullOrWhiteSpace(parsedCode) || string.IsNullOrWhiteSpace(ascii))
            return false;

        code = parsedCode;
        displayName = parsedName;
        asciiFullName = ascii;
        return true;
    }
}
