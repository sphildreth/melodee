using Melodee.Common.Serialization;
using Melodee.Services;
using Melodee.Utils;

namespace Melodee.Controllers.OpenSubsonic;

public class SharingController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
    //TODO
    //getShares
    //createShare
    //updateShare
    //deleteShare
}
