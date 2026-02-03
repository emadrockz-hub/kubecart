using Dapper;
using KubeCart.Catalog.Api.Data;

namespace KubeCart.Catalog.Api.Repositories;

public sealed class DbPingRepository
{
    private readonly DbConnectionFactory _db;

    public DbPingRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<int> PingAsync()
    {
        using var conn = _db.Create();
        return await conn.ExecuteScalarAsync<int>("SELECT 1");
    }
}
