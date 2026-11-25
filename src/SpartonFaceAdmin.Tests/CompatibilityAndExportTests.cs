using ClosedXML.Excel;
using SpartonFaceAdmin.Core.Utilities;
using SpartonFaceAdmin.Infrastructure.Configuration;
using SpartonFaceAdmin.Infrastructure.Export;
using Xunit;

namespace SpartonFaceAdmin.Tests;

public sealed class CompatibilityAndExportTests
{
    [Fact]
    public void VniToEng_PreservesLegacyMapping()
    {
        Assert.Equal("Nguyen Van Dong", VietnameseTextHelper.VniToEng("Nguyễn Văn Đông"));
        Assert.Equal("Tran Minh Trung", VietnameseTextHelper.VniToEng("Trần Minh Trung"));
        Assert.Equal("Do Thi Dao", VietnameseTextHelper.VniToEng("Đỗ Thị Đào"));
    }

    [Fact]
    public void FormatEmpDisplayName_MatchesLegacyExcelLog()
    {
        Assert.Equal("Thien Bui", VietnameseTextHelper.FormatEmpDisplayName("Bui Hong Thanh Thien"));
        Assert.Equal("Quyet Vu", VietnameseTextHelper.FormatEmpDisplayName("Vu Van Quyet"));
        Assert.Equal("Admin", VietnameseTextHelper.FormatEmpDisplayName("Admin"));
    }

    [Fact]
    public void DpapiSettingsStore_RoundTripsSecretsSafely()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"sparton_cfg_{Guid.NewGuid():N}.json");
        try
        {
            var store = new DpapiSettingsStore(tempFile);
            var settings = store.Load();
            settings.HrDatabase.ProtectedPassword = DpapiSettingsStore.ProtectSecret("S3cret!@#");
            settings.PreviewScalePercent = 125;
            settings.IsFitPreviewScale = false;
            store.Save(settings);

            var reloaded = store.Load();
            Assert.Equal("S3cret!@#", DpapiSettingsStore.UnprotectSecret(reloaded.HrDatabase.ProtectedPassword));
            Assert.Equal(125, reloaded.PreviewScalePercent);
            Assert.False(reloaded.IsFitPreviewScale);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ExcelCardExporter_WritesExpectedColumnsAndImageFile()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"sparton_exp_{Guid.NewGuid():N}");
        string excelPath = Path.Combine(tempDir, "cards.xlsx");
        try
        {
            var exporter = new ExcelCardExporter();
            byte[] fakeJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
            var items = new[] { new CardExportItem(101, "0525", "Bui Hong Thanh Thien", fakeJpeg) };

            var saved = await exporter.ExportAsync(items, tempDir, excelPath);
            Assert.Single(saved);
            Assert.True(File.Exists(saved[0]));
            Assert.Equal("0525 - Bui Hong Thanh Thien.jpg", Path.GetFileName(saved[0]));

            using var wb = new XLWorkbook(excelPath);
            var ws = wb.Worksheet(1);
            Assert.Equal("EmpCode", ws.Cell(1, 1).GetString());
            Assert.Equal("Emp Formatted Name", ws.Cell(1, 2).GetString());
            Assert.Equal("Emp FullName", ws.Cell(1, 3).GetString());
            Assert.Equal("ImagePath", ws.Cell(1, 4).GetString());

            Assert.Equal("0525", ws.Cell(2, 1).GetString());
            Assert.Equal("Thien Bui", ws.Cell(2, 2).GetString());
            Assert.Equal("Bui Hong Thanh Thien", ws.Cell(2, 3).GetString());
            Assert.Equal(Path.GetFullPath(saved[0]), ws.Cell(2, 4).GetString());
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
