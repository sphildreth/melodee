using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class InternetRadioController(ISerializer serializer, EtagRepository etagRepository, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    //TODO
    // getInternetRadioStations
    // createInternetRadioStation
    // updateInternetRadioStation
    // deleteInternetRadioStation
}
