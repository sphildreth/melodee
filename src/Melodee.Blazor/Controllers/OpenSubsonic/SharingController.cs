using Melodee.Blazor.Filters;
using Melodee.Common.Serialization;
using Melodee.Common.Services;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class SharingController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
    //TODO
    //getShares
    //createShare
    //updateShare
    //deleteShare
}
