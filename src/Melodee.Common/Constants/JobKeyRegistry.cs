using Quartz;

namespace Melodee.Common.Constants;

public static class JobKeyRegistry
{
    public static readonly JobKey LibraryInboundProcessJobKey = new JobKey("LibraryInboundProcessJob");
    public static readonly JobKey LibraryProcessJobJobKey = new JobKey("LibraryProcessJob");
    public static readonly JobKey MusicBrainzUpdateDatabaseJobKey = new JobKey("MusicBrainzUpdateDatabaseJob");
    public static readonly JobKey ArtistHousekeepingJobJobKey = new JobKey("ArtistHousekeepingJob");
}
