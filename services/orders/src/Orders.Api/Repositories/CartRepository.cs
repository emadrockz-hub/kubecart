using Dapper;
using KubeCart.Orders.Api.Data;
using KubeCart.Orders.Api.Models;

namespace KubeCart.Orders.Api.Repositories;

public sealed class CartRepository
{
    private readonly DbConnectionFactory _db;

    public CartRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Cart?> GetActiveCartByUserIdAsync(Guid userId)
    {
        const string sql = @"
SELECT TOP 1 Id, UserId, Status, CreatedAtUtc, UpdatedAtUtc
FROM dbo.Carts
WHERE UserId = @UserId AND Status = 'Active'
ORDER BY CreatedAtUtc DESC;";

        using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<Cart>(sql, new { UserId = userId });
    }


public async Task<Cart> GetOrCreateActiveCartAsync(Guid userId)
    {
        var existing = await GetActiveCartByUserIdAsync(userId);
        if (existing is not null) return existing;

        const string insertSql = @"
INSERT INTO dbo.Carts (UserId, Status)
OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.Status, INSERTED.CreatedAtUtc, INSERTED.UpdatedAtUtc
VALUES (@UserId, 'Active');";

        using var conn = _db.Create();
        return await conn.QuerySingleAsync<Cart>(insertSql, new { UserId = userId });
    }
}
