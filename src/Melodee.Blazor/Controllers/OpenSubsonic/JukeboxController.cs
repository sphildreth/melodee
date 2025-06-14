using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class JukeboxController(ISerializer serializer, EtagRepository etagRepository, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    //TODO
    // jukeboxControl
}
