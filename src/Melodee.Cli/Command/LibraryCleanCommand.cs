using System.Net;
using Lucene.Net.Codecs;
using Mapster;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Serialization;
using Melodee.Jobs;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Plugins.Validation;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Scanning;
using Melodee.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

/// <summary>
/// Clean library of folders that don't add value. 
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
        
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
            var settingService = new SettingService(Log.Logger, cacheManager, dbFactory);
            var melodeeConfiguration = await settingService.GetMelodeeConfigurationAsync().ConfigureAwait(false);
            
            var libraryService = new LibraryService(Log.Logger,
                cacheManager,
                dbFactory,
                settingService,
                serializer,
                null);
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



