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

    public async Task<bool> UpdateQuantityInActiveCartAsync(Guid userId, long cartItemId, int quantity, CancellationToken ct)
    {
        using var conn = _db.Create();
        await conn.OpenAsync(ct);

        const string sqlCart = @"
SELECT TOP 1 c.Id
FROM dbo.Carts c
WHERE c.UserId = @UserId AND c.Status = 'Active'
ORDER BY c.CreatedAtUtc DESC;
";
        var cartId = await conn.QuerySingleOrDefaultAsync<Guid?>(new CommandDefinition(
            sqlCart, new { UserId = userId }, cancellationToken: ct));

        if (cartId is null) return false;

        const string sqlUpdate = @"
UPDATE dbo.CartItems
SET Quantity = @Quantity, UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id AND CartId = @CartId;
";
        var rows = await conn.ExecuteAsync(new CommandDefinition(
            sqlUpdate,
            new { Quantity = quantity, Id = cartItemId, CartId = cartId.Value },
            cancellationToken: ct));

        return rows > 0;
    }

    public async Task<bool> DeleteFromActiveCartAsync(Guid userId, long cartItemId, CancellationToken ct)
    {
        using var conn = _db.Create();
        await conn.OpenAsync(ct);

        // 1) Find active cart id for the user
        const string sqlCart = @"
SELECT TOP 1 c.Id
FROM dbo.Carts c
WHERE c.UserId = @UserId AND c.Status = 'Active'
ORDER BY c.CreatedAtUtc DESC;
";

        var cartId = await conn.QuerySingleOrDefaultAsync<Guid?>(new CommandDefinition(
            sqlCart, new { UserId = userId }, cancellationToken: ct));

        if (cartId is null) return false;

        // 2) Delete the cart item only if it belongs to that active cart
        const string sqlDelete = @"
DELETE FROM dbo.CartItems
WHERE Id = @Id AND CartId = @CartId;
";

        var rows = await conn.ExecuteAsync(new CommandDefinition(
            sqlDelete, new { Id = cartItemId, CartId = cartId.Value }, cancellationToken: ct));

        return rows > 0;
    }
}
