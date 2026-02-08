using Catalog.Api.Contracts.Products;
using KubeCart.Catalog.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KubeCart.Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ProductRepository _repo;

    public ProductsController(ProductRepository repo)
    {
        _repo = repo;
    }

    // GET /api/catalog/products
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] string? search)
    {
        var products = await _repo.GetAllAsync();

        if (categoryId.HasValue)
            products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(products);
    }

    // GET /api/catalog/products/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null) return NotFound();
        return Ok(product);
    }

    // POST /api/catalog/products/{id}/decrease-stock
    // INTERNAL ONLY (Orders service calls this)
    [Authorize(Policy = "InternalApi")]
    [HttpPost("{id:int}/decrease-stock")]
    public async Task<IActionResult> DecreaseStock([FromRoute] int id, [FromBody] DecreaseStockRequest request)
    {
        if (request.Quantity <= 0) return BadRequest("Quantity must be >= 1.");

        var result = await _repo.DecreaseStockAsync(id, request.Quantity);

        return result switch
        {
            DecreaseStockResult.Success => NoContent(),
            DecreaseStockResult.NotFound => NotFound(),
            DecreaseStockResult.InsufficientStock => Conflict("Insufficient stock."),
            _ => StatusCode(500)
        };
    }
}
