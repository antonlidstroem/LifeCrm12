using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace LifeCrm.Api.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ctx.User.Identity?.Name ?? ctx.Request.Headers.Host.ToString(),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit       = 300,
                        Window            = TimeSpan.FromMinutes(1)
                    }));
            opts.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Rate limit exceeded.", ct);
            };
        });
        return services;
    }
}
