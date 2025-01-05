using Melodee.Common.Data.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Cards;
using Melodee.Common.Models.Extensions;

namespace Melodee.Common.Models.Collection.Extensions;

public static class ArtistDataInfoExtensions
{
    public static async Task<ArtistCard> ToMelodeeArtistCardModelAsync(this ArtistDataInfo artist, FileSystemDirectoryInfo artistDirectory, byte[] defaultImageBytes, object? state = null)
    {
        byte[] artistPrimaryImageBytes = defaultImageBytes;
        var primaryImage = artistDirectory.AllFileImageTypeFileInfos().FirstOrDefault(x => x.Name == Data.Models.Artist.PrimaryImageFileName);
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
    
    public static FileSystemDirectoryInfo ToFileSystemDirectoryInfo(this ArtistDataInfo artist, string? libraryPath = null)
    {
        return new FileSystemDirectoryInfo
        {
            Path = libraryPath ?? artist.LibraryPath,
            Name = artist.Directory
        };
    }
    
    public static string ToApiKey(this ArtistDataInfo albumDataInfo)
        => $"artist{OpenSubsonicServer.ApiIdSeparator}{albumDataInfo.ApiKey}";
}
