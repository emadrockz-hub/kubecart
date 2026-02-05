using Catalog.Api.Contracts.Products;
using KubeCart.Catalog.Api.Repositories;
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
        // If you already have filtering in repo, use it.
        // Otherwise fall back to GetAllAsync() and filter later (for now).
        var products = await _repo.GetAllAsync();

        if (categoryId.HasValue)
            products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(products);
    }

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

    // GET /api/catalog/products/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null) return NotFound();
        return Ok(product);
    }
}
