using KubeCart.Orders.Api.Clients;
using KubeCart.Orders.Api.Contracts.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Contracts.Orders;
using Orders.Api.Repositories.Orders;
using Orders.Api.Security;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrdersRepository _orders;
    private readonly CatalogClient _catalog;

    public OrdersController(IOrdersRepository orders, CatalogClient catalog)
    {
        _orders = orders;
        _catalog = catalog;
    }

    // POST /api/orders/checkout
    // JWT-only. Body can include optional payment info.
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var userId = UserContext.TryGetUserId(User);
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return BadRequest("JWT userId is missing.");

        // 1) Get cart items from Orders DB
        var cartItems = await _orders.GetActiveCartItemsAsync(userId.Value, ct);
        if (cartItems is null || cartItems.Count == 0)
            return BadRequest("Active cart is empty.");

        // 2) Fetch catalog snapshots for each cart item
        var snapshots = new List<OrderItemSnapshot>(cartItems.Count);

        foreach (var item in cartItems)
        {
            var product = await _catalog.GetProductAsync(item.ProductId, ct);
            if (product is null)
                return BadRequest($"ProductId {item.ProductId} not found in catalog.");

            snapshots.Add(new OrderItemSnapshot
            {
                ProductId = item.ProductId,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                UnitPrice = product.Price
            });
        }

        // Compute total
        var snapshotMap = snapshots.ToDictionary(s => s.ProductId, s => s);
        decimal totalAmount = 0m;

        foreach (var ci in cartItems)
        {
            var snap = snapshotMap[ci.ProductId];
            totalAmount += snap.UnitPrice * ci.Quantity;
        }

        // Payment validation (optional) -> determines status
        var status = "Pending";

        if (request.Payment is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Payment.Method))
                return BadRequest("payment.method is required.");

            if (request.Payment.Amount <= 0)
                return BadRequest("payment.amount must be > 0.");

            if (decimal.Round(request.Payment.Amount, 2) != decimal.Round(totalAmount, 2))
                return BadRequest($"payment.amount must equal order total ({decimal.Round(totalAmount, 2)}).");

            status = "Paid";
        }

        // Reduce stock in Catalog for each cart item
        foreach (var i in cartItems)
        {
            var ok = await _catalog.DecreaseStockAsync(i.ProductId, i.Quantity, ct);
            if (!ok)
                return Conflict($"Catalog decrease-stock failed for ProductId={i.ProductId}. Check Orders console output for Status/Body.");
        }

        // 3) Create order + items + close cart (repo handles transaction)
        var orderId = await _orders.CreateOrderFromActiveCartAsync(userId.Value, snapshots, status, ct);

        return Ok(new CheckoutResponse { OrderId = orderId });
    }

    // GET /api/orders/orders/{id}
    // Owner-only (Admin can view any)
    [HttpGet("orders/{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(id, ct);
        if (order is null) return NotFound();

        var jwtUserId = UserContext.TryGetUserId(User);
        if (!jwtUserId.HasValue || jwtUserId.Value == Guid.Empty)
            return BadRequest("JWT userId is missing.");

        var isAdmin = UserContext.IsAdmin(User);
        if (!isAdmin && order.UserId != jwtUserId.Value)
            return Forbid();

        return Ok(order);
    }

    // GET /api/orders/orders
    // JWT-only (no query param)
    [HttpGet("orders")]
    public async Task<ActionResult<List<OrderResponse>>> GetMyOrders(CancellationToken ct)
    {
        var jwtUserId = UserContext.TryGetUserId(User);
        if (!jwtUserId.HasValue || jwtUserId.Value == Guid.Empty)
            return BadRequest("JWT userId is missing.");

        var orders = await _orders.GetByUserIdAsync(jwtUserId.Value, ct);
        return Ok(orders);
    }

    // DEV ONLY: Delete an order (and its items) for cleanup while testing.
    // Enabled only in Development environment. Admin-only.
    [Authorize(Roles = "Admin")]
    [HttpDelete("dev/orders/{id:guid}")]
    public async Task<IActionResult> DevDeleteOrder([FromRoute] Guid id, CancellationToken ct)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
            return NotFound();

        var deleted = await _orders.DeleteOrderAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }
}
