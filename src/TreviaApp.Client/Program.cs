using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TreviaApp.Client;
using TreviaApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IPwaInstallPromptService, PwaInstallPromptService>();
builder.Services.AddScoped<IWorkoutOfflineStorage, IndexedDbWorkoutStorage>();
builder.Services.AddScoped<ISyncQueue, IndexedDbSyncQueue>();
builder.Services.AddScoped<ICurrentUserIdProvider, DefaultCurrentUserIdProvider>();
builder.Services.AddHostedService<SyncBackgroundService>();

var host = builder.Build();

try
{
    var installService = host.Services.GetRequiredService<IPwaInstallPromptService>();
    if (installService is PwaInstallPromptService initService)
    {
        await initService.InitializeAsync();
    }
}
catch (Exception)
{
}

await host.RunAsync();
