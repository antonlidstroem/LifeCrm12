using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace LifeCrm.Api.Extensions;

public static class RateLimitServiceExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            opts.AddFixedWindowLimiter("login", o => { o.Window = TimeSpan.FromMinutes(5); o.PermitLimit = 10; o.QueueLimit = 0; o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; });
            opts.AddFixedWindowLimiter("api",   o => { o.Window = TimeSpan.FromMinutes(1); o.PermitLimit = 300; o.QueueLimit = 0; o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; });
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
        return services;
    }
}
