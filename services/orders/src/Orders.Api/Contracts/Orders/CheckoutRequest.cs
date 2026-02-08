namespace KubeCart.Orders.Api.Contracts.Orders;

public sealed class CheckoutRequest
{
    public PaymentRequest? Payment { get; set; }
}
