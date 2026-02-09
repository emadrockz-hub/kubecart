using Identity.Api.Models;

namespace Identity.Api.Data;

public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(Guid userId);
    Task<TodoItem?> GetByIdAsync(Guid userId, int id);
    Task<int> CreateAsync(Guid userId, string title);
    Task<bool> UpdateAsync(Guid userId, int id, string title, bool isDone);
    Task<bool> DeleteAsync(Guid userId, int id);
}
