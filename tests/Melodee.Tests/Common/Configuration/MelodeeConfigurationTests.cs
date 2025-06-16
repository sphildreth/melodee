using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Serialization;
using Moq;

namespace Melodee.Tests.Common.Configuration;

public class MelodeeConfigurationTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("The Outlaws", "Outlaws")]
    [InlineData("El Outlaws", "Outlaws")]
    [InlineData("LOS Outlaws", "Outlaws")]
    [InlineData("A Bad Boy", "Bad Boy")]
    [InlineData("Something with a Series Of LETTERs", "Something with a Series Of LETTERs")]
    [InlineData("Out in the West", "Out in the West")]
    [InlineData("Colin Hay", "Colin Hay")]
    public void ValidateArticlesAreRemoved(string? input, string? shouldBe)
    {
        var configuration = TestsBase.NewPluginsConfiguration();
        configuration.SetSetting(SettingRegistry.ProcessingIgnoredArticles, "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A");
        Assert.Equal(shouldBe, configuration.RemoveUnwantedArticles(input));

        configuration.SetSetting(SettingRegistry.ProcessingIgnoredArticles, string.Empty);
        Assert.Equal(input.Nullify(), configuration.RemoveUnwantedArticles(input));
    }

    [Fact]
    public void Constructor_InitializesConfiguration()
    {
        // Arrange
        var configDict = new Dictionary<string, object?> { { "test.key", "test.value" } };

        // Act
        var config = new MelodeeConfiguration(configDict);

        // Assert
        Assert.Equal(configDict, config.Configuration);
    }

    [Theory]
    [InlineData("http://example.com", "api123", ImageSize.Thumbnail, "http://example.com/images/api123/thumbnail")]
    [InlineData("http://example.com", "api123", ImageSize.Medium, "http://example.com/images/api123/medium")]
    [InlineData("http://example.com", "api123", ImageSize.Large, "http://example.com/images/api123/large")]
    public void GenerateImageUrl_ReturnsCorrectUrl(string baseUrl, string apiKey, ImageSize imageSize, string expectedUrl)
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>
        {
            { SettingRegistry.SystemBaseUrl, baseUrl }
        });

        // Act
        var url = config.GenerateImageUrl(apiKey, imageSize);

        // Assert
        Assert.Equal(expectedUrl, url);
    }

    [Fact]
    public void GenerateImageUrl_ThrowsException_WhenBaseUrlNotSet()
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>());

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => config.GenerateImageUrl("api123", ImageSize.Medium));
        Assert.Contains($"Configuration setting [{SettingRegistry.SystemBaseUrl}] is invalid", exception.Message);
    }

    [Fact]
    public void GenerateImageUrl_ThrowsException_WhenBaseUrlIsRequiredNotSet()
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>
        {
            { SettingRegistry.SystemBaseUrl, MelodeeConfiguration.RequiredNotSetValue }
        });

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => config.GenerateImageUrl("api123", ImageSize.Medium));
        Assert.Contains($"Configuration setting [{SettingRegistry.SystemBaseUrl}] is invalid", exception.Message);
    }

    [Theory]
    [InlineData(new[] { "term1" }, "https://www.google.com/search?q=term1")]
    [InlineData(new[] { "term1", "term2" }, "https://www.google.com/search?q=term1+term2")]
    [InlineData(new[] { "special chars: &?=" }, "https://www.google.com/search?q=special+chars%3a+%26%3f%3d")]
    public void GenerateWebSearchUrl_ReturnsCorrectUrl(object[] searchTerms, string expectedUrl)
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>());

        // Act
        var url = config.GenerateWebSearchUrl(searchTerms);

        // Assert
        Assert.Equal(expectedUrl, url);
    }

    [Theory]
    [InlineData(null, 250)]
    [InlineData(0, 250)]
    [InlineData(-1, 250)]
    [InlineData(1, 1)]
    [InlineData(500, 500)]
    [InlineData(1500, 1000)]
    public void BatchProcessingSize_ReturnsExpectedValue(int? configuredValue, int expectedResult)
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>
        {
            { SettingRegistry.DefaultsBatchSize, configuredValue }
        });

        // Act
        var result = config.BatchProcessingSize();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("1.0.0", null, null, "1.0.0")]
    [InlineData(null, "2.0.0", null, "2.0.0")]
    [InlineData(null, null, "3.0.0", "3.0.0")]
    [InlineData("1.0.0", "2.0.0", "3.0.0", "1.0.0")]
    public void ApiVersion_ReturnsCorrectVersion(string? apiVersion, string? subsonicVersion, string? fallbackVersion, string expected)
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>
        {
            { SettingRegistry.SystemApiVersion, apiVersion },
            { SettingRegistry.OpenSubsonicServerVersion, subsonicVersion }
        });

        // If needed for the test, manually update the default fallback value
        if (fallbackVersion != null && apiVersion == null && subsonicVersion == null)
        {
            config.SetSetting(SettingRegistry.SystemApiVersion, fallbackVersion);
        }

        // Act
        var result = config.ApiVersion();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValue_ReturnsCorrectValueWithCorrectType()
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>
        {
            { "string.key", "string value" },
            { "int.key", 42 },
            { "bool.key", true },
            { "null.key", null }
        });

        // Act & Assert
        Assert.Equal("string value", config.GetValue<string>("string.key"));
        Assert.Equal(42, config.GetValue<int>("int.key"));
        Assert.True(config.GetValue<bool>("bool.key"));
        Assert.Null(config.GetValue<string>("null.key"));
        Assert.Null(config.GetValue<string>("non.existent.key"));
    }

    [Fact]
    public void GetValue_UsesTransformFunction_WhenProvided()
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>
        {
            { "transform.key", "lowercase" }
        });

        // Act
        var result = config.GetValue<string>("transform.key", v => v?.ToUpper());

        // Assert
        Assert.Equal("LOWERCASE", result);
    }

    [Fact]
    public void SetSetting_UpdatesExistingValue()
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>
        {
            { "existing.key", "old value" }
        });

        // Act
        config.SetSetting("existing.key", "new value");

        // Assert
        Assert.Equal("new value", config.GetValue<string>("existing.key"));
    }

    [Fact]
    public void SetSetting_AddsNewValue_WhenKeyDoesNotExist()
    {
        // Arrange
        var config = new MelodeeConfiguration(new Dictionary<string, object?>());

        // Act
        config.SetSetting("new.key", "new value");

        // Assert
        Assert.Equal("new value", config.GetValue<string>("new.key"));
    }

    [Fact]
    public void IsTrue_ReturnsTrueForBooleanTrueValues()
    {
        // Arrange
        var settings = new Dictionary<string, object?>
        {
            { "true.bool", true },
            { "true.string", "true" },
            { "true.one", 1 },
            { "false.bool", false },
            { "false.string", "false" },
            { "false.zero", 0 }
        };

        // Act & Assert
        Assert.True(MelodeeConfiguration.IsTrue(settings, "true.bool"));
        Assert.True(MelodeeConfiguration.IsTrue(settings, "true.string"));
        Assert.True(MelodeeConfiguration.IsTrue(settings, "true.one"));
        Assert.False(MelodeeConfiguration.IsTrue(settings, "false.bool"));
        Assert.False(MelodeeConfiguration.IsTrue(settings, "false.string"));
        Assert.False(MelodeeConfiguration.IsTrue(settings, "false.zero"));
        Assert.False(MelodeeConfiguration.IsTrue(settings, "non.existent"));
    }

    [Fact]
    public void AllSettings_ReturnsAllRegisteredSettings()
    {
        // Act
        var result = MelodeeConfiguration.AllSettings();

        // Assert
        // Verify some known settings exist in the dictionary
        Assert.Contains(SettingRegistry.SystemBaseUrl, result.Keys);
        Assert.Contains(SettingRegistry.ProcessingIgnoredArticles, result.Keys);
        Assert.Contains(SettingRegistry.DefaultsBatchSize, result.Keys);
    }

    [Fact]
    public void AllSettings_WithValues_ReturnsPopulatedSettings()
    {
        // Arrange
        var initialValues = new Dictionary<string, object?>
        {
            { SettingRegistry.SystemBaseUrl, "http://example.com" },
            { SettingRegistry.ProcessingIgnoredArticles, "THE|A" }
        };

        // Act
        var result = MelodeeConfiguration.AllSettings(initialValues);

        // Assert
        Assert.Equal("http://example.com", result[SettingRegistry.SystemBaseUrl]);
        Assert.Equal("THE|A", result[SettingRegistry.ProcessingIgnoredArticles]);
    }

    [Fact]
    public void FromSerializedJsonArray_DeserializesCorrectly()
    {
        // Arrange
        var jsonArray = "[\"item1\",\"item2\",\"item3\"]";
        var mockSerializer = new Mock<ISerializer>();
        mockSerializer.Setup(s => s.Deserialize<string[]>(It.IsAny<string>()))
            .Returns(new[] { "item1", "item2", "item3" });

        // Act
        var result = MelodeeConfiguration.FromSerializedJsonArray(jsonArray, mockSerializer.Object);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("item1", result[0]);
        Assert.Equal("item2", result[1]);
        Assert.Equal("item3", result[2]);
    }

    [Fact]
    public void FromSerializedJsonArray_ReturnsEmptyArray_WhenInputIsNull()
    {
        // Arrange
        var mockSerializer = new Mock<ISerializer>();

        // Act
        var result = MelodeeConfiguration.FromSerializedJsonArray(null, mockSerializer.Object);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FromSerializedJsonDictionary_DeserializesCorrectly()
    {
        // Arrange
        var jsonDict = "{\"key1\":[\"value1\",\"value2\"],\"key2\":[\"value3\"]}";
        var mockSerializer = new Mock<ISerializer>();
        mockSerializer.Setup(s => s.Deserialize<Dictionary<string, string[]>>(It.IsAny<string>()))
            .Returns(new Dictionary<string, string[]>
            {
                { "key1", new[] { "value1", "value2" } },
                { "key2", new[] { "value3" } }
            });

        // Act
        var result = MelodeeConfiguration.FromSerializedJsonDictionary(jsonDict, mockSerializer.Object);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result["key1"].Length);
        Assert.Equal(1, result["key2"].Length);
        Assert.Equal("value1", result["key1"][0]);
        Assert.Equal("value3", result["key2"][0]);
    }

    [Fact]
    public void FromSerializedJsonDictionary_ReturnsEmptyDictionary_WhenInputIsNull()
    {
        // Arrange
        var mockSerializer = new Mock<ISerializer>();

        // Act
        var result = MelodeeConfiguration.FromSerializedJsonDictionary(null, mockSerializer.Object);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SetSetting_Static_UpdatesExistingKeyInDictionary()
    {
        // Arrange
        var settings = new Dictionary<string, object?>
        {
            { "existing.key", "old value" }
        };

        // Act
        MelodeeConfiguration.SetSetting(settings, "existing.key", "new value");

        // Assert
        Assert.Equal("new value", settings["existing.key"]);
    }

    [Fact]
    public void SetSetting_Static_AddsNewKeyToDictionary()
    {
        // Arrange
        var settings = new Dictionary<string, object?>();

        // Act
        MelodeeConfiguration.SetSetting(settings, "new.key", "new value");

        // Assert
        Assert.Equal("new value", settings["new.key"]);
        Assert.Single(settings);
    }
}
