namespace KubeCart.Orders.Api.Models;

public sealed class CartItem
{
    public long Id { get; init; }
    public Guid CartId { get; init; }

    public int ProductId { get; init; }
    public int Quantity { get; init; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
