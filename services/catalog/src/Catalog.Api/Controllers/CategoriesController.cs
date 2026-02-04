using KubeCart.Catalog.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KubeCart.Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly CategoryRepository _repo;

    public CategoriesController(CategoryRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _repo.GetAllAsync();
        return Ok(categories);
    }
}
