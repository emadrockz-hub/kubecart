using KubeCart.Identity.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KubeCart.Identity.Api.Controllers;

[ApiController]
[Route("api/diag")]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly DbPingRepository _repo;

    public DiagnosticsController(DbPingRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("db-ping")]
    public async Task<IActionResult> DbPing()
    {
        var result = await _repo.PingAsync();
        return Ok(new { ok = result == 1 });
    }
}
