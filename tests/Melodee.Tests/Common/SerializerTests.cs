using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using Melodee.Common.Configuration;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Tests.Services;
using Melodee.Tests.Validation;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Serilog;
using Serilog.Core;
using Xunit.Abstractions;

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
        Assert.Equal(album.UniqueId(), deserialized.UniqueId());
        Assert.Equal(album.Status, deserialized.Status);
        Assert.NotNull(deserialized.Songs);

        Assert.Equal(AlbumValidatorTests.ShouldBeBitRate, deserialized.Songs?.FirstOrDefault()?.BitRate());
    }

  
}
