using Melodee.Common.Serialization;
using Melodee.Services;
using Melodee.Utils;

namespace Melodee.Controllers.OpenSubsonic;

public class InternetRadioController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
    //TODO
    // getInternetRadioStations
    // createInternetRadioStation
    // updateInternetRadioStation
    // deleteInternetRadioStation
}
