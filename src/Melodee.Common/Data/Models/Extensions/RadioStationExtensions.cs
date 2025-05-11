using Melodee.Common.Data.Constants;
using Melodee.Common.Models.OpenSubsonic;

namespace Melodee.Common.Data.Models.Extensions;

public static class RadioStationExtensions
{
    public static string ToApiKey(this RadioStation radioStation)
    {
        return $"radio{OpenSubsonicServer.ApiIdSeparator}{radioStation.ApiKey}";
    }

    public static InternetRadioStation ToApiInternetRadioStation(this RadioStation radioStation)
    {
        return new InternetRadioStation
        (
            radioStation.ToApiKey(),
            radioStation.Name,
            radioStation.StreamUrl,
            radioStation.HomePageUrl
        );
    }
}
