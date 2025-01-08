using Blazored.SessionStorage;
using Melodee.Blazor.Components;
using Melodee.Blazor.Extensions;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Middleware;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Jobs;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models;
using Melodee.Common.Plugins.Scrobbling;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.EventHandlers;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.AspNetCore;
using Radzen;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, loggerConfiguration)
    => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers(options => { options.Filters.Add<ETagFilter>(); });

builder.Services.AddDbContextFactory<MelodeeDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), o
        => o.UseNodaTime()
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new OrmLiteConnectionFactory(builder.Configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddAntiforgery(opt =>
{
    opt.Cookie.Name = "melodee_csrf";
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddInMemoryEvent<UserLoginEvent, UserLoginEventHandler>();
builder.Services.AddInMemoryEvent<AlbumUpdatedEvent, AlbumUpdatedEventHandler>();

builder.Services.AddRadzenComponents();

builder.Services.AddBlazoredSessionStorage();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(x =>
    {
        x.Cookie.SameSite = SameSiteMode.Strict;
        x.Cookie.Name = "melodee_auth";
    });
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

#region Melodee Services

builder.Services
    .AddScoped<MainLayoutProxyService>()
    .AddSingleton<ISerializer, Serializer>()
    .AddSingleton<ICacheManager>(opt
        => new MemoryCacheManager(opt.GetRequiredService<Serilog.ILogger>(),
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
    .AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>()
    .AddSingleton<EtagRepository>()
    .AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>()
    .AddScoped<SettingService>()
    .AddScoped<ArtistService>()
    .AddScoped<AlbumService>()
    .AddScoped<SongService>()
    .AddScoped<ScrobbleService>()
    .AddScoped<LibraryService>()
    .AddScoped<UserService>()
    .AddScoped<AlbumDiscoveryService>()
    .AddScoped<MediaEditService>()
    .AddScoped<DirectoryProcessorService>()
    .AddScoped<ImageConversionService>()
    .AddScoped<OpenSubsonicApiService>()
    .AddScoped<AlbumImageSearchEngineService>()
    .AddScoped<ArtistImageSearchEngineService>()
    .AddScoped<ArtistSearchEngineService>()
    .AddScoped<StatisticsService>();
#endregion

#region Quartz Related

builder.Services.AddQuartz(q =>
{
    q.UseTimeZoneConverter();

    q.AddJob<LibraryInboundProcessJob>(opts => opts.WithIdentity(JobKeyRegistry.LibraryInboundProcessJobKey));
    q.AddTrigger(opts => opts
            .ForJob(JobKeyRegistry.LibraryInboundProcessJobKey)
            .WithIdentity("LibraryInboundProcessJob-trigger")
            .UsingJobData(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString())
            .UsingJobData(JobMapNameRegistry.Count, 0)
            .WithCronSchedule("0 0/10 * * * ?") // Every 10 minutes
    );

    q.AddJob<LibraryInsertJob>(opts => opts.WithIdentity(JobKeyRegistry.LibraryProcessJobJobKey));
    q.AddTrigger(opts => opts
            .ForJob(JobKeyRegistry.LibraryProcessJobJobKey)
            .WithIdentity("LibraryProcessJob-trigger")
            .UsingJobData(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString())
            .UsingJobData(JobMapNameRegistry.Count, 0)
            .WithCronSchedule("0 0 * * * ?") // Once a day, at 00:00
    );

    q.AddJob<MusicBrainzUpdateDatabaseJob>(opts => opts.WithIdentity(JobKeyRegistry.MusicBrainzUpdateDatabaseJobKey));
    q.AddTrigger(opts => opts
            .ForJob(JobKeyRegistry.MusicBrainzUpdateDatabaseJobKey)
            .WithIdentity("MusicBrainzUpdateDatabaseJob-trigger")
            .WithCronSchedule("0 0 1 * * ?") // Once a month, at 00:00
    );

    q.AddJob<ArtistHousekeepingJob>(opts => opts.WithIdentity(JobKeyRegistry.ArtistHousekeepingJobJobKey));
    q.AddTrigger(opts => opts
            .ForJob(JobKeyRegistry.ArtistHousekeepingJobJobKey)
            .WithIdentity("ArtistHousekeepingJobJobKey-trigger")
            .WithCronSchedule("0 * * * * ?") // Every hour
    );
});
builder.Services.AddSingleton<IScheduler>(provider =>
{
    var factory = provider.GetRequiredService<ISchedulerFactory>();
    var scheduler = factory.GetScheduler().Result;
    return scheduler;
});
builder.Services.AddQuartzServer(opts => { opts.WaitForJobsToComplete = true; });

#endregion

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Error");

//app.UseHttpsRedirection();

app.Services.StartConsumersAsync();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict
});

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    };
});

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseCors(
    options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
);

app.UseMelodeeBlazorHeader();

app.MapControllers();

app.Run();
