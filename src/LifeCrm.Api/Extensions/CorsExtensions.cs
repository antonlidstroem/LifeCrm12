namespace LifeCrm.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddCors(opts => opts.AddPolicy("LifeCrmPolicy", policy =>
        {
            policy
                .WithOrigins(
                    "https://localhost:7001",
                    "http://localhost:5001",
                    "https://localhost:5001")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }));
        return services;
    }
}
