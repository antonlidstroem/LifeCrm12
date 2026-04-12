using System.Text;
using LifeCrm.Api.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace LifeCrm.Api.Extensions;

public static class JwtServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("Jwt");
        var secretKey   = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        if (Encoding.UTF8.GetByteCount(secretKey) < 32) throw new InvalidOperationException("Jwt:SecretKey must be at least 32 bytes.");
        services.AddAuthentication(opts => { opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = jwtSettings["Issuer"] ?? "LifeCrm", ValidateAudience = true, ValidAudience = jwtSettings["Audience"] ?? "LifeCrmWeb", ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1) };
                opts.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs")) ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });
        return services;
    }
}
