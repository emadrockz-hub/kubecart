using KubeCart.Orders.Api.Clients;
using KubeCart.Orders.Api.Contracts.Carts;
using KubeCart.Orders.Api.Models;
using KubeCart.Orders.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Security;

namespace KubeCart.Orders.Api.Controllers;

public sealed class UpdateCartItemQuantityRequest
{
    public int Quantity { get; set; }
}

[ApiController]
[Route("api/orders/carts")]
[Authorize] // JWT-only for all cart operations
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

    // PUT /api/orders/carts/items/{id}
    [HttpPut("items/{id:long}")]
    public async Task<IActionResult> UpdateCartItemQuantity(
        [FromRoute] long id,
        [FromBody] UpdateCartItemQuantityRequest request,
        CancellationToken ct)
    {
        if (request.Quantity <= 0) return BadRequest("Quantity must be >= 1.");

        var userId = UserContext.TryGetUserId(User);
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return BadRequest("JWT userId is missing.");

        var updated = await _items.UpdateQuantityInActiveCartAsync(userId.Value, id, request.Quantity, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    // GET /api/orders/carts/active
    [HttpGet("active")]
    public async Task<IActionResult> GetOrCreateActive(CancellationToken ct)
    {
        var userId = UserContext.TryGetUserId(User);
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return BadRequest("JWT userId is missing.");

        var cart = await _repo.GetOrCreateActiveCartAsync(userId.Value);
        return Ok(cart);
    }

    // DELETE /api/orders/carts/items/{id}
    [HttpDelete("items/{id:long}")]
    public async Task<IActionResult> DeleteCartItem([FromRoute] long id, CancellationToken ct)
    {
        var userId = UserContext.TryGetUserId(User);
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return BadRequest("JWT userId is missing.");

        var deleted = await _items.DeleteFromActiveCartAsync(userId.Value, id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // GET /api/orders/carts/active/items
    [HttpGet("active/items")]
    public async Task<IActionResult> GetActiveCartItems(CancellationToken ct)
    {
        var userId = UserContext.TryGetUserId(User);
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return BadRequest("JWT userId is missing.");

        var cart = await _repo.GetOrCreateActiveCartAsync(userId.Value);
        var items = await _items.GetByCartIdAsync(cart.Id);

        // Enrich items from Catalog
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
    // Body: { "productId": 14, "quantity": 1 }
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemJwtRequest req, CancellationToken ct)
    {
        var userId = UserContext.TryGetUserId(User);
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return BadRequest("JWT userId is missing.");

        if (req.ProductId <= 0) return BadRequest("productId must be > 0.");
        if (req.Quantity <= 0) return BadRequest("quantity must be > 0.");

        // Validate product via Catalog API
        var product = await _catalog.GetProductAsync(req.ProductId, ct);
        if (product is null) return NotFound("Product not found.");
        if (!product.IsActive) return BadRequest("Product is inactive.");
        if (product.Stock < req.Quantity) return BadRequest("Insufficient stock.");

        var cart = await _repo.GetOrCreateActiveCartAsync(userId.Value);
        await _items.UpsertItemAsync(cart.Id, req.ProductId, req.Quantity);

        return Ok(new { cartId = cart.Id, added = true });
    }
}
