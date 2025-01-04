using Melodee.Common.Data.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Cards;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Common.Data.Models.Extensions;

public static class ArtistExtensions
{
    public static Melodee.Common.Models.Artist ToMelodeeArtistModel(this Artist artist)
    {
        return new Common.Models.Artist(artist.Name,
            artist.NameNormalized,
            artist.SortName,
            null,
            artist.Id,
            artist.MusicBrainzId?.ToString());
    }

    public static async Task<ArtistCard> ToMelodeeArtistCardModelAsync(this Artist artist, FileSystemDirectoryInfo artistDirectory, byte[] defaultImageBytes, object? state = null)
    {
        byte[] artistPrimaryImageBytes = defaultImageBytes;
        var primaryImage = artistDirectory.AllFileImageTypeFileInfos().FirstOrDefault(x => x.Name == Artist.PrimaryImageFileName);
        if (primaryImage != null)
        {
            artistPrimaryImageBytes = await File.ReadAllBytesAsync(primaryImage.FullName);
        }
        return new ArtistCard
        {
            AlbumCount = artist.AlbumCount,
            ApiKeyId = artist.ToApiKey(),
            Created = artist.CreatedAt,
            Id = artist.ApiKey,
            ImageBytes = artistPrimaryImageBytes,
            IsValid = artist.ApiKey != Guid.Empty && artist.Name.Nullify() != null,
            Name = artist.Name,
            SongCount = artist.SongCount,
            State = state
        };
    }
    
    public static string ToCoverArtId(this Artist artist)
    {
        return artist.ToApiKey();
    }

    public static string ToApiKey(this Artist artist)
    {
        return $"artist{OpenSubsonicServer.ApiIdSeparator}{artist.ApiKey}";
    }

    public static ArtistID3 ToApiArtistID3(this Artist artist, UserArtist? userArtist = null)
    {
        return new ArtistID3(
            artist.ToApiKey(),
            artist.Name,
            artist.ToCoverArtId(),
            artist.AlbumCount,
            userArtist?.Rating ?? 0,
            "URL",
            userArtist?.StarredAt.ToString(),
            artist.MusicBrainzId?.ToString(),
            artist.SortName,
            artist.Roles?.ToTags()?.ToArray()
        );
    }

    public static Common.Models.OpenSubsonic.Artist ToApiArtist(this Artist artist, UserArtist? userArtist = null)
    {
        return new Common.Models.OpenSubsonic.Artist(
            artist.ToApiKey(),
            artist.Name,
            artist.AlbumCount,
            userArtist?.Rating ?? 0,
            artist.CalculatedRating,
            artist.ToCoverArtId(),
            "Url", // TODO ?
            userArtist?.StarredAt.ToString()
        );
    }

    public static ArtistQuery ToArtistQuery(this Artist artist, KeyValue[] albumKeyValues)
    {
        return new ArtistQuery
        {
            Name = artist.Name,
            AlbumKeyValues = albumKeyValues,
            MusicBrainzId = artist.MusicBrainzId?.ToString()
        };
    }

    public static FileSystemDirectoryInfo ToFileSystemDirectoryInfo(this Artist artist, string? libraryPath = null)
    {
        return new FileSystemDirectoryInfo
        {
            Path = Path.Combine(libraryPath ?? artist.Library.Path, artist.Directory),
            Name = artist.Directory
        };
    }
}
