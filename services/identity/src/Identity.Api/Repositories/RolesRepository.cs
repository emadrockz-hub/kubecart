using Dapper;
using KubeCart.Identity.Api.Data;
using Microsoft.Data.SqlClient;

namespace KubeCart.Identity.Api.Repositories;

public sealed class RolesRepository
{
    private readonly DbConnectionFactory _db;

    public RolesRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<int?> GetRoleIdByNameAsync(string name, CancellationToken ct)
    {
        const string sql = @"
SELECT TOP 1 Id
FROM dbo.Roles
WHERE Name = @Name;
";
        await using SqlConnection conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<int?>(
            new CommandDefinition(sql, new { Name = name }, cancellationToken: ct));
    }

    public async Task<int> EnsureRoleAsync(string name, CancellationToken ct)
    {
        var existing = await GetRoleIdByNameAsync(name, ct);
        if (existing.HasValue) return existing.Value;

        // If dbo.Roles.Id is INT IDENTITY, we must NOT insert Id manually.
        const string insert = @"
INSERT INTO dbo.Roles (Name)
VALUES (@Name);

SELECT CAST(SCOPE_IDENTITY() AS int);
";
        await using SqlConnection conn = _db.Create();
        return await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(insert, new { Name = name }, cancellationToken: ct));
    }
}
