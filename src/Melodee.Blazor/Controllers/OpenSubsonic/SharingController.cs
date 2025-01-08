using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class SharingController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService,IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    //TODO
    //getShares
    //createShare
    //updateShare
    //deleteShare
}
