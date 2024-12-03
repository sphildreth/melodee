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

public class SerializerTests(ITestOutputHelper testOutputHelper) : ServiceTestBase
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

    [Fact]
    public async Task ValidateSubsonicXmlPingResponse()
    {
        string xsdPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "subsonic-rest-api-1.16.1.xsd");
        var schemas = new XmlSchemaSet();
        schemas.Add("http://subsonic.org/restapi", xsdPath);
        
        var goodXml = "<subsonic-response xmlns=\"http://subsonic.org/restapi\" status=\"ok\" version=\"1.1.1\"> </subsonic-response>";
        var badXml = "<subsonic-response xmlns=\"http://subsonic.org/restapi\" _sXXXatus=\"batman\" version=\"1.1.1\"> </subsonic-response>";

        // Should be good
        var xmlDoc = XDocument.Load(new StringReader(goodXml));
        bool isValid = true;
        xmlDoc.Validate(schemas, (sender, e) =>
        {
            testOutputHelper.WriteLine("Validation error: {0}", e.Message);
            isValid = false;
        });
        Assert.True(isValid);
        
        // Should be bad
        xmlDoc = XDocument.Load(new StringReader(badXml));
        isValid = true;
        xmlDoc.Validate(schemas, (sender, e) =>
        {
            testOutputHelper.WriteLine("Validation error: {0}", e.Message);
            isValid = false;
        });
        Assert.False(isValid);

        var response = new Melodee.Common.Models.OpenSubsonic.Responses.ResponseModel
        {
            IsSuccess = true,
            UserInfo = new Melodee.Common.Models.UserInfo(5, Guid.NewGuid(), "batman", "batman@melodee.net"),
            ResponseData = new Melodee.Common.Models.OpenSubsonic.Responses.ApiResponse
            {
                IsSuccess = true,
                Status = "ok",
                Version = "1.16.1",
                Type = "Melodee",
                ServerVersion = "v1.0.0-rc1",
                DataPropertyName = string.Empty
            }
        };
        var xml = Serializer.SerializeOpenSubsonicModelToXml(response);
        Assert.NotNull(xml);
        xmlDoc = XDocument.Load(new StringReader(xml));
        isValid = true;
        xmlDoc.Validate(schemas, (sender, e) =>
        {
            testOutputHelper.WriteLine("Validation error: {0}", e.Message);
            isValid = false;
        });
        // Should be good        
        Assert.True(isValid);

    }
}
