using Dapper;
using Microsoft.Data.SqlClient;
using SpartonFaceAdmin.Core.Models;
using SpartonFaceAdmin.Core.Utilities;

namespace SpartonFaceAdmin.Infrastructure.Data;

public sealed class HrRepository
{
    private const string SelectEmployeesSql = @"
SELECT
    [EmployeeCode],
    LTRIM(RTRIM(
        ISNULL([LastName], '') + ' ' +
        CASE WHEN ISNULL([MidName], '') = '' THEN '' ELSE [MidName] + ' ' END +
        ISNULL([FirstName], '')
    )) AS [FullName],
    CAST(ISNULL([Active], 0) AS bit) AS [Active]
FROM [dbo].[HR_Employee]
ORDER BY [EmployeeCode]";

    public async Task<IReadOnlyList<EmployeeItem>> GetEmployeesAsync(
        DatabaseConnectionSettings settings,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(SqlConnectionFactory.BuildConnectionString(settings));
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var rows = await conn.QueryAsync<EmployeeRow>(
            new CommandDefinition(SelectEmployeesSql, commandTimeout: Math.Clamp(settings.TimeoutSeconds * 3, 5, 60), cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        var result = new List<EmployeeItem>();
        foreach (var row in rows)
        {
            if (activeOnly && !row.Active)
                continue;

            string code = row.EmployeeCode?.Trim() ?? string.Empty;
            string fullName = row.FullName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(code))
                continue;

            result.Add(new EmployeeItem
            {
                EmployeeCode = code,
                FullName = fullName,
                AsciiFullName = VietnameseTextHelper.VniToEng(fullName),
                Active = row.Active
            });
        }

        return result;
    }

    private sealed class EmployeeRow
    {
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public bool Active { get; set; }
    }
}
