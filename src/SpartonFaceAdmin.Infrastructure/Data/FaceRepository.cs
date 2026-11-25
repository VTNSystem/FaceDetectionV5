using Dapper;
using Microsoft.Data.SqlClient;
using Serilog;
using SpartonFaceAdmin.Core.Models;

namespace SpartonFaceAdmin.Infrastructure.Data;

public sealed class FaceRepository
{
    private const string SelectSummariesByCodeSql = @"
SELECT
    [ID] AS Id,
    [Code] AS Code,
    [Fullname] AS Fullname,
    [Group] AS [Group],
    [SimilarThreshold] AS SimilarThreshold,
    CAST(ISNULL([HasFaceMask], 0) AS bit) AS HasFaceMask,
    ISNULL([CreatedDate], GETDATE()) AS CreatedDate,
    CAST(CASE WHEN [Image] IS NOT NULL AND [FaceImage] IS NOT NULL THEN 1 ELSE 0 END AS bit) AS HasStoredImage
FROM [dbo].[FaceList]
WHERE [Code] = @Code
ORDER BY [ID] DESC";

    private const string SelectFaceCountsSql = @"
SELECT [Code] AS Code, COUNT(1) AS Total
FROM [dbo].[FaceList]
GROUP BY [Code]";

    private const string SelectImagesByIdSql = @"
SELECT
    [ID] AS Id,
    [Image] AS FullImageBytes,
    [FaceImage] AS FaceImageBytes
FROM [dbo].[FaceList]
WHERE [ID] = @Id";

    private const string InsertFaceSql = @"
INSERT INTO [dbo].[FaceList] (
    [Code], [Fullname], [Group],
    [FacePositionXc], [FacePositionYc], [FacePositionW], [FacePositionAngle],
    [Eye1X], [Eye1Y], [Eye2X], [Eye2Y],
    [Template], [Image], [FaceImage],
    [SimilarThreshold], [HasFaceMask], [CreatedDate]
)
VALUES (
    @Code, @Fullname, @Group,
    @FacePositionXc, @FacePositionYc, @FacePositionW, @FacePositionAngle,
    @Eye1X, @Eye1Y, @Eye2X, @Eye2Y,
    @Template, @Image, @FaceImage,
    @SimilarThreshold, @HasFaceMask, GETDATE()
);
SELECT CAST(SCOPE_IDENTITY() AS int);";

    private const string DeleteByIdSql = "DELETE FROM [dbo].[FaceList] WHERE [ID] = @Id";
    private const string DeleteByCodeSql = "DELETE FROM [dbo].[FaceList] WHERE [Code] = @Code";
    private const string InsertLogSql = "INSERT INTO [dbo].[Logs] ([Message], [Type], [CreatedDate]) VALUES (@Message, @Type, GETDATE())";

    public async Task<Dictionary<string, int>> GetEmployeeFaceCountsAsync(
        DatabaseConnectionSettings settings,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(SqlConnectionFactory.BuildConnectionString(settings));
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var rows = await conn.QueryAsync<FaceCountRow>(
            new CommandDefinition(SelectFaceCountsSql, commandTimeout: Math.Clamp(settings.TimeoutSeconds * 3, 5, 60), cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in rows)
        {
            if (!string.IsNullOrWhiteSpace(r.Code))
                dict[r.Code.Trim()] = r.Total;
        }
        return dict;
    }

    public async Task<IReadOnlyList<FaceRecordSummary>> GetFaceSummariesByCodeAsync(
        DatabaseConnectionSettings settings,
        string code,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(SqlConnectionFactory.BuildConnectionString(settings));
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var rows = await conn.QueryAsync<FaceRecordSummary>(
            new CommandDefinition(
                SelectSummariesByCodeSql,
                new { Code = code },
                commandTimeout: Math.Clamp(settings.TimeoutSeconds * 3, 5, 60),
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return rows.AsList();
    }

    public async Task<FaceRecordImages?> GetFaceImagesByIdAsync(
        DatabaseConnectionSettings settings,
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(SqlConnectionFactory.BuildConnectionString(settings));
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await conn.QueryFirstOrDefaultAsync<FaceRecordImages>(
            new CommandDefinition(
                SelectImagesByIdSql,
                new { Id = id },
                commandTimeout: Math.Clamp(settings.TimeoutSeconds * 5, 10, 60),
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task<int> InsertFaceRecordAsync(
        DatabaseConnectionSettings settings,
        FaceRecordInsertRequest record,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(SqlConnectionFactory.BuildConnectionString(settings));
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                InsertFaceSql,
                new
                {
                    record.Code,
                    record.Fullname,
                    record.Group,
                    record.FacePositionXc,
                    record.FacePositionYc,
                    record.FacePositionW,
                    record.FacePositionAngle,
                    record.Eye1X,
                    record.Eye1Y,
                    record.Eye2X,
                    record.Eye2Y,
                    record.Template,
                    record.Image,
                    record.FaceImage,
                    record.SimilarThreshold,
                    HasFaceMask = record.HasFaceMask ? 1 : 0
                },
                commandTimeout: Math.Clamp(settings.TimeoutSeconds * 5, 10, 60),
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task<int> DeleteByIdAsync(
        DatabaseConnectionSettings settings,
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(SqlConnectionFactory.BuildConnectionString(settings));
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await conn.ExecuteAsync(
            new CommandDefinition(DeleteByIdSql, new { Id = id }, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task<int> DeleteByCodeAsync(
        DatabaseConnectionSettings settings,
        string code,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(SqlConnectionFactory.BuildConnectionString(settings));
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await conn.ExecuteAsync(
            new CommandDefinition(DeleteByCodeSql, new { Code = code }, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task WriteAuditLogAsync(
        DatabaseConnectionSettings settings,
        string message,
        string type = "Info")
    {
        string formatted = $"[{Environment.MachineName}] {message}";
        try
        {
            await using var conn = new SqlConnection(SqlConnectionFactory.BuildConnectionString(settings));
            await conn.OpenAsync().ConfigureAwait(false);
            await conn.ExecuteAsync(InsertLogSql, new { Message = formatted, Type = type }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Information("Audit fallback ({Type}): {Message} | DB Error: {Err}", type, formatted, ex.Message);
        }
    }

    private sealed class FaceCountRow
    {
        public string Code { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
