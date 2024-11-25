using Melodee.Services.Interfaces;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

public class MusicBrainzUpdateDatabaseJob(
    ILogger logger,
    ISettingService settingService) : JobBase(logger, settingService)
{
    public override Task Execute(IJobExecutionContext context)
    {
        //TODO Musicbrainz Db for metadata update job

        throw new NotImplementedException();
    }
}
