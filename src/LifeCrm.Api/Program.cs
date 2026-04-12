using LifeCrm.Api.Extensions;
using LifeCrm.Api.Hubs;
using LifeCrm.Api.Middleware;
using LifeCrm.Application;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure;
using LifeCrm.Infrastructure.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiAuthorization();
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddApiSwagger();
builder.Services.AddApiRateLimiting();

builder.Services.AddSignalR(opts => opts.EnableDetailedErrors = builder.Environment.IsDevelopment());
builder.Services.AddSingleton<IActivityNotifier, ActivityNotifier>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "LifeCrm API v1"); c.RoutePrefix = "swagger"; });
}
else { app.UseHsts(); }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseBlazorFrameworkFiles();
app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("api");
app.MapHub<ActivityHub>("/hubs/activity").RequireAuthorization();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
