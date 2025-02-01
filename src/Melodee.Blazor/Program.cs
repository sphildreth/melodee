using Blazored.SessionStorage;
using Melodee.Blazor.Components;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Middleware;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Jobs;
using Melodee.Common.MessageBus.EventHandlers;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.Scrobbling;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.AspNetCore;
using Radzen;
using Rebus.Compression;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Transport.InMem;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ILogger = Serilog.ILogger;

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

builder.Services.AddDbContextFactory<ArtistSearchEngineServiceDbContext>(opt 
    => opt.UseSqlite(builder.Configuration.GetConnectionString("ArtistSearchEngineConnection")));

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new OrmLiteConnectionFactory(builder.Configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddAntiforgery(opt =>
{
    opt.Cookie.Name = "melodee_csrf";
    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddRadzenComponents();

builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "melodee_ui_theme";
    options.Duration = TimeSpan.FromDays(9999);
});

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
    .AddScoped<AlbumSearchEngineService>()
    .AddScoped<ArtistSearchEngineService>()
    .AddScoped<StatisticsService>()
    .AddScoped<SearchService>()
    .AddScoped<ShareService>()
    .AddScoped<PlaylistService>();

#endregion

#region Quartz Related

builder.Services.AddQuartz(q => { q.UseTimeZoneConverter(); });
builder.Services.AddSingleton<IScheduler>(provider =>
{
    var factory = provider.GetRequiredService<ISchedulerFactory>();
    var scheduler = factory.GetScheduler().Result;
    return scheduler;
});

builder.Services.AddQuartzServer(opts => { opts.WaitForJobsToComplete = true; });

#endregion

builder.Services.AddRebus((configurer, provider) =>
{
    return configurer
        .Logging(l => l.Serilog(provider.GetRequiredService<ILogger>()))
        .Options(o =>
        {
            o.EnableCompression(32768);
            o.SetNumberOfWorkers(2);
            o.SetMaxParallelism(20);
        })
        .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "melodee_bus"))
        .Sagas(s => s.StoreInMemory())
        .Timeouts(t => t.StoreInMemory());
});
builder.Services.AddRebusHandler<AlbumUpdatedEventHandler>();
builder.Services.AddRebusHandler<SearchHistoryEventHandler>();
builder.Services.AddRebusHandler<UserLoginEventHandler>();
builder.Services.AddRebusHandler<UserStreamEventHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Error");

//app.UseHttpsRedirection();

#region Scheduling Quartz Jobs with Configuration

var isQuartzDisabled = SafeParser.ToBoolean(builder.Configuration["QuartzDisabled"]);
if (!isQuartzDisabled)
{
    var quartzScheduler = app.Services.GetRequiredService<IScheduler>();
    var melodeeConfigurationFactory = app.Services.GetRequiredService<IMelodeeConfigurationFactory>();
    var melodeeConfiguration = await melodeeConfigurationFactory.GetConfigurationAsync();

    var artistHousekeepingCronExpression = melodeeConfiguration.GetValue<string>(SettingRegistry.JobsArtistHousekeepingCronExpression);
    if (artistHousekeepingCronExpression.Nullify() != null)
    {
        await quartzScheduler.ScheduleJob(
            JobBuilder.Create<ArtistHousekeepingJob>()
                .WithIdentity(JobKeyRegistry.ArtistHousekeepingJobJobKey)
                .Build(),
            TriggerBuilder.Create()
                .WithIdentity("ArtistHousekeepingJobJobKey-trigger")
                .WithCronSchedule(artistHousekeepingCronExpression!)
                .StartNow()
                .Build());
    }

    var libraryInboundProcessJobKeyCronExpression = melodeeConfiguration.GetValue<string>(SettingRegistry.JobsLibraryProcessCronExpression);
    if (libraryInboundProcessJobKeyCronExpression.Nullify() != null)
    {
        await quartzScheduler.ScheduleJob(
            JobBuilder.Create<LibraryInboundProcessJob>()
                .WithIdentity(JobKeyRegistry.LibraryInboundProcessJobKey)
                .Build(),
            TriggerBuilder.Create()
                .WithIdentity("LibraryInboundProcessJob-trigger")
                .UsingJobData(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString())
                .UsingJobData(JobMapNameRegistry.Count, 0)
                .WithCronSchedule(libraryInboundProcessJobKeyCronExpression!)
                .StartNow()
                .Build());
    }

    var libraryInsertCronExpression = melodeeConfiguration.GetValue<string>(SettingRegistry.JobsLibraryInsertCronExpression);
    if (libraryInsertCronExpression.Nullify() != null)
    {
        await quartzScheduler.ScheduleJob(
            JobBuilder.Create<LibraryInsertJob>()
                .WithIdentity(JobKeyRegistry.LibraryProcessJobJobKey)
                .Build(),
            TriggerBuilder.Create()
                .WithIdentity("LibraryProcessJob-trigger")
                .UsingJobData(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString())
                .UsingJobData(JobMapNameRegistry.Count, 0)
                .WithCronSchedule(libraryInsertCronExpression!)
                .StartNow()
                .Build());
    }

    var musicBrainzUpdateDatabaseCronExpression = melodeeConfiguration.GetValue<string>(SettingRegistry.JobsMusicBrainzUpdateDatabaseCronExpression);
    if (musicBrainzUpdateDatabaseCronExpression.Nullify() != null)
    {
        await quartzScheduler.ScheduleJob(
            JobBuilder.Create<MusicBrainzUpdateDatabaseJob>()
                .WithIdentity(JobKeyRegistry.MusicBrainzUpdateDatabaseJobKey)
                .Build(),
            TriggerBuilder.Create()
                .WithIdentity("MusicBrainzUpdateDatabaseJob-trigger")
                .WithCronSchedule(musicBrainzUpdateDatabaseCronExpression!)
                .StartNow()
                .Build());
    }
}

#endregion

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
