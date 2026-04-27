// src/LifeCrm.Web/Program.cs
using Blazored.LocalStorage;
using LifeCrm.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace LifeCrm.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // ── HttpClient ──────────────────────────────────────────────────────
        var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
            ?? throw new Exception("ApiBaseUrl is not configured in wwwroot/appsettings.json");

        builder.Services.AddScoped(_ => new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/")
        });

        // ── MudBlazor ───────────────────────────────────────────────────────
        builder.Services.AddMudServices(cfg =>
        {
            cfg.SnackbarConfiguration.PositionClass        = MudBlazor.Defaults.Classes.Position.BottomRight;
            cfg.SnackbarConfiguration.PreventDuplicates    = false;
            cfg.SnackbarConfiguration.NewestOnTop          = true;
            cfg.SnackbarConfiguration.ShowCloseIcon        = true;
            cfg.SnackbarConfiguration.VisibleStateDuration = 4000;
        });

        // ── Local Storage ───────────────────────────────────────────────────
        builder.Services.AddBlazoredLocalStorage();

        // ── Domain API clients ──────────────────────────────────────────────
        builder.Services.AddScoped<AuthApiClient>();
        builder.Services.AddScoped<ContactsApiClient>();
        builder.Services.AddScoped<DonationsApiClient>();
        builder.Services.AddScoped<CampaignsApiClient>();
        builder.Services.AddScoped<ProjectsApiClient>();
        builder.Services.AddScoped<InteractionsApiClient>();
        builder.Services.AddScoped<UsersApiClient>();
        builder.Services.AddScoped<DashboardApiClient>();
        builder.Services.AddScoped<ReportsApiClient>();
        builder.Services.AddScoped<NewslettersApiClient>();
        builder.Services.AddScoped<SignalRSettingsApiClient>();
        builder.Services.AddScoped<EmailSettingsApiClient>();

        // ── Façade — must be registered AFTER domain clients ────────────────
        builder.Services.AddScoped<ApiClient>();

        // ── FIX E: Reference data cache (Scoped = one per browser tab) ──────
        builder.Services.AddScoped<ReferenceDataCache>();
        builder.Services.AddScoped<CachedReferenceDataService>();

        // ── App-level services ──────────────────────────────────────────────
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddScoped<SignalRService>();

        await builder.Build().RunAsync();
    }
}
