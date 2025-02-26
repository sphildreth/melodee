using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Jobs;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Rebus.Bus;
using Serilog;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     This runs the job that scans all the library type libraries
/// </summary>
public class LibraryScanCommand : CommandBase<LibraryScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryScanSettings settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var job = new LibraryInsertJob
            (
                scope.ServiceProvider.GetRequiredService<ILogger>(),
                scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>(),
                scope.ServiceProvider.GetRequiredService<LibraryService>(),
                scope.ServiceProvider.GetRequiredService<ISerializer>(),
                scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>(),
                scope.ServiceProvider.GetRequiredService<ArtistService>(),
                scope.ServiceProvider.GetRequiredService<AlbumService>(),
                scope.ServiceProvider.GetRequiredService<AlbumDiscoveryService>(),
                scope.ServiceProvider.GetRequiredService<DirectoryProcessorToStagingService>(),
                scope.ServiceProvider.GetRequiredService<IBus>()
            );

            job.OnProcessingEvent += (_, e) => { Log.Information(e.ToString()); };

            var jobExecutionContext = new MelodeeJobExecutionContext(CancellationToken.None);
            jobExecutionContext.Put(MelodeeJobExecutionContext.ForceMode, settings.ForceMode);
            jobExecutionContext.Put(MelodeeJobExecutionContext.Verbose, settings.Verbose);
            await job.Execute(jobExecutionContext);
            return 1;
        }
    }
}
