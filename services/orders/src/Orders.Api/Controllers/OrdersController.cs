using Microsoft.AspNetCore.Mvc;
using KubeCart.Orders.Api.Clients;
using Orders.Api.Contracts.Orders;
using Orders.Api.Repositories.Orders;
using KubeCart.Orders.Api.Contracts.Orders;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
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
    // Body: { "userId": "GUID" } (TEMP until JWT)
    [HttpPost("checkout")]
    public async Task<ActionResult<Guid>> Checkout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        // 1) Get cart items
        var cartItems = await _orders.GetActiveCartItemsAsync(request.UserId, ct);
        if (cartItems.Count == 0) return BadRequest("No active cart items to checkout.");

        // 2) Fetch catalog snapshots for each unique productId
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

        // Reduce stock in Catalog for each cart item
        foreach (var i in cartItems)
        {
            var ok = await _catalog.DecreaseStockAsync(i.ProductId, i.Quantity, ct);
            if (!ok) return Conflict($"Catalog decrease-stock failed for ProductId={i.ProductId}. Check Orders console output for Status/Body.");

        }

        // 3) Create order + items + close cart (DB transaction inside repo)
        var orderId = await _orders.CreateOrderFromActiveCartAsync(request.UserId, snapshots, ct);

        return Ok(new CheckoutResponse { OrderId = orderId });
    }

    // GET /api/orders/orders/{id}
    [HttpGet("orders/{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(id, ct);
        if (order is null) return NotFound();
        return Ok(order);
    }

    // DEV ONLY: Delete an order (and its items) for cleanup while testing.
    // Enabled only in Development environment.
    [HttpDelete("dev/orders/{id:guid}")]
    public async Task<IActionResult> DevDeleteOrder([FromRoute] Guid id, CancellationToken ct)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
            return NotFound(); // hide in non-dev

        var deleted = await _orders.DeleteOrderAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }


    // GET /api/orders/orders?userId=GUID
    [HttpGet("orders")]
    public async Task<ActionResult<List<OrderResponse>>> GetByUserId([FromQuery] Guid userId, CancellationToken ct)
    {
        var orders = await _orders.GetByUserIdAsync(userId, ct);
        return Ok(orders);
    }
}
