using System.IO;
using ClosedXML.Excel;
using SpartonFaceAdmin.Core.Utilities;

namespace SpartonFaceAdmin.Infrastructure.Export;

public sealed record CardExportItem(int Id, string EmpCode, string EmpFullName, byte[] FullImageBytes);

public sealed class ExcelCardExporter
{
    public async Task<IReadOnlyList<string>> ExportAsync(
        IEnumerable<CardExportItem> items,
        string saveFolder,
        string? excelPath,
        CancellationToken cancellationToken = default)
    {
        var itemList = items.ToList();
        return await Task.Run(() =>
        {
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            var savedPaths = new List<string>(itemList.Count);
            var rowsForExcel = new List<(string EmpCode, string EmpFullName, string ImagePath)>(itemList.Count);

            var duplicateCounter = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in itemList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string baseName = $"{VietnameseTextHelper.SanitizeFileNamePart(item.EmpCode)} - {VietnameseTextHelper.SanitizeFileNamePart(item.EmpFullName)}";
                duplicateCounter.TryGetValue(baseName, out int seenCount);
                duplicateCounter[baseName] = seenCount + 1;

                string fileName = seenCount == 0 && itemList.Count == 1
                    ? $"{baseName}.jpg"
                    : seenCount == 0
                        ? $"{baseName}.jpg"
                        : $"{baseName}_{item.Id}.jpg";

                string fullPath = Path.Combine(saveFolder, fileName);
                File.WriteAllBytes(fullPath, item.FullImageBytes);
                savedPaths.Add(fullPath);
                rowsForExcel.Add((item.EmpCode, item.EmpFullName, fullPath));
            }

            if (!string.IsNullOrWhiteSpace(excelPath) && rowsForExcel.Count > 0)
            {
                AppendRows(excelPath.Trim(), rowsForExcel);
            }

            return (IReadOnlyList<string>)savedPaths;
        }, cancellationToken).ConfigureAwait(false);
    }

    public static void AppendRows(
        string xlsxPath,
        IEnumerable<(string EmpCode, string EmpFullName, string ImagePath)> rows)
    {
        string? dir = Path.GetDirectoryName(xlsxPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var wb = File.Exists(xlsxPath) ? new XLWorkbook(xlsxPath) : new XLWorkbook();
        IXLWorksheet ws = wb.Worksheets.Count == 0
            ? wb.Worksheets.Add("FaceExports")
            : wb.Worksheet(1);

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
        int dataRow;
        if (lastRow == 0)
        {
            ws.Cell(1, 1).Value = "EmpCode";
            ws.Cell(1, 2).Value = "Emp Formatted Name";
            ws.Cell(1, 3).Value = "Emp FullName";
            ws.Cell(1, 4).Value = "ImagePath";
            dataRow = 2;
        }
        else
        {
            dataRow = lastRow + 1;
        }

        foreach (var row in rows)
        {
            string formatted = VietnameseTextHelper.FormatEmpDisplayName(row.EmpFullName);
            string imageFull = string.IsNullOrWhiteSpace(row.ImagePath)
                ? string.Empty
                : Path.GetFullPath(row.ImagePath);

            ws.Cell(dataRow, 1).Value = row.EmpCode ?? string.Empty;
            ws.Cell(dataRow, 2).Value = formatted;
            ws.Cell(dataRow, 3).Value = row.EmpFullName ?? string.Empty;
            ws.Cell(dataRow, 4).Value = imageFull;
            dataRow++;
        }

        wb.SaveAs(xlsxPath);
    }
}
