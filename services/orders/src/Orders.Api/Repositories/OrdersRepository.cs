using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using KubeCart.Orders.Api.Data;
using Orders.Api.Contracts.Orders;

namespace Orders.Api.Repositories.Orders;

public sealed class OrdersRepository : IOrdersRepository
{
    private readonly DbConnectionFactory _db;

    public OrdersRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    // NEW: Used by checkout controller to get ProductId+Quantity from active cart
    public async Task<List<ActiveCartItem>> GetActiveCartItemsAsync(Guid userId, CancellationToken ct)
    {
        using SqlConnection conn = _db.Create();
        await conn.OpenAsync(ct);

        // 1) Find active cart id
        const string sqlCart = @"
SELECT TOP 1 c.Id
FROM dbo.Carts c
WHERE c.UserId = @UserId AND c.Status = 'Active'
ORDER BY c.CreatedAtUtc DESC;
";

        var cartId = await conn.QuerySingleOrDefaultAsync<Guid?>(new CommandDefinition(
            sqlCart, new { UserId = userId }, cancellationToken: ct));

        if (cartId is null) return new List<ActiveCartItem>();

        // 2) Read cart items (schema: ProductId int, Quantity int)
        const string sqlItems = @"
SELECT
    ci.ProductId,
    ci.Quantity
FROM dbo.CartItems ci
WHERE ci.CartId = @CartId;
";

        var items = await conn.QueryAsync<ActiveCartItem>(new CommandDefinition(
            sqlItems, new { CartId = cartId.Value }, cancellationToken: ct));

        return items.AsList();
    }

    // Existing GET: /api/orders/orders/{id}
    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        using SqlConnection conn = _db.Create();
        await conn.OpenAsync(ct);

        const string sqlOrder = @"
SELECT
    o.Id,
    o.UserId,
    o.Status,
    o.TotalAmount,
    o.CreatedAtUtc
FROM dbo.Orders o
WHERE o.Id = @Id;
";

        const string sqlItems = @"
SELECT
    oi.ProductId,
    oi.ProductName,
    oi.ImageUrl,
    oi.UnitPrice,
    oi.Quantity,
    oi.LineTotal
FROM dbo.OrderItems oi
WHERE oi.OrderId = @Id
ORDER BY oi.Id;
";

        var order = await conn.QuerySingleOrDefaultAsync<OrderResponse>(new CommandDefinition(
            sqlOrder, new { Id = id }, cancellationToken: ct));

        if (order is null) return null;

        var items = await conn.QueryAsync<OrderItemResponse>(new CommandDefinition(
            sqlItems, new { Id = id }, cancellationToken: ct));

        order.Items = items.AsList();
        return order;
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId, CancellationToken ct)
    {
        using SqlConnection conn = _db.Create();
        await conn.OpenAsync(ct);

        using var tx = conn.BeginTransaction();

        try
        {
            // Ensure order exists
            const string sqlExists = @"SELECT COUNT(1) FROM dbo.Orders WHERE Id = @Id;";
            var exists = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                sqlExists, new { Id = orderId }, transaction: tx, cancellationToken: ct));

            if (exists == 0)
            {
                tx.Rollback();
                return false;
            }

            // Delete items first
            const string sqlDeleteItems = @"DELETE FROM dbo.OrderItems WHERE OrderId = @Id;";
            await conn.ExecuteAsync(new CommandDefinition(
                sqlDeleteItems, new { Id = orderId }, transaction: tx, cancellationToken: ct));

            // Delete order
            const string sqlDeleteOrder = @"DELETE FROM dbo.Orders WHERE Id = @Id;";
            await conn.ExecuteAsync(new CommandDefinition(
                sqlDeleteOrder, new { Id = orderId }, transaction: tx, cancellationToken: ct));

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }


    // Existing GET: /api/orders/orders?userId=GUID
    public async Task<List<OrderResponse>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        using SqlConnection conn = _db.Create();
        await conn.OpenAsync(ct);

        const string sqlOrders = @"
SELECT
    o.Id,
    o.UserId,
    o.Status,
    o.TotalAmount,
    o.CreatedAtUtc
FROM dbo.Orders o
WHERE o.UserId = @UserId
ORDER BY o.CreatedAtUtc DESC;
";

        const string sqlItems = @"
SELECT
    oi.OrderId,
    oi.ProductId,
    oi.ProductName,
    oi.ImageUrl,
    oi.UnitPrice,
    oi.Quantity,
    oi.LineTotal
