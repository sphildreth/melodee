using Blazored.SessionStorage;
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Melodee.Common.Data;
using Melodee.Common.Serialization;
using Melodee.Components;
using Melodee.Filters;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Interfaces;
using Melodee.Services.Scanning;
using Melodee.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, loggerConfiguration)
    => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<MelodeeDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseNodaTime()));

builder.Services.AddBlazorBootstrap();
builder.Services.AddHttpContextAccessor();

//The cookie authentication is never used (jwt is), but it is required to prevent a runtime error
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(x =>
    {
        x.Cookie.Name = "melodee_auth";
        x.Cookie.MaxAge = TimeSpan.FromDays(365);
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CookieStorageAccessor>();
builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddSingleton<ISerializer, Serializer>()
    .AddSingleton<ICacheManager>(opt
        => new MemoryCacheManager(opt.GetRequiredService<ILogger>(),
            new TimeSpan(1,
                0,
                0,
                0),
            opt.GetRequiredService<ISerializer>()))
    .AddScoped<LocalStorageService>()
    .AddScoped<SettingService>()
    .AddScoped<UserService>()
    .AddScoped<AlbumDiscoveryService>();

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<IStorageSessionService, StorageSessionService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<MainLayoutProxyService>();

builder.Services.AddTransient<IBackgroundProcess, ProcessMonitor>(x => new ProcessMonitor(checkInterval: TimeSpan.FromSeconds(10)));
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseSerilogLogProvider()
    .UseInMemoryStorage()
    .UseRecommendedSerializerSettings()
    .UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(10))
    .UseJobsLogger());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// This is used by the Hangfire auth to get the JWT cookie and see if the user has the admin credential. Yes it is ugly.
var jwtValidatorToken = builder.Configuration.GetValue<string>("MelodeeAuthSettings:Token")!;

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Melodee Hangfire Dashboard",
    Authorization = new [] { new MelodeeHangfireAuthorizationFilter(jwtValidatorToken) }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
