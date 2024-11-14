using Quartz;

namespace Melodee.Common.Constants;

public static class JobKeyRegistry
{
    public static readonly JobKey LibraryInboundProcessJobKey = new JobKey("LibraryInboundProcessJob");
}
