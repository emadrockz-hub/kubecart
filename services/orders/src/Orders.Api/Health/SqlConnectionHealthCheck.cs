using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.Api.Health;

public sealed class SqlConnectionHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;

    public SqlConnectionHealthCheck(IConfiguration config)
    {
        _config = config;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connStr = _config.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connStr))
            return HealthCheckResult.Unhealthy("Missing ConnectionStrings:Default");

        try
        {
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            cmd.CommandType = CommandType.Text;

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return (result is not null)
                ? HealthCheckResult.Healthy("SQL reachable")
                : HealthCheckResult.Unhealthy("SQL reachable but SELECT 1 returned null");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL not reachable", ex);
        }
    }
}
