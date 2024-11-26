using System.ComponentModel;
using Asp.Versioning;
using Blazored.SessionStorage;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Components;
using Melodee.Jobs;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Scrobbling;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Interfaces;
using Melodee.Services.Scanning;
using Melodee.Services.SearchEngines;
using Melodee.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using Quartz;
using Quartz.AspNetCore;
using Serilog;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, loggerConfiguration)
    => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

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

builder.Services.AddHttpClient();

builder.Services
    .AddSingleton<ISerializer, Serializer>()
    .AddSingleton<ICacheManager>(opt
        => new MemoryCacheManager(opt.GetRequiredService<ILogger>(),
            new TimeSpan(1,
                0,
                0,
                0),
            opt.GetRequiredService<ISerializer>()))
    .AddSingleton<DefaultImages>(_ => new DefaultImages
    {
        UserAvatarBytes = File.ReadAllBytes("wwwroot/images/avatar.png"),
        AlbumCoverBytes = File.ReadAllBytes("wwwroot/images/album.jpg"),
        ArtistBytes = File.ReadAllBytes("wwwroot/images/artist.jpg")
    })
    .AddSingleton<INowPlayingRepository, NowPlayingInMemoryRepository>()
    .AddScoped<LocalStorageService>()
    .AddScoped<ISettingService, SettingService>()
    .AddScoped<ArtistService>()
    .AddScoped<AlbumService>()
    .AddScoped<SongService>()
    .AddScoped<ScrobbleService>()
    .AddScoped<ILibraryService, LibraryService>()
    .AddScoped<UserService>()
    .AddScoped<AlbumDiscoveryService>()
    .AddScoped<MediaEditService>()
    .AddScoped<DirectoryProcessorService>()
    .AddScoped<ImageConversionService>()
    .AddScoped<OpenSubsonicApiService>()
    .AddScoped<ImageSearchEngineService>()
    .AddScoped<ArtistSearchEngineService>();
    
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<IStorageSessionService, StorageSessionService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<MainLayoutProxyService>();

builder.Services.AddQuartz(q =>
{
    q.UseTimeZoneConverter();
    
    q.AddJob<LibraryInboundProcessJob>(opts => opts.WithIdentity(JobKeyRegistry.LibraryInboundProcessJobKey));
    q.AddTrigger(opts => opts
        .ForJob(JobKeyRegistry.LibraryInboundProcessJobKey)
        .WithIdentity("LibraryInboundProcessJob-trigger")
        .UsingJobData(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString())
        .UsingJobData(JobMapNameRegistry.Count, 0)
        .WithCronSchedule("0 0/10 * * * ?")
    );
    
    q.AddJob<LibraryProcessJob>(opts => opts.WithIdentity(JobKeyRegistry.LibraryProcessJobJobKey));
    q.AddTrigger(opts => opts
        .ForJob(JobKeyRegistry.LibraryProcessJobJobKey)
        .WithIdentity("LibraryProcessJob-trigger")
        .UsingJobData(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString())
        .UsingJobData(JobMapNameRegistry.Count, 0)        
        .WithCronSchedule("0 0 * * * ?")
    );    
    
});
builder.Services.AddSingleton<IScheduler>(provider =>
{
    var factory = provider.GetRequiredService<ISchedulerFactory>();
    var scheduler = factory.GetScheduler().Result;
    return scheduler;
});
builder.Services.AddQuartzServer(opts =>
{
    opts.WaitForJobsToComplete = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseSerilogRequestLogging();

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
