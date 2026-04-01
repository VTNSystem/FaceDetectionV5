using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FaceDetection
{
    public class FailedDataHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<string> Read()
        {
            List<string> data = new List<string>();
            using (StreamReader reader = new StreamReader(FaceDataHandler.FAILED_DATA_PATH))
            {
                while (!reader.EndOfStream)
                {
                    String line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                        data.Add(line);
                }
            }

            return data;
        }

        public static void Write(List<string> data)
        {
            using (StreamWriter file = new StreamWriter(FaceDataHandler.FAILED_DATA_PATH))
            {
                foreach (string line in data)
                {
                    file.WriteLine(line);
                }
            }
        }

        public static int Sync()
        {
            int count = 0;
            List<string> syncData = Read();
            List<string> failedData = new List<string>();

            if (syncData.Count > 0)
            {
                foreach (string data in syncData)
                {
                    try
                    {
                        string[] s = data.Split(';');
                        // inserting if data is available
                        if (s.Length >= 3)
                        {
                            string code = s[0];
                            DateTime time = DateTime.Parse(s[1]);
                            int mode = Int32.Parse(s[2]);
                            if (DB_TimeLog.InsertData(code, time, mode))
                            {
                                int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] Sync data successfully (code: {code}, date: {time}, inout: {mode})", "Info");
                                if (res == 0)
                                {
                                    log.Info($"Sync data successfully (code: {code}, date: {time}, inout: {mode})");
                                }
                                count++;
                            }
                            else
                            {
                                failedData.Add(data);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        failedData.Add(data);
                        int res = frmMain.SaveLogsToDB($"[{Environment.MachineName}] {ex.ToString()}", "Error");
                        if (res == 0)
                        {
                            log.Error(ex);
                        }
                    }
                }

                Write(failedData);
            }

            return count;
        }
    }
}
