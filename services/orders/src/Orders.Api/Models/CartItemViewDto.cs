namespace KubeCart.Orders.Api.Models;

public sealed class CartItemViewDto
{
    public long Id { get; init; }
    public Guid CartId { get; init; }

    public int ProductId { get; init; }
    public int Quantity { get; init; }

    // Enriched from Catalog
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = "";

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
