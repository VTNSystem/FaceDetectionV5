using System;
using System.IO;
using ClosedXML.Excel;

namespace FaceDetection
{
    /// <summary>
    /// Appends rows to an .xlsx log (e.g. Vu Van Quyet → formatted "Quyet Vu").
    /// </summary>
    public static class FaceListExcelLog
    {
        /// <summary>
        /// Last word + space + first word (e.g. "Bui Hong Thanh Thien" → "Thien Bui").
        /// </summary>
        public static string FormatEmpDisplayName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;
            string[] parts = fullName.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return string.Empty;
            if (parts.Length == 1)
                return parts[0];
            return parts[parts.Length - 1] + " " + parts[0];
        }

        /// <summary>
        /// Columns A–D: EmpCode, Emp Formatted Name, Emp FullName, ImagePath. Creates file and header row if needed.
        /// </summary>
        public static void AppendRow(string xlsxPath, string empCode, string empFullName, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(xlsxPath))
                return;

            xlsxPath = xlsxPath.Trim();
            string dir = Path.GetDirectoryName(xlsxPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string formatted = FormatEmpDisplayName(empFullName);
            string imageFull = string.IsNullOrWhiteSpace(imagePath) ? string.Empty : Path.GetFullPath(imagePath);

            using (var wb = File.Exists(xlsxPath) ? new XLWorkbook(xlsxPath) : new XLWorkbook())
            {
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

                ws.Cell(dataRow, 1).Value = empCode ?? string.Empty;
                ws.Cell(dataRow, 2).Value = formatted;
                ws.Cell(dataRow, 3).Value = empFullName ?? string.Empty;
                ws.Cell(dataRow, 4).Value = imageFull;

                wb.SaveAs(xlsxPath);
            }
        }
    }
}
