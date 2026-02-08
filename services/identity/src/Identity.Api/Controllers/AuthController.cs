using KubeCart.Identity.Api.Contracts.Auth;
using KubeCart.Identity.Api.Repositories;
using KubeCart.Identity.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KubeCart.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UsersRepository _users;
    private readonly RolesRepository _roles;
    private readonly UserRolesRepository _userRoles;
    private readonly IConfiguration _config;

    public AuthController(
        UsersRepository users,
        RolesRepository roles,
        UserRolesRepository userRoles,
        IConfiguration config)
    {
        _users = users;
        _roles = roles;
        _userRoles = userRoles;
        _config = config;
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        var userId =
            User.FindFirstValue("userId")
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var email =
            User.FindFirstValue("email")
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(ClaimTypes.Email);

        return Ok(new
        {
            userId,
            email
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/ping")]
    public ActionResult<object> AdminPing()
    {
        return Ok(new { ok = true, message = "Admin access granted." });
    }


    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var email = request.Email.Trim();

        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing is not null)
            return Conflict("Email already exists.");

        var (hash, salt) = PasswordHasher.HashPassword(request.Password);
        var userId = Guid.NewGuid();

        await _users.InsertAsync(userId, email, hash, salt, ct);

        // Assign default role
        var customerRoleId = await _roles.EnsureRoleAsync("Customer", ct);
        await _userRoles.AssignAsync(userId, customerRoleId, ct);

        // Fetch roles and issue token
        var roles = await _userRoles.GetRoleNamesForUserAsync(userId, ct);
        var token = CreateToken(userId, email, roles);

        return Ok(new AuthResponse
        {
            UserId = userId.ToString(),
            Email = email,
            Token = token
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var email = request.Email.Trim();

        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null)
            return Unauthorized("Invalid credentials.");

        var ok = PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
        if (!ok)
            return Unauthorized("Invalid credentials.");

        var roles = await _userRoles.GetRoleNamesForUserAsync(user.Id, ct);
        var token = CreateToken(user.Id, user.Email, roles);

        return Ok(new AuthResponse
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            Token = token
        });
    }

    private string CreateToken(Guid userId, string email, string[] roles)
    {
        var jwt = _config.GetSection("Jwt");
        var issuer = jwt["Issuer"]!;
        var audience = jwt["Audience"]!;
        var expiresMinutes = int.Parse(jwt["ExpiresMinutes"]!);
        var signingKey = jwt["SigningKey"]!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("email", email),
            new("userId", userId.ToString())
        };

        // C2: add role claims here
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("dev/grant-admin")]
    public async Task<ActionResult> GrantAdmin([FromQuery] string email, CancellationToken ct)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
            return NotFound();

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("email is required.");

        var user = await _users.GetByEmailAsync(email.Trim(), ct);
        if (user is null)
            return NotFound("User not found.");

        var adminRoleId = await _roles.EnsureRoleAsync("Admin", ct);
        await _userRoles.AssignAsync(user.Id, adminRoleId, ct);

        return NoContent();
    }
}
