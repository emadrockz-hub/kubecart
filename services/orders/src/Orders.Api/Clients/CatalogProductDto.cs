namespace KubeCart.Orders.Api.Clients;

public sealed class CatalogProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; }
}
