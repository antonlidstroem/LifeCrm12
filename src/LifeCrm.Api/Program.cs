using LifeCrm.Api.Extensions;
using LifeCrm.Api.Hubs;
using LifeCrm.Api.Middleware;
using LifeCrm.Application;
using LifeCrm.Infrastructure;
using LifeCrm.Infrastructure.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddSwaggerDocs();
builder.Services.AddRateLimiting();
builder.Services.AddSignalR();

var app = builder.Build();

// Seed DB on startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LifeCrm API v1"));
}

app.UseHttpsRedirection();
app.UseCors("LifeCrmPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHub<ActivityHub>("/hubs/activity");

// Serve Blazor WASM
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
