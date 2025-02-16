using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     Generate a report like summary of albums found for given library.
/// </summary>
public class LibraryAlbumStatusReportCommand : CommandBase<LibraryAlbumStatusReportSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryAlbumStatusReportSettings settings)
    {
        // var services = new ServiceCollection();
        // services.AddDbContextFactory<MelodeeDbContext>(opt =>
        //     opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o => o.UseNodaTime()));
        // services.AddHttpClient();
        // services.AddSingleton<IDbConnectionFactory>(opt =>
        //     new OrmLiteConnectionFactory(configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));
        // services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        //     
        // services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        // services.AddSingleton(Log.Logger);
        // services.AddRebus(configure =>
        // {
        //     return configure
        //         .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "melodee_bus"));
        // });
        //
        // services.AddScoped<AlbumDiscoveryService>();
        // services.AddScoped<AlbumImageSearchEngineService>();
        // services.AddScoped<ArtistSearchEngineService>();
        // services.AddScoped<DirectoryProcessorService>();
        // services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        // services.AddScoped<LibraryService>();
        // services.AddScoped<MediaEditService>();
        // services.AddScoped<MelodeeMetadataMaker>();
        // services.AddScoped<SettingService>();           

        using (var scope = CreateServiceProvider().CreateScope())
        {
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
            var result = await libraryService.AlbumStatusReport(settings.LibraryName);

            if (!settings.ReturnRaw)
            {
                var table = new Table();
                table.AddColumn("Summary");
                table.AddColumn("Data");
                table.AddColumn("Information");
                foreach (var stat in result.Data ?? [])
                {
                    table.AddRow(stat.Title.EscapeMarkup(), $"[{stat.DisplayColor}]{stat.Data?.ToString().EscapeMarkup() ?? string.Empty}[/]", stat.Message.EscapeMarkup());
                }

                AnsiConsole.Write(table);
                if (result.Messages?.Any() ?? false)
                {
                    AnsiConsole.WriteLine();
                    foreach (var message in result.Messages)
                    {
                        AnsiConsole.WriteLine(message.EscapeMarkup());
                    }

                    AnsiConsole.WriteLine();
                }
            }
            else
            {
                foreach (var stat in result.Data ?? [])
                {
                    Trace.WriteLine($"{stat.Title}\t{stat.Data}\t{stat.Message}");
                }
            }

            return 1;
        }
    }
}
