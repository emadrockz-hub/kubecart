namespace Identity.Api.Models;

public sealed class CreateTodoRequest
{
    public string Title { get; set; } = "";
}

public sealed class UpdateTodoRequest
{
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
}
