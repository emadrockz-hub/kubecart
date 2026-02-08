namespace KubeCart.Orders.Api.Contracts.Orders;

public sealed class PaymentRequest
{
    // Examples: "Card", "Cash", "Test"
    public string Method { get; set; } = default!;

    // Example: "Stripe", "Square", "Mock"
    public string? Provider { get; set; }

    // External reference (we won't persist yet)
    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    // Example: "USD"
    public string Currency { get; set; } = "USD";
}
