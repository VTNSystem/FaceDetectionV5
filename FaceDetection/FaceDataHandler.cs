using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FaceDetection
{
    public static class FaceDataHandler
    {
        //public const string LUXAND_LICENSE = "pW5Onb56TjN0MYYFwfMiaB27FjRz5H50BcZajvqlGQy9UFluuwlmwWwBfvNyiqDRltmCu8lvUA0IVew5EcMgiqK+A8woTSQVr/ycifUqVCclkJn3WbSJ/V61apeSUjEnxmYyrKSFOZoiYCkfNZRpMh9a3Pl2MpXPqeTOUwqkjhs=";
        public const string LUXAND_LICENSE = "LImcnZzkzw8RrawhS1F1kirLGcLBEC5xWkpNWBFXCnSiOYEzzcbTSJ0M5NqD7e6Ai7KCJi4g63nKFPVyxhRHq0vwGN/d+bqXummVuocUNIzUO6EWAhlMUv/dtztWSAMNzi0RjrZm0XOJzq0ukos9ZQT4L+aiGm1mKjDEKuPETBk=";
        public const string FAILED_DATA_PATH = "logs/failed_data.txt";
        public const string PREFIX = "<$~!#_";
        public const string SUFFIX = "_#!~$>";

        public static string GetTempDataFilePath(string dataPath, string fileName = "tracker_temp.dat")
        {
            string filePath = Path.Combine("data", fileName);
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");

            try
            {
                // backup data file
                if (File.Exists(dataPath))
                {
                    bool needCopy = true;
                    if (File.Exists(filePath))
                    {
                        FileInfo fileInfo = new FileInfo(dataPath);
                        FileInfo tempInfo = new FileInfo(filePath);
                        needCopy = fileInfo.Length != tempInfo.Length;
                    }

                    // copy a new if file is modifed
                    if (needCopy)
                    {
                        // copy a data file to temp folder
                        File.Copy(dataPath, filePath, true);
                    }
                }
            }
            catch { }

            return filePath;
        }

        public static string GetReportPath()
        {
            string folder = Properties.Settings.Default.ReportPath;
            if (folder == null || !Directory.Exists(folder))
                folder = "Reports";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        public static string GetMultipleParameter(bool detectExpression = false, bool recognizeFaces = true, bool learning = true)
        {
            int detectionThreshold = Properties.Settings.Default.DetectionThreshold;
            float threshold = Properties.Settings.Default.Threshold;
            int memoryLimit = Properties.Settings.Default.MemoryLimit;
            string parameters = $"HandleArbitraryRotations=false; DetermineFaceRotationAngle=true; InternalResizeWidth=100; FaceDetectionThreshold={detectionThreshold}; ";
            parameters += $"RecognizeFaces={recognizeFaces.ToString().ToLower()}; DetectExpression={detectExpression.ToString().ToLower()}; Learning={learning.ToString().ToLower()}; ";
            parameters += $"Threshold={threshold}; ThresholdFeed=0.998; MemoryLimit={memoryLimit}";
            return parameters;
        }

        public static string SetIdentifier(string name)
        {
            return $"{PREFIX}{name}{SUFFIX}";
        }

        public static string GetNameFromIdentifier(string identifier)
        {
            return identifier.Replace(PREFIX, "").Replace(SUFFIX, "");
        }
    }
}
