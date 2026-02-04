namespace KubeCart.Orders.Api.Models;

public sealed class AddCartItemRequest
{
    public Guid UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
