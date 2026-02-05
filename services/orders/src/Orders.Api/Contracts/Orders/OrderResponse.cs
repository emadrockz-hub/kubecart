namespace Orders.Api.Contracts.Orders;

public sealed class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public sealed class OrderItemResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
