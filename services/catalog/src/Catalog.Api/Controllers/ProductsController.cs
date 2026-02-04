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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _repo.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null) return NotFound();
        return Ok(product);
    }
}
