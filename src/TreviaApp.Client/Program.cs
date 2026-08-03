using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TreviaApp.Client;
using TreviaApp.Client.Services;
using TreviaApp.Client.Services.Auth;
using TreviaApp.Client.Services.Coaching;
using TreviaApp.Client.Services.Consents;
using TreviaApp.Client.Services.Feedbacks;
using TreviaApp.Client.Services.Notifications;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

var apiBase = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrEmpty(apiBase))
{
    var baseAddr = new Uri(builder.HostEnvironment.BaseAddress);
    apiBase = baseAddr.Scheme == "https" || baseAddr.Host == "localhost"
        ? builder.HostEnvironment.BaseAddress
        : "http://localhost:5000/";
}
if (!apiBase.EndsWith('/')) apiBase += '/';

builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddHttpClient("TreviaApp.AnonymousApi", client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddHttpClient("TreviaApp.Api", client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = TimeSpan.FromSeconds(300);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserIdProvider, AuthCurrentUserIdProvider>();

builder.Services.AddScoped<IPwaInstallPromptService, PwaInstallPromptService>();
builder.Services.AddScoped<IWorkoutOfflineStorage, IndexedDbWorkoutStorage>();
builder.Services.AddScoped<ISyncQueue, IndexedDbSyncQueue>();

builder.Services.AddScoped<IProfileService, ProfilesApiService>();
builder.Services.AddScoped<IExerciseService, ExercisesApiService>();
builder.Services.AddScoped<ITrainingPlansService, TrainingPlansApiService>();
builder.Services.AddScoped<IWorkoutsService, WorkoutsApiService>();
builder.Services.AddScoped<IGamificationService, GamificationApiService>();
builder.Services.AddScoped<IReportsService, ReportsApiService>();
builder.Services.AddScoped<ICoachingService, CoachingApiService>();
builder.Services.AddScoped<IConsentsService, ConsentsApiService>();
builder.Services.AddScoped<INotificationsService, NotificationsApiService>();
builder.Services.AddScoped<IFeedbacksService, FeedbacksApiService>();

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
catch
{
}

await host.RunAsync();
