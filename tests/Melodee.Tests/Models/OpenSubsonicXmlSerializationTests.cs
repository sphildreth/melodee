using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Tests.Services;
using NodaTime;
using Xunit.Abstractions;

namespace Melodee.Tests.Models;

public class OpenSubsonicXmlSerializationTests(ITestOutputHelper testOutputHelper) : ServiceTestBase
{
    private XmlSchemaSet GetXmlSchemaSet()
    {
        string xsdPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "subsonic-rest-api-1.16.1.xsd");
        var schemas = new XmlSchemaSet();
        schemas.Add("http://subsonic.org/restapi", xsdPath);
        return schemas;
    }

    [Fact]
    public void ValidateAlbumList2Response()
    {
        var schemas = GetXmlSchemaSet();
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
                DataPropertyName = string.Empty,
                Data = new AlbumList2[]
                {
                    new AlbumList2
                    {
                        Id = "1",
                        Album = "a:1",
                        Title = "Da Album Title",
                        Name = "Da Album Name",
                        CoverArt = "cover_art:1",
                        SongCount = 5,
                        CreatedRaw = Instant.FromDateTimeUtc(DateTime.UtcNow),
                        Duration = 1327,
                        PlayedCount = 0,
                        ArtistId = "a:1",
                        Artist = "Da Artist",
                        Year = 1980,
                        UserStarredCount = 0,
                        Starred = false,
                        UserRating = 0,
                        Genres = new string[]
                        {
                            "Genre1",
                            "Genre2"
                        }
                    },
                    new AlbumList2
                    {
                        Id = "2",
                        Album = "a:2",
                        Title = "Da Album Title 2",
                        Name = "Da Album Name 2",
                        CoverArt = "cover_art:2",
                        SongCount = 6,
                        CreatedRaw = Instant.FromDateTimeUtc(DateTime.UtcNow),
                        Duration = 1328,
                        PlayedCount = 1,
                        ArtistId = "a:2",
                        Artist = "Da Artist 2",
                        Year = 1981,
                        UserStarredCount = 1,
                        Starred = true,
                        UserRating = 2,
                        Genres = new string[]
                        {
                            "Genre2",
                            "Genre3"
                        }
                    }                    
                }
            }
        };
        var xml = Serializer.SerializeOpenSubsonicModelToXml(response);
        var xmlDoc = XDocument.Load(new StringReader(xml));        
        Assert.NotNull(xml);
        xmlDoc = XDocument.Load(new StringReader(xml));
        var isValid = true;
        xmlDoc.Validate(schemas, (sender, e) =>
        {
            testOutputHelper.WriteLine("Validation error: {0}", e.Message);
            isValid = false;
        });
        Assert.True(isValid);
    }
    
    [Fact]
    public void ValidateSubsonicXmlPingResponse()
    {
        var schemas = GetXmlSchemaSet();
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
