using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LifeCrm.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuth(
        this IServiceCollection services, IConfiguration config)
    {
        var secret  = config["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured.");
        var issuer   = config["Jwt:Issuer"]   ?? "LifeCrm";
        var audience = config["Jwt:Audience"] ?? "LifeCrmUsers";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer           = true,
                    ValidIssuer              = issuer,
                    ValidateAudience         = true,
                    ValidAudience            = audience,
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                };

                // Allow SignalR to pass token in query string
                opts.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(opts =>
        {
            opts.AddPolicy("CanWrite",       p => p.RequireRole("Manager", "Finance", "Admin"));
            opts.AddPolicy("FinanceOrAdmin", p => p.RequireRole("Finance", "Admin"));
            opts.AddPolicy("AdminOnly",      p => p.RequireRole("Admin"));
        });

        return services;
    }
}
