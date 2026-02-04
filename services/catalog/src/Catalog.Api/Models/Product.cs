namespace KubeCart.Catalog.Api.Models;

public sealed class Product
{
    public int Id { get; init; }
    public int CategoryId { get; init; }

    public string Name { get; init; } = "";
    public string? Description { get; init; }

    public decimal Price { get; init; }
    public int Stock { get; init; }

    public string? ImageUrl { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
