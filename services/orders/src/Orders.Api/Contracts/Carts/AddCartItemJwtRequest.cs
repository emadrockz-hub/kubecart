namespace KubeCart.Orders.Api.Contracts.Carts;

public sealed class AddCartItemJwtRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
