using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceDetection
{
    public class DB_TimeLog
    {
        private static string ConnectionString;
        public static bool Connected { get; private set; }

        public static bool CheckConnection(string server, string database, string user, string password, int timeout)
        {
            ConnectionString = $"Server={server};Database={database};User Id={user};Password={password};Connection Timeout={timeout}";
            Connected = false;
            
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    Connected = true;
                }
                catch { }
                finally
                {
                    if (connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }
            }
            return Connected;
        }

        private static string VniToEng(string text)
        {
            return new string(
                text
                .Normalize(System.Text.NormalizationForm.FormKD)
                .ToCharArray()
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray())
                .Replace("đ", "d").Replace("Đ", "D");
        }

        public static List<Models.User> GetUsers()
        {
            List<Models.User> users = new List<Models.User>();
            if (Connected)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = @"SELECT [EmployeeCode], [LastName] + ' ' + [MidName] + ' ' + [FirstName] AS 'FullName',
[Gender], [JoinedDate], CONVERT(date, CONVERT(varchar, [YearOfBirth]) + '/' + CONVERT(varchar, [MonthOfBirth]) + '/' + CONVERT(varchar, [DayOfBirth])) AS [DateOfBirth], [Active]
FROM [dbo].[HR_Employee]";
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Models.User user = new Models.User();
                                user.Username = reader.GetString(0);
                                user.FullName = reader.GetString(1);
                                user.NickName = VniToEng(user.FullName);
                                user.Gender = reader.GetBoolean(2);
                                user.JoinedDate = reader.GetDateTime(3);
                                user.DateOfBirth = reader.GetDateTime(4);
                                user.Active = reader.GetBoolean(5);
                                users.Add(user);
                            }
                            reader.Close();
                        }
                        connection.Close();
                    }
                }
                catch(Exception ex) { }
            }
            
            return users;
        }

        public static System.Data.DataTable GetRollCall(DateTime date, string code)
        {
            System.Data.DataTable table = new System.Data.DataTable();
            if (Connected)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter())
                        {
                            SqlCommand command = new SqlCommand();
                            command.Connection = connection;
                            command.CommandText = @"SELECT DISTINCT [Date], [Employee code], [FullName], 
	(SELECT MIN([Time]) FROM [dbo].[TA_TimeLog] WHERE [EmployeeATID] = [Employee code] AND CONVERT(date, [Time]) = @Date AND InOutMode = 1) AS 'In',
	(SELECT MAX([Time]) FROM [dbo].[TA_TimeLog] WHERE [EmployeeATID] = [Employee code] AND CONVERT(date, [Time]) = @Date AND InOutMode = 2) AS 'Out'
FROM (
	SELECT CONVERT(date, [Time]) AS [Date], T.[EmployeeATID] AS [Employee code], E.[LastName] + ' ' + E.[MidName] + ' ' + E.[FirstName] AS [FullName], [Time]
	FROM [dbo].[TA_TimeLog] T JOIN [dbo].[HR_Employee] E ON T.[EmployeeATID] = E.[EmployeeCode]
	WHERE CONVERT(date, [Time]) = @Date AND (@Code = '' OR T.[EmployeeATID] = @Code)
) AS [TimeLog]";
                            command.Parameters.Add("@Date", System.Data.SqlDbType.Date).Value = date;
                            command.Parameters.Add("@Code", System.Data.SqlDbType.VarChar).Value = code;
                            adapter.SelectCommand = command;
                            adapter.Fill(table);
                        }
                        connection.Close();
                    }
                }
                catch { }
            }

            return table;
        }

        public static bool InsertData(string id, DateTime datetime, int inOut)
        {
            bool success = false;
            if (Connected)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO TA_TimeLog (EmployeeATID, Time, MachineSerial, InOutMode, SpecifiedMode, Action, UpdatedDate, UpdatedUser, CompanyIndex) " +
                            $"VALUES ('{id}', '{datetime.ToString()}', '{Environment.MachineName}', {inOut}, 0, 'ADD', '{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}', '', 1)";
                        connection.Open();
                        int affectedRows = command.ExecuteNonQuery();
                        success = affectedRows > 0;
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return success;
        }
    }
}
