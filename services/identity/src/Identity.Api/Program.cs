using KubeCart.Identity.Api.Health;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<KubeCart.Identity.Api.Data.DbConnectionFactory>();
builder.Services.AddScoped<KubeCart.Identity.Api.Repositories.DbPingRepository>();
builder.Services.AddScoped<KubeCart.Identity.Api.Repositories.UsersRepository>();
builder.Services.AddScoped<KubeCart.Identity.Api.Repositories.RolesRepository>();
builder.Services.AddScoped<KubeCart.Identity.Api.Repositories.UserRolesRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KubeCart.Identity.Api", Version = "v1" });

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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var signingKey = jwtSection["SigningKey"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Health checks:
// - live: always healthy (no dependencies)
// - ready: checks DB connectivity using our custom check
builder.Services.AddHealthChecks()
    .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck<SqlConnectionHealthCheck>("db", tags: new[] { "ready" });

var app = builder.Build();
// Seed roles in Development (idempotent)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var rolesRepo = scope.ServiceProvider.GetRequiredService<KubeCart.Identity.Api.Repositories.RolesRepository>();

    // fire and wait (top-level statements)
    await rolesRepo.EnsureRoleAsync("Admin", CancellationToken.None);
    await rolesRepo.EnsureRoleAsync("Customer", CancellationToken.None);
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // IMPORTANT
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
