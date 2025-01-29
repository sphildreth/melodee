using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
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
///     Clean library of folders that don't add value.
/// </summary>
public class LibraryCleanCommand : AsyncCommand<LibraryCleanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryCleanSettings settings)
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

        var services = new ServiceCollection();
        services.AddDbContextFactory<MelodeeDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o => o.UseNodaTime()));
        services.AddHttpClient();
        services.AddSingleton<IDbConnectionFactory>(opt =>
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
            var bus = scope.ServiceProvider.GetRequiredService<IBus>();

            var libraryService = new LibraryService(Log.Logger,
                cacheManager,
                dbFactory,
                configFactory,
                serializer,
                bus);
            var configurationFactory = new MelodeeConfigurationFactory(dbFactory);

            var result = await libraryService.CleanLibraryAsync(settings.LibraryName);

            if (settings.Verbose)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("Process Result")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }

            return 1;
        }
    }
}
