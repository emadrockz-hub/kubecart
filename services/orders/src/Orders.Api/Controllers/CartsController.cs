using KubeCart.Orders.Api.Clients;
using KubeCart.Orders.Api.Models;
using KubeCart.Orders.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KubeCart.Orders.Api.Controllers;

[ApiController]
[Route("api/orders/carts")]
public sealed class CartsController : ControllerBase
{
    private readonly CartRepository _repo;
    private readonly CartItemRepository _items;
    private readonly CatalogClient _catalog;

    public CartsController(CartRepository repo, CartItemRepository items, CatalogClient catalog)
    {
        _repo = repo;
        _items = items;
        _catalog = catalog;
    }

    // TEMP for now: pass userId as query string until JWT is wired.
    // Example: /api/orders/carts/active?userId=GUID
    [HttpGet("active")]
    public async Task<IActionResult> GetOrCreateActive([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty) return BadRequest("userId is required.");

        var cart = await _repo.GetOrCreateActiveCartAsync(userId);
        return Ok(cart);
    }

    // GET /api/orders/carts/active/items?userId=GUID
    [HttpGet("active/items")]
    public async Task<IActionResult> GetActiveCartItems([FromQuery] Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty) return BadRequest("userId is required.");

        var cart = await _repo.GetOrCreateActiveCartAsync(userId);
        var items = await _items.GetByCartIdAsync(cart.Id);

        // Enrich items from Catalog (simple approach for now)
        var catalogProducts = await _catalog.GetProductsAsync(ct);
        var lookup = catalogProducts.ToDictionary(p => p.Id, p => p);

        var viewItems = items.Select(i =>
        {
            lookup.TryGetValue(i.ProductId, out var p);

            return new CartItemViewDto
            {
                Id = i.Id,
                CartId = i.CartId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,

                Name = p?.Name ?? "",
                Price = p?.Price ?? 0m,
                ImageUrl = p?.ImageUrl ?? "",

                CreatedAtUtc = i.CreatedAtUtc,
                UpdatedAtUtc = i.UpdatedAtUtc
            };
        }).ToList();

        return Ok(new
        {
            cartId = cart.Id,
            userId = cart.UserId,
            status = cart.Status,
            items = viewItems
        });
    }

    // POST /api/orders/carts/items
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest req, CancellationToken ct)
    {
        if (req.UserId == Guid.Empty) return BadRequest("userId is required.");
        if (req.ProductId <= 0) return BadRequest("productId must be > 0.");
        if (req.Quantity <= 0) return BadRequest("quantity must be > 0.");

        // Validate product via Catalog API (NO direct DB access)
        var product = await _catalog.GetProductAsync(req.ProductId, ct);
        if (product is null) return NotFound("Product not found.");
        if (!product.IsActive) return BadRequest("Product is inactive.");
        if (product.Stock < req.Quantity) return BadRequest("Insufficient stock.");

        var cart = await _repo.GetOrCreateActiveCartAsync(req.UserId);
        await _items.UpsertItemAsync(cart.Id, req.ProductId, req.Quantity);

        return Ok(new { cartId = cart.Id, added = true });
    }
}
