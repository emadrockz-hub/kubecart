using Dapper;
using KubeCart.Orders.Api.Data;
using KubeCart.Orders.Api.Models;

namespace KubeCart.Orders.Api.Repositories;

public sealed class CartItemRepository
{
    private readonly DbConnectionFactory _db;

    public CartItemRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task UpsertItemAsync(Guid cartId, int productId, int quantity)
    {
        const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.CartItems WHERE CartId = @CartId AND ProductId = @ProductId)
BEGIN
    UPDATE dbo.CartItems
    SET Quantity = Quantity + @Quantity,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE CartId = @CartId AND ProductId = @ProductId;
END
ELSE
BEGIN
    INSERT INTO dbo.CartItems (CartId, ProductId, Quantity, CreatedAtUtc)
    VALUES (@CartId, @ProductId, @Quantity, SYSUTCDATETIME());
END";

        using var conn = _db.Create();
        await conn.ExecuteAsync(sql, new { CartId = cartId, ProductId = productId, Quantity = quantity });
    }
    public async Task<IReadOnlyList<CartItemResponse>> GetByCartIdAsync(Guid cartId)
    {
        const string sql = @"
SELECT
    Id,
    CartId,
    ProductId,
    Quantity,
    CreatedAtUtc,
    UpdatedAtUtc
FROM dbo.CartItems
WHERE CartId = @cartId
ORDER BY CreatedAtUtc DESC;";

        using var conn = _db.Create();
        var rows = await conn.QueryAsync<CartItemResponse>(sql, new { cartId });
        return rows.AsList();
    }
}
