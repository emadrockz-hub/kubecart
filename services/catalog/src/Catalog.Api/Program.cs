// services/catalog/src/Catalog.Api/Program.cs

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Catalog.Api.Health;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using KubeCart.Catalog.Api.Security; // <-- must match your InternalApiKeyAuthHandler namespace

var builder = WebApplication.CreateBuilder(args);

// Build ConnectionStrings:Default from env vars when running in Kubernetes
// (ConfigMap/Secret contract: DB_HOST, DB_NAME, DB_USER, DB_PASSWORD)
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (!string.IsNullOrWhiteSpace(dbHost) &&
    !string.IsNullOrWhiteSpace(dbName) &&
    !string.IsNullOrWhiteSpace(dbUser) &&
    !string.IsNullOrWhiteSpace(dbPassword))
{
    builder.Configuration["ConnectionStrings:Default"] =
        $"Server={dbHost};Database={dbName};User Id={dbUser};Password={dbPassword};Encrypt=False;TrustServerCertificate=True;";
}

// Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.WriteIndented = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KubeCart.Catalog.Api", Version = "v1" });
});

// DI
builder.Services.AddSingleton<KubeCart.Catalog.Api.Data.DbConnectionFactory>();
builder.Services.AddScoped<KubeCart.Catalog.Api.Repositories.DbPingRepository>();
builder.Services.AddScoped<KubeCart.Catalog.Api.Repositories.CategoryRepository>();
builder.Services.AddScoped<KubeCart.Catalog.Api.Repositories.ProductRepository>();

// INTERNAL API KEY auth (for decrease-stock)
const string InternalScheme = "InternalApiKey";

builder.Services
    .AddAuthentication(InternalScheme)
    .AddScheme<AuthenticationSchemeOptions, InternalApiKeyAuthHandler>(
        InternalScheme, options => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InternalApi", policy =>
    {
        policy.AddAuthenticationSchemes(InternalScheme);
        policy.RequireAuthenticatedUser();
    });
});

// Health checks:
// - live: always healthy (no dependencies)
// - ready: checks DB connectivity using our custom check
builder.Services.AddHealthChecks()
    .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck<SqlConnectionHealthCheck>("db", tags: new[] { "ready" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Name == "live"
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.Run();
