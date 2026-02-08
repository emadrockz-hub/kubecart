using Orders.Api.Contracts.Orders;

namespace Orders.Api.Repositories.Orders;

public interface IOrdersRepository
{
    Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<OrderResponse>> GetByUserIdAsync(Guid userId, CancellationToken ct);

    Task<List<ActiveCartItem>> GetActiveCartItemsAsync(Guid userId, CancellationToken ct);

    Task<Guid> CreateOrderFromActiveCartAsync(Guid userId, List<OrderItemSnapshot> snapshots, string status, CancellationToken ct);

    Task<bool> DeleteOrderAsync(Guid orderId, CancellationToken ct);
}

    public sealed record ActiveCartItem(int ProductId, int Quantity);
