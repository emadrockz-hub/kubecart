using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KubeCart.Orders.Api.Data;

public sealed class DbConnectionFactory
{
    private readonly IConfiguration _config;

    public DbConnectionFactory(IConfiguration config)
    {
        _config = config;
    }

    public SqlConnection Create()
    {
        var connStr = _config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

        return new SqlConnection(connStr);
    }
}
