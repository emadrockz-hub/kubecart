using Dapper;
using Identity.Api.Models;
using KubeCart.Identity.Api.Data;
using System.Data;

namespace Identity.Api.Data;

public sealed class TodoRepository : ITodoRepository
{
    private readonly DbConnectionFactory _factory;

    public TodoRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    private IDbConnection Open() => _factory.Create();

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(Guid userId)
    {
        const string sql = @"
SELECT Id, UserId, Title, IsDone, CreatedAtUtc, UpdatedAtUtc
FROM dbo.Todos
WHERE UserId = @UserId
ORDER BY CreatedAtUtc DESC;";

        using var db = Open();
        var rows = await db.QueryAsync<TodoItem>(sql, new { UserId = userId });
        return rows.AsList();
    }

    public async Task<TodoItem?> GetByIdAsync(Guid userId, int id)
    {
        const string sql = @"
SELECT Id, UserId, Title, IsDone, CreatedAtUtc, UpdatedAtUtc
FROM dbo.Todos
WHERE UserId = @UserId AND Id = @Id;";

        using var db = Open();
        return await db.QuerySingleOrDefaultAsync<TodoItem>(sql, new { UserId = userId, Id = id });
    }

    public async Task<int> CreateAsync(Guid userId, string title)
    {
        const string sql = @"
INSERT INTO dbo.Todos (UserId, Title)
VALUES (@UserId, @Title);
SELECT CAST(SCOPE_IDENTITY() as int);";

        using var db = Open();
        return await db.ExecuteScalarAsync<int>(sql, new { UserId = userId, Title = title });
    }

    public async Task<bool> UpdateAsync(Guid userId, int id, string title, bool isDone)
    {
        const string sql = @"
UPDATE dbo.Todos
SET Title = @Title,
    IsDone = @IsDone,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE UserId = @UserId AND Id = @Id;";

        using var db = Open();
        var affected = await db.ExecuteAsync(sql, new
        {
            UserId = userId,
            Id = id,
            Title = title,
            IsDone = isDone
        });

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid userId, int id)
    {
        const string sql = @"
DELETE FROM dbo.Todos
WHERE UserId = @UserId AND Id = @Id;";

        using var db = Open();
        var affected = await db.ExecuteAsync(sql, new { UserId = userId, Id = id });
        return affected > 0;
    }
}
