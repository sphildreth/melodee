using Melodee.Blazor.Filters;
using Melodee.Common.Serialization;
using Melodee.Services;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class JukeboxController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
    //TODO
    // jukeboxControl
}
