using Dapper;
using KubeCart.Catalog.Api.Data;
using KubeCart.Catalog.Api.Models;

namespace KubeCart.Catalog.Api.Repositories;

public sealed class CategoryRepository
{
    private readonly DbConnectionFactory _db;

    public CategoryRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        const string sql = @"
SELECT Id, Name, Description, IsActive, CreatedAtUtc, UpdatedAtUtc
FROM dbo.Categories
ORDER BY Name;";

        using var conn = _db.Create();
        var rows = await conn.QueryAsync<Category>(sql);
        return rows.AsList();
    }
}
