using Melodee.Common.Serialization;
using Melodee.Services;
using Melodee.Utils;

namespace Melodee.Controllers.OpenSubsonic;

public class JukeboxController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
    //TODO
    // jukeboxControl
}
