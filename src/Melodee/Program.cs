using System.Net;
using System.Net.Mime;
using Blazored.SessionStorage;
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
using Melodee.Components;
using Melodee.Extensions;
using Melodee.Filters;
using Melodee.Services;
using Melodee.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.AspNetCore;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, loggerConfiguration)
    => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ETagFilter>();
});

builder.Services.AddDbContextFactory<MelodeeDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), o 
        => o.UseNodaTime()
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddSingleton<IDbConnectionFactory>(_ => 
    new OrmLiteConnectionFactory(builder.Configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));

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

builder.Services.AddInMemoryEvent<UserLoginEvent, UserLoginEventHandler>();
builder.Services.AddInMemoryEvent<AlbumUpdatedEvent, AlbumUpdatedEventHandler>();

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
    .AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>()
    .AddSingleton<EtagRepository>()
    .AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>()
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
    .AddScoped<AlbumImageSearchEngineService>()
    .AddScoped<ArtistImageSearchEngineService>()    
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
builder.Services.AddQuartzServer(opts =>
{
    opts.WaitForJobsToComplete = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
   // app.UseExceptionHandler("/Error", createScopeForErrors: true);
   app.UseExceptionHandler(exceptionHandlerApp =>
   {
       exceptionHandlerApp.Run(async context =>
       {
            var code =context.Response.StatusCode;
            if (code == (int)HttpStatusCode.NotFound)
            {
                Log.Error("404 [{Url}]", context.Request.Path);
            }
            context.Response.ContentType = MediaTypeNames.Text.Plain;
            await context.Response.WriteAsync("Doh! You found something that doesn't exist.");
       });
   });
}

app.Services.StartConsumersAsync();

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

app.MapControllers();


app.Run();
