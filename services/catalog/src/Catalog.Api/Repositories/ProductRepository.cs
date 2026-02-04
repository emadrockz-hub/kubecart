using Dapper;
using KubeCart.Catalog.Api.Data;
using KubeCart.Catalog.Api.Models;

namespace KubeCart.Catalog.Api.Repositories;

public sealed class ProductRepository
{
    private readonly DbConnectionFactory _db;

    public ProductRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        const string sql = @"
SELECT
    Id, CategoryId, Name, Description, Price, Stock, ImageUrl, IsActive, CreatedAtUtc, UpdatedAtUtc
FROM dbo.Products
ORDER BY Name;";

        using var conn = _db.Create();
        var rows = await conn.QueryAsync<Product>(sql);
        return rows.AsList();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        const string sql = @"
SELECT
    Id, CategoryId, Name, Description, Price, Stock, ImageUrl, IsActive, CreatedAtUtc, UpdatedAtUtc
FROM dbo.Products
WHERE Id = @Id;";

        using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(int? categoryId, string? search)
    {
        var sql = @"
SELECT
    Id, CategoryId, Name, Description, Price, Stock, ImageUrl, IsActive, CreatedAtUtc, UpdatedAtUtc
FROM dbo.Products
WHERE 1=1
";

        var p = new DynamicParameters();

        if (categoryId.HasValue)
        {
            sql += " AND CategoryId = @CategoryId";
            p.Add("CategoryId", categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (Name LIKE @Search OR Description LIKE @Search)";
            p.Add("Search", $"%{search.Trim()}%");
        }

        sql += " ORDER BY Name;";

        using var conn = _db.Create();
        var rows = await conn.QueryAsync<Product>(sql, p);
        return rows.AsList();
    }
}