FROM dbo.OrderItems oi
WHERE oi.OrderId IN @OrderIds
ORDER BY oi.Id;
";

        var orders = (await conn.QueryAsync<OrderResponse>(new CommandDefinition(
            sqlOrders, new { UserId = userId }, cancellationToken: ct))).AsList();

        if (orders.Count == 0) return orders;

        var orderIds = orders.Select(o => o.Id).ToArray();
        if (orderIds.Length == 0) return orders;

        var items = await conn.QueryAsync<OrderItemRow>(new CommandDefinition(
            sqlItems, new { OrderIds = orderIds }, cancellationToken: ct));

        var map = orders.ToDictionary(o => o.Id, o => o);

        foreach (var row in items)
        {
            if (map.TryGetValue(row.OrderId, out var o))
            {
                o.Items.Add(new OrderItemResponse
                {
                    ProductId = row.ProductId,
                    ProductName = row.ProductName,
                    ImageUrl = row.ImageUrl,
                    UnitPrice = row.UnitPrice,
                    Quantity = row.Quantity,
                    LineTotal = row.LineTotal
                });
            }
        }

        return orders;
    }

    // Creates order + orderitems from active cart, and closes cart.
    // Snapshot fields are filled by caller using Catalog HTTP.
    public async Task<Guid> CreateOrderFromActiveCartAsync(
        Guid userId,
        IReadOnlyList<OrderItemSnapshot> snapshots,
        CancellationToken ct)
    {
        if (snapshots is null || snapshots.Count == 0)
            throw new InvalidOperationException("No order items to checkout.");

        using SqlConnection conn = _db.Create();
        await conn.OpenAsync(ct);

        using var tx = conn.BeginTransaction();

        try
        {
            // 1) Find active cart
            const string sqlCart = @"
SELECT TOP 1 c.Id
FROM dbo.Carts c
WHERE c.UserId = @UserId AND c.Status = 'Active'
ORDER BY c.CreatedAtUtc DESC;
";

            var cartId = await conn.QuerySingleOrDefaultAsync<Guid?>(new CommandDefinition(
                sqlCart, new { UserId = userId }, transaction: tx, cancellationToken: ct));

            if (cartId is null)
                throw new InvalidOperationException("No active cart found for this user.");

            // 2) Ensure cart items exist (schema: ProductId + Quantity only)
            const string sqlCartItems = @"
SELECT
    ci.ProductId,
    ci.Quantity
FROM dbo.CartItems ci
WHERE ci.CartId = @CartId;
";

            var cartItems = (await conn.QueryAsync<CartItemRow>(new CommandDefinition(
                sqlCartItems, new { CartId = cartId.Value }, transaction: tx, cancellationToken: ct))).AsList();

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            // 3) Every cart item must have a snapshot entry
            var snapshotMap = snapshots.ToDictionary(s => s.ProductId, s => s);

            foreach (var ci in cartItems)
            {
                if (!snapshotMap.ContainsKey(ci.ProductId))
                    throw new InvalidOperationException($"Missing product snapshot for ProductId={ci.ProductId}.");
            }

            // 4) Calculate total
            decimal total = 0m;
            foreach (var ci in cartItems)
            {
                var snap = snapshotMap[ci.ProductId];
                total += snap.UnitPrice * ci.Quantity;
            }

            // 5) Insert Orders
            var orderId = Guid.NewGuid();

            const string sqlInsertOrder = @"
INSERT INTO dbo.Orders (Id, UserId, Status, TotalAmount, CreatedAtUtc)
VALUES (@Id, @UserId, @Status, @TotalAmount, SYSUTCDATETIME());
";

            await conn.ExecuteAsync(new CommandDefinition(
                sqlInsertOrder,
                new { Id = orderId, UserId = userId, Status = "Pending", TotalAmount = total },
                transaction: tx,
                cancellationToken: ct));

            // 6) Insert OrderItems (snapshot + cart qty) - NO CreatedAtUtc column in your table
            const string sqlInsertItem = @"
INSERT INTO dbo.OrderItems
(
    OrderId,
    ProductId,
    ProductName,
    ImageUrl,
    UnitPrice,
    Quantity
)
VALUES
(
    @OrderId,
    @ProductId,
    @ProductName,
    @ImageUrl,
    @UnitPrice,
    @Quantity
);
";

            foreach (var ci in cartItems)
            {
                var snap = snapshotMap[ci.ProductId];

                await conn.ExecuteAsync(new CommandDefinition(
                    sqlInsertItem,
                    new
                    {
                        OrderId = orderId,
                        ProductId = ci.ProductId,
                        ProductName = snap.ProductName,
                        ImageUrl = snap.ImageUrl,
                        UnitPrice = snap.UnitPrice,
                        Quantity = ci.Quantity
                    },
                    transaction: tx,
                    cancellationToken: ct));
            }

            // 7) Close cart
            const string sqlCloseCart = @"
UPDATE dbo.Carts
SET Status = 'Closed', UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @CartId;
";

            await conn.ExecuteAsync(new CommandDefinition(
                sqlCloseCart, new { CartId = cartId.Value }, transaction: tx, cancellationToken: ct));

            tx.Commit();
            return orderId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private sealed class CartItemRow
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    private sealed class OrderItemRow
    {
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}

// Snapshot payload used at checkout time (filled from Catalog HTTP in the controller/service)
public sealed class OrderItemSnapshot
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
}
