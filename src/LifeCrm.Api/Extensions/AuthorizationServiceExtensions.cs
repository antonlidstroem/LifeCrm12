using LifeCrm.Core.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace LifeCrm.Api.Extensions;

public static class AuthorizationServiceExtensions
{
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy("CanWrite",       p => p.RequireRole(Roles.Admin, Roles.Finance, Roles.Manager));
            opts.AddPolicy("FinanceOrAdmin", p => p.RequireRole(Roles.Admin, Roles.Finance));
            opts.AddPolicy("AdminOnly",      p => p.RequireRole(Roles.Admin));
        });
        return services;
    }
}
