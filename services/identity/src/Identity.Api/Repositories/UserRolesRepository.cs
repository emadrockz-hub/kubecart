using Dapper;
using KubeCart.Identity.Api.Data;
using Microsoft.Data.SqlClient;

namespace KubeCart.Identity.Api.Repositories;

public sealed class UserRolesRepository
{
    private readonly DbConnectionFactory _db;

    public UserRolesRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task AssignAsync(Guid userId, int roleId, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @RoleId
)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
END
";
        await using SqlConnection conn = _db.Create();
        await conn.ExecuteAsync(new CommandDefinition(sql, new { UserId = userId, RoleId = roleId }, cancellationToken: ct));
    }

    public async Task<string[]> GetRoleNamesForUserAsync(Guid userId, CancellationToken ct)
    {
        const string sql = @"
SELECT r.Name
FROM dbo.UserRoles ur
JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE ur.UserId = @UserId
ORDER BY r.Name;
";
        await using SqlConnection conn = _db.Create();
        var rows = await conn.QueryAsync<string>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
        return rows.ToArray();
    }
}
