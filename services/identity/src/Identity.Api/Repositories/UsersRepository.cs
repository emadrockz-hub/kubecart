using Dapper;
using KubeCart.Identity.Api.Data;
using Microsoft.Data.SqlClient;

namespace KubeCart.Identity.Api.Repositories;

public sealed class UsersRepository
{
    private readonly DbConnectionFactory _db;

    public UsersRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<UserRow?> GetByEmailAsync(string email, CancellationToken ct)
    {
        const string sql = @"
SELECT TOP 1
    Id,
    Email,
    PasswordHash,
    PasswordSalt
FROM dbo.Users
WHERE Email = @Email;
";
        await using SqlConnection conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: ct));
    }

    public async Task<Guid> InsertAsync(Guid userId, string email, byte[] passwordHash, byte[] passwordSalt, CancellationToken ct)
    {
        const string sql = @"
INSERT INTO dbo.Users (Id, Email, PasswordHash, PasswordSalt)
VALUES (@Id, @Email, @PasswordHash, @PasswordSalt);
";
        await using SqlConnection conn = _db.Create();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = userId,
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        }, cancellationToken: ct));

        return userId;
    }

    public sealed class UserRow
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    }
}
