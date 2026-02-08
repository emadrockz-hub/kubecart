using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Orders.Api.Health;
using System.Text;

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<KubeCart.Orders.Api.Repositories.CartRepository>();
builder.Services.AddScoped<KubeCart.Orders.Api.Repositories.CartItemRepository>();
builder.Services.AddScoped<Orders.Api.Repositories.Orders.IOrdersRepository, Orders.Api.Repositories.Orders.OrdersRepository>();

builder.Services.AddSingleton<KubeCart.Orders.Api.Data.DbConnectionFactory>();
builder.Services.AddScoped<KubeCart.Orders.Api.Repositories.DbPingRepository>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KubeCart.Orders.Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Catalog base URL (local dev + k8s) via env var
var catalogBaseUrl =
    Environment.GetEnvironmentVariable("CATALOG_SERVICE_URL")
    ?? builder.Configuration["CatalogService:BaseUrl"]
    ?? "http://localhost:5254";

// Internal API Key for service-to-service calls
var internalApiKey =
    Environment.GetEnvironmentVariable("INTERNAL_API_KEY")
    ?? builder.Configuration["InternalApiKey"];

// HttpClient for Catalog
builder.Services.AddHttpClient<KubeCart.Orders.Api.Clients.CatalogClient>(client =>
{
    client.BaseAddress = new Uri(catalogBaseUrl);

    if (!string.IsNullOrWhiteSpace(internalApiKey))
        client.DefaultRequestHeaders.Add("X-Internal-Api-Key", internalApiKey);
});

builder.Services.AddHealthChecks()
    .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck<SqlConnectionHealthCheck>("db", tags: new[] { "ready" });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        var signingKey = jwt["SigningKey"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

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
