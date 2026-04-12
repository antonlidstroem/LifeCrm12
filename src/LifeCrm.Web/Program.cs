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

        var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
        builder.Services.AddScoped(sp =>
        {
            var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
            var baseAddress = !string.IsNullOrWhiteSpace(apiBaseUrl)
                ? apiBaseUrl.TrimEnd('/') + "/"
                : nav.BaseUri;
            return new HttpClient { BaseAddress = new Uri(baseAddress) };
        });

        builder.Services.AddMudServices(cfg =>
        {
            cfg.SnackbarConfiguration.PositionClass        = MudBlazor.Defaults.Classes.Position.BottomRight;
            cfg.SnackbarConfiguration.PreventDuplicates    = false;
            cfg.SnackbarConfiguration.NewestOnTop          = true;
            cfg.SnackbarConfiguration.ShowCloseIcon        = true;
            cfg.SnackbarConfiguration.VisibleStateDuration = 4000;
        });

        builder.Services.AddBlazoredLocalStorage();

        // Domain API clients
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

        // Façade — registered after domain clients so DI can inject them
        builder.Services.AddScoped<ApiClient>();

        // App-level services
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddScoped<SignalRService>();

        await builder.Build().RunAsync();
    }
}
