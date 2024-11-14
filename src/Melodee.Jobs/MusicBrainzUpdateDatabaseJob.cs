using Melodee.Services;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

public class MusicBrainzUpdateDatabaseJob(
    ILogger logger,
    SettingService settingService) : JobBase(logger, settingService)
{
    public override Task Execute(IJobExecutionContext context)
    {
        // TODO Musicbrainz Db for metadata update job
        
        throw new NotImplementedException();
    }
}
