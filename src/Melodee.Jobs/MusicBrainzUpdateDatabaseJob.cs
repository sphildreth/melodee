using Melodee.Common.Configuration;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Services.Interfaces;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

public class MusicBrainzUpdateDatabaseJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    MusicBrainzRepository repository) : JobBase(logger, configurationFactory)
{
    public override Task Execute(IJobExecutionContext context)
    {
        //TODO Musicbrainz Db for metadata update job

        throw new NotImplementedException();
    }
}
