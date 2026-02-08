namespace KubeCart.Identity.Api.Contracts.Auth;

public sealed class AuthResponse
{
    public string UserId { get; set; } = string.Empty;   // GUID string
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
