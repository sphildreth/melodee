using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Transport.InMem;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

/// <summary>
///     Validates a given album
/// </summary>
public class ValidateCommand : AsyncCommand<ValidateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ValidateSettings settings)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var serializer = new Serializer(Log.Logger);
        var cacheManager = new MemoryCacheManager(Log.Logger, TimeSpan.FromDays(1), serializer);

        var isValid = false;

        var services = new ServiceCollection();
        services.AddDbContextFactory<MelodeeDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o => o.UseNodaTime()));
        services.AddHttpClient();
        services.AddSingleton<IDbConnectionFactory>(_ =>
            new OrmLiteConnectionFactory(configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));
        services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        services.AddSingleton(Log.Logger);
        services.AddRebus(configure =>
        {
            return configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "melodee_bus"));
        });

        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
            var configFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();

            var config = await configFactory.GetConfigurationAsync();

            var albumValidator = new AlbumValidator(config);

            Album? album = null;
            if (settings is { LibraryName: not null, Id: not null })
            {
                var bus = scope.ServiceProvider.GetRequiredService<IBus>();

                var libraryService = new LibraryService(Log.Logger,
                    cacheManager,
                    dbFactory,
                    configFactory,
                    serializer,
                    bus);

                var libraryListResult = await libraryService.ListAsync(new PagedRequest()).ConfigureAwait(false);
                var library = libraryListResult.Data.FirstOrDefault(x => x.Name == settings.LibraryName);
                if (library == null)
                {
                    Log.Logger.Error("Could not find library named {LibraryName}", settings.LibraryName);
                    return 0;
                }

                var albumDiscoveryService = new AlbumDiscoveryService(
                    Log.Logger,
                    cacheManager,
                    dbFactory,
                    configFactory,
                    serializer);
                await albumDiscoveryService.InitializeAsync();
                album = await albumDiscoveryService.AlbumByUniqueIdAsync(library.ToFileSystemDirectoryInfo(), settings.Id.Value);
            }
            else if (settings.ApiKey != null)
            {
                var albumService = new AlbumService(Log.Logger, cacheManager, dbFactory);
                var albumResult = await albumService.GetByApiKeyAsync(SafeParser.ToGuid(settings.ApiKey)!.Value).ConfigureAwait(false);
                if (albumResult.IsSuccess)
                {
                    album = await Album.DeserializeAndInitializeAlbumAsync(serializer, Path.Combine(albumResult.Data!.Directory, "melodee.json")).ConfigureAwait(false);
                }
            }
            else if (settings.PathToMelodeeDataFile != null)
            {
                album = await Album.DeserializeAndInitializeAlbumAsync(serializer, Path.Combine(settings.PathToMelodeeDataFile)).ConfigureAwait(false);
            }

            if (album != null)
            {
                var validationResult = albumValidator.ValidateAlbum(album);
                isValid = validationResult.IsSuccess;
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(validationResult) ?? string.Empty))
                        .Header("Validation Result")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Red));
            }

            return isValid ? 0 : 1;
        }
    }
}
