namespace KubeCart.Orders.Api.Models;

public sealed class Cart
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }

    public string Status { get; init; } = ""; // e.g. Active, CheckedOut (we'll standardize)
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
