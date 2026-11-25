using Microsoft.Data.SqlClient;
using SpartonFaceAdmin.Core.Models;
using SpartonFaceAdmin.Infrastructure.Configuration;

namespace SpartonFaceAdmin.Infrastructure.Data;

public static class SqlConnectionFactory
{
    public static string BuildConnectionString(DatabaseConnectionSettings settings, string? plainPasswordOverride = null)
    {
        string password = plainPasswordOverride ?? DpapiSettingsStore.UnprotectSecret(settings.ProtectedPassword);
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = settings.Server?.Trim() ?? string.Empty,
            InitialCatalog = settings.Database?.Trim() ?? string.Empty,
            UserID = settings.Username?.Trim() ?? string.Empty,
            Password = password,
            ConnectTimeout = Math.Clamp(settings.TimeoutSeconds, 1, 60),
            Encrypt = false,
            TrustServerCertificate = true,
            MultipleActiveResultSets = false
        };
        return builder.ConnectionString;
    }

    public static async Task<(bool Ok, string? Error)> TestConnectionAsync(
        DatabaseConnectionSettings settings,
        string? plainPasswordOverride = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new SqlConnection(BuildConnectionString(settings, plainPasswordOverride));
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
