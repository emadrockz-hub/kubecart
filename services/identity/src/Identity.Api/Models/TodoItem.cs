namespace Identity.Api.Models;

public sealed class TodoItem
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
