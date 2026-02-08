using KubeCart.Orders.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using KubeCart.Orders.Api.Clients;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Orders.Api.Security;

namespace KubeCart.Orders.Api.Controllers;

[ApiController]
[Route("api/diag")]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly DbPingRepository _repo;
    private readonly CatalogClient _catalog;

    public DiagnosticsController(DbPingRepository repo, CatalogClient catalog)
    {
        _repo = repo;
        _catalog = catalog;
    }

    [HttpGet("catalog-base-url")]
    public IActionResult CatalogBaseUrl()
    {
        var env = Environment.GetEnvironmentVariable("CATALOG_SERVICE_URL");
        return Ok(new { catalogServiceUrl = env });
    }

    [HttpGet("catalog-product/{id:int}")]
    public async Task<IActionResult> GetCatalogProduct(int id, CancellationToken ct)
    {
        var product = await _catalog.GetProductAsync(id, ct);
        if (product is null) return NotFound();
        return Ok(product);
    }


    [HttpGet("db-ping")]
    public async Task<IActionResult> DbPing()
    {
        var result = await _repo.PingAsync();
        return Ok(new { ok = result == 1 });
    }

    [Authorize]
    [HttpGet("whoami")]
    public ActionResult<object> WhoAmI()
    {
        var userId =
            User.FindFirstValue("userId")
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var email =
            User.FindFirstValue("email")
            ?? User.FindFirstValue(ClaimTypes.Email);

        return Ok(new { userId, email });
    }

    [Authorize]
    [HttpGet("effective-user")]
    public ActionResult<object> EffectiveUser()
    {
        var jwtUserId = UserContext.TryGetUserId(User);
        return Ok(new { jwtUserId = jwtUserId?.ToString() });
    }
}