using Hqub.Lastfm;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Utility;
using NodaTime;
using Scrobble = Hqub.Lastfm.Entities.Scrobble;

namespace Melodee.Plugins.Scrobbling;

public class LastFmScrobbler(IMelodeeConfiguration configuration) : IScrobbler
{
    public string Id => "7ADF8A80-433C-487A-8359-20FD473F20BB";

    public string DisplayName => nameof(LastFmScrobbler);

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;

    public async Task<OperationResult<bool>> NowPlaying(User user, ScrobbleInfo scrobble, CancellationToken token = default)
    {
        var result = true;
        var apiKey = SettingRegistry.ScrobblingLastFmApiKey;
        if (apiKey.Nullify() != null)
        {
            var client = new LastfmClient(apiKey);
            result = await client.Track.UpdateNowPlayingAsync(
                scrobble.SongTitle,
                scrobble.SongArtist,
                scrobble.SongNumber ?? 0,
                scrobble.AlbumTitle,
                scrobble.ArtistName).ConfigureAwait(false);
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> Scrobble(User user, ScrobbleInfo scrobble, CancellationToken token = default)
    {
        var result = true;
        var apiKey = SettingRegistry.ScrobblingLastFmApiKey;
        if (apiKey.Nullify() != null)
        {
            var client = new LastfmClient(configuration.GetValue<string>(apiKey));

            var scrobbleResult = await client.Track.ScrobbleAsync([
                new Scrobble
                {
                    Track = scrobble.SongTitle,
                    Artist = scrobble.SongArtist,
                    Date = DateTime.UtcNow,
                    Album = scrobble.AlbumTitle,
                    AlbumArtist = scrobble.ArtistName,
                    MBID = scrobble.SongMusicBrainzId,
                    Duration = SafeParser.ToNumber<int>(scrobble.SongDuration),
                    TrackNumber = scrobble.SongNumber ?? 0,
                    ChosenByUser = !scrobble.IsRandomizedScrobble
                }
            ]).ConfigureAwait(false);
            result = scrobbleResult.Accepted > 0;
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }
}