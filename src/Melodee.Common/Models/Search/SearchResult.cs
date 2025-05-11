using Melodee.Common.Models.Collection;

namespace Melodee.Common.Models.Search;

public sealed record SearchResult(
    ArtistDataInfo[] Artists,
    AlbumDataInfo[] Albums,
    SongDataInfo[] Songs,
    ArtistDataInfo[] MusicBrainzArtists);
