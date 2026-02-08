using System.Security.Claims;

namespace Orders.Api.Security;

public static class UserContext
{
    public static Guid? TryGetUserId(ClaimsPrincipal user)
    {
        var raw =
            user.FindFirstValue("userId")
            ?? user.FindFirstValue("sub")
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static string? TryGetEmail(ClaimsPrincipal user)
    {
        return user.FindFirstValue("email")
            ?? user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue("preferred_username");
    }

    public static bool IsAdmin(ClaimsPrincipal user) =>
        user.IsInRole("Admin");
}
