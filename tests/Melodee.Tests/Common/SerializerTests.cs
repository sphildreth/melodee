using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Tests.Services;
using Melodee.Tests.Validation;

namespace Melodee.Tests.Common;

public class SerializerTests : ServiceTestBase
{
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
