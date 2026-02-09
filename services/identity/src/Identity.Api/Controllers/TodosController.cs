using Identity.Api.Data;
using Identity.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/todos")]
[Authorize]
public sealed class TodosController : ControllerBase
{
    private readonly ITodoRepository _repo;

    public TodosController(ITodoRepository repo)
    {
        _repo = repo;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TodoItem>>> GetAll()
    {
        var userId = GetUserId();
        var items = await _repo.GetAllAsync(userId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateTodoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest("Title is required.");

        var userId = GetUserId();
        var id = await _repo.CreateAsync(userId, req.Title.Trim());
        return Created($"/api/todos/{id}", new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoRequest req)
    {
        var userId = GetUserId();
        var ok = await _repo.UpdateAsync(userId, id, req.Title ?? "", req.IsDone);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var ok = await _repo.DeleteAsync(userId, id);
        return ok ? NoContent() : NotFound();
    }
}
