using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Tests.Services;
using Melodee.Tests.Validation;

namespace Melodee.Tests.Common;

public class SerializerTests : ServiceTestBase
{
    [Fact]
    public void ValidateDeserializerWithUnicode()
    {
        var artist = new Artist
        (
            "J\\u00E4de",
            "JADE",
            "J\\u00E4de"
        );
        var serialized = Serializer.Serialize(artist);
        Assert.NotNull(serialized);
        var deserialized = Serializer.Deserialize<Artist>(serialized);
        Assert.NotNull(deserialized);
        Assert.Equal(artist.Name, deserialized.Name);
        Assert.Equal(artist.NameNormalized, deserialized.NameNormalized);
        Assert.Equal(artist.SortName, deserialized.SortName);
    }

    [Fact]
    public async Task ValidateDeserializerWriteToFileReadBackTextWithUnicode()
    {
        var artist = new Artist
        (
            "Jäde",
            "JADE",
            "Jäde"
        );
        var fileName = Path.GetTempFileName();
        await File.WriteAllTextAsync(fileName, Serializer.Serialize(artist));
        var fileInfo = new FileInfo(fileName);
        Assert.True(fileInfo.Exists);
        var deserialized = Serializer.Deserialize<Artist>(await File.ReadAllTextAsync(fileName));
        Assert.NotNull(deserialized);
        Assert.Equal(artist.Name, deserialized.Name);
        Assert.Equal(artist.NameNormalized, deserialized.NameNormalized);
        Assert.Equal(artist.SortName, deserialized.SortName);
    }

    [Fact]
    public async Task ValidateUnicodeDeserializerWriteToFileReadBackBytesWithUnicode()
    {
        var artist = new Artist
        (
            "Jäde",
            "JADE",
            "Jäde"
        );
        var fileName = Path.GetTempFileName();
        await File.WriteAllTextAsync(fileName, Serializer.Serialize(artist));
        var fileInfo = new FileInfo(fileName);
        Assert.True(fileInfo.Exists);
        var deserialized = Serializer.Deserialize<Artist>(await File.ReadAllBytesAsync(fileName));
        Assert.NotNull(deserialized);
        Assert.Equal(artist.Name, deserialized.Name);
        Assert.Equal(artist.NameNormalized, deserialized.NameNormalized);
        Assert.Equal(artist.SortName, deserialized.SortName);
    }

    [Fact]
    public async Task ValidateTextDeserializerWriteToFileReadBackBytes()
    {
        var artist = new Artist
        (
            "Spongebob SquarePants",
            "SPONGEBOBSQUAREPANTS",
            "SquarePants, Spongebob"
        );
        var fileName = Path.GetTempFileName();
        await File.WriteAllTextAsync(fileName, Serializer.Serialize(artist));
        var fileInfo = new FileInfo(fileName);
        Assert.True(fileInfo.Exists);
        var deserialized = Serializer.Deserialize<Artist>(await File.ReadAllBytesAsync(fileName));
        Assert.NotNull(deserialized);
        Assert.Equal(artist.Name, deserialized.Name);
        Assert.Equal(artist.NameNormalized, deserialized.NameNormalized);
        Assert.Equal(artist.SortName, deserialized.SortName);
    }

    [Fact]
    public void CanSerializerAndDeserializerAlbum()
    {
        var album = AlbumValidatorTests.TestAlbum;
        var serialized = Serializer.Serialize(album);
        Assert.NotNull(serialized);
        var deserialized = Serializer.Deserialize<Album>(serialized);
        Assert.NotNull(deserialized);
        Assert.Equal(album.Id, deserialized.Id);
        Assert.Equal(album.Status, deserialized.Status);
        Assert.Equal(album.AlbumType, deserialized.AlbumType);
        Assert.NotNull(deserialized.Songs);
        Assert.NotNull(album.Tags);
        Assert.NotNull(deserialized.Tags);
        var albumAlbumTag = album.Tags.First(x => x.Identifier == MetaTagIdentifier.Album);
        var deserializedAlbumTag = deserialized.Tags.First(x => x.Identifier == MetaTagIdentifier.Album);
        Assert.Equal(albumAlbumTag.ToString(), deserializedAlbumTag.ToString());
        Assert.Equal(AlbumValidatorTests.ShouldBeBitRate, deserialized.Songs?.FirstOrDefault()?.BitRate());

        album.SetTagValue(MetaTagIdentifier.Album, deserializedAlbumTag.Value?.ToString());
        Assert.Empty(album.ModifiedTags());

        album.SetTagValue(MetaTagIdentifier.Album, Guid.NewGuid().ToString());
        Assert.NotEmpty(album.ModifiedTags());
    }
}
