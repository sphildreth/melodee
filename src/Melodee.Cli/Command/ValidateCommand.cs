using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Plugins.Validation;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

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
        services.AddSingleton<IDbConnectionFactory>(opt => 
            new OrmLiteConnectionFactory(configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));
        services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();    
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        services.AddSingleton(Log.Logger);
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
            var settingService = new SettingService(Log.Logger, cacheManager, scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>());
            var config = new MelodeeConfiguration(await settingService.GetAllSettingsAsync().ConfigureAwait(false));

            var albumValidator = new AlbumValidator(config);
            
            Album? album = null;
            if (settings.LibraryName != null && settings.Id != null)
            {
                var libraryService = new LibraryService(Log.Logger,
                    cacheManager,
                    dbFactory,
                    settingService,
                    serializer);

                var libraryListResult = await libraryService.ListAsync(new PagedRequest()).ConfigureAwait(false);
                var library = libraryListResult.Data.FirstOrDefault(x => x.Name == settings.LibraryName);
                var albumDiscoveryService = new AlbumDiscoveryService(
                    Log.Logger,
                    cacheManager,
                    dbFactory,
                    settingService,
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
                    var pathToAlbum = Path.Combine(albumResult.Data.Directory, "melodee.json");
                    album = serializer.Deserialize<Album>(await File.ReadAllBytesAsync(pathToAlbum).ConfigureAwait(false));
                }
            }
            else
            {
                
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
