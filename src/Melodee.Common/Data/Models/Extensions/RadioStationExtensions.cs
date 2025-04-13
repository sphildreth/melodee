using Melodee.Common.Data.Constants;

namespace Melodee.Common.Data.Models.Extensions;

public static class RadioStationExtensions
{
    public static string ToApiKey(this RadioStation radioStation)
    {
        return $"radio{OpenSubsonicServer.ApiIdSeparator}{radioStation.ApiKey}";
    }    
    
    public static Common.Models.OpenSubsonic.InternetRadioStation ToApiInternetRadioStation(this RadioStation radioStation)
    {
        return new Common.Models.OpenSubsonic.InternetRadioStation
        (
           radioStation.ToApiKey(),
           radioStation.Name,
           radioStation.StreamUrl,
           radioStation.HomePageUrl
        );
    }    
}
