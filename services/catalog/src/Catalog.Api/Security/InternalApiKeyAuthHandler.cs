using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace KubeCart.Catalog.Api.Security;

public sealed class InternalApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string HeaderName = "X-Internal-Api-Key";

    public InternalApiKeyAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : base(options, logger, encoder) { }


    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Only authenticate if header exists (so public endpoints don't get forced)
        if (!Request.Headers.TryGetValue(HeaderName, out var provided))
            return Task.FromResult(AuthenticateResult.NoResult());

        var expected =
            Environment.GetEnvironmentVariable("INTERNAL_API_KEY")
            ?? Context.RequestServices.GetRequiredService<IConfiguration>()["InternalApiKey"];

        if (string.IsNullOrWhiteSpace(expected))
            return Task.FromResult(AuthenticateResult.Fail("Internal API key not configured."));

        if (!string.Equals(provided.ToString(), expected, StringComparison.Ordinal))
            return Task.FromResult(AuthenticateResult.Fail("Invalid internal API key."));

        var claims = new[] { new Claim(ClaimTypes.Name, "internal-client") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
