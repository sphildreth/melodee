using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Metadata;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Transport.InMem;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

public abstract class CommandBase<T> : AsyncCommand<T> where T : Spectre.Console.Cli.CommandSettings
{
    protected IConfigurationRoot Configuration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();
    }

    protected ServiceProvider CreateServiceProvider()
    {
        var configuration = Configuration();
        var services = new ServiceCollection();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        services.AddSingleton<ISerializer, Serializer>();
        services.AddHttpClient();
        services.AddDbContextFactory<MelodeeDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o => o.UseNodaTime()));
        services.AddSingleton<IDbConnectionFactory>(opt =>
            new OrmLiteConnectionFactory(configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));
        services.AddDbContextFactory<ArtistSearchEngineServiceDbContext>(opt
            => opt.UseSqlite(configuration.GetConnectionString("ArtistSearchEngineConnection")));
        services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        services.AddSingleton<ICacheManager>(opt
            => new MemoryCacheManager(opt.GetRequiredService<ILogger>(),
                new TimeSpan(1,
                    0,
                    0,
                    0),
                opt.GetRequiredService<ISerializer>()));
        services.AddSingleton(Log.Logger);
        services.AddRebus(configure =>
        {
            return configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "melodee_bus"));
        });

        services.AddScoped<AlbumDiscoveryService>();
        services.AddScoped<AlbumImageSearchEngineService>();
        services.AddScoped<ArtistSearchEngineService>();
        services.AddScoped<DirectoryProcessorService>();
        services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        services.AddScoped<LibraryService>();
        services.AddScoped<MediaEditService>();
        services.AddScoped<MelodeeMetadataMaker>();
        services.AddScoped<SettingService>();
        services.AddScoped<ArtistService>();
        services.AddScoped<AlbumService>();

        return services.BuildServiceProvider();
    }
}
