using Melodee.Common.Security;

namespace Melodee.Tests.Security;

public class HmacTokenServiceTests
{
    private const string TestSecretKey = "ThisIsATestSecretKeyForUnitTesting";

    [Fact]
    public void Constructor_WithNullOrEmptyKey_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HmacTokenService(string.Empty));
    }

    [Fact]
    public void GenerateToken_WithValidData_ReturnsToken()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";

        // Act
        var token = service.GenerateToken(testData);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_WithNullOrEmptyData_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.GenerateToken(string.Empty));
    }

    [Fact]
    public void GenerateToken_WithSameInputs_ReturnsSameToken()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";

        // Act
        var token1 = service.GenerateToken(testData);
        var token2 = service.GenerateToken(testData);

        // Assert
        Assert.Equal(token1, token2);
    }

    [Fact]
    public void GenerateToken_WithDifferentSecretKeys_ReturnsDifferentTokens()
    {
        // Arrange
        var service1 = new HmacTokenService(TestSecretKey);
        var service2 = new HmacTokenService("DifferentSecretKey");
        var testData = "TestData";

        // Act
        var token1 = service1.GenerateToken(testData);
        var token2 = service2.GenerateToken(testData);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";
        var token = service.GenerateToken(testData);

        // Act
        var isValid = service.ValidateToken(testData, token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";
        var token = "InvalidToken"; // Not a valid Base64 token

        // Act
        var isValid = service.ValidateToken(testData, token);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_WithTamperedData_ReturnsFalse()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";
        var token = service.GenerateToken(testData);

        // Act
        var isValid = service.ValidateToken("TamperedData", token);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_WithNullOrEmptyParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";
        var token = service.GenerateToken(testData);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.ValidateToken(string.Empty, token));
        Assert.Throws<ArgumentNullException>(() => service.ValidateToken(testData, string.Empty));
    }

    [Fact]
    public void GenerateTimedToken_ReturnsFormattedString()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";

        // Act
        var timedToken = service.GenerateTimedToken(testData);

        // Assert
        Assert.NotNull(timedToken);
        Assert.NotEmpty(timedToken);

        var parts = timedToken.Split('|');
        Assert.Equal(3, parts.Length);
        Assert.Equal(testData, parts[0]);
        Assert.True(long.TryParse(parts[1], out _), "Timestamp should be parseable as long");
    }

    [Fact]
    public void ValidateTimedToken_WithValidNonExpiredToken_ReturnsTrue()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";
        var timedToken = service.GenerateTimedToken(testData, 5); // 5 minutes expiration

        // Act
        var isValid = service.ValidateTimedToken(timedToken);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateTimedToken_WithExpiredToken_ReturnsFalse()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";
        // Create a token that expires immediately (0 minutes)
        var timedToken = service.GenerateTimedToken(testData, 0);

        // Wait a bit to ensure it expires
        Thread.Sleep(1000);

        // Act
        var isValid = service.ValidateTimedToken(timedToken);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateTimedToken_WithInvalidFormat_ReturnsFalse()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);

        // Act & Assert
        Assert.False(service.ValidateTimedToken("InvalidFormat"));
        Assert.False(service.ValidateTimedToken("Part1|Part2"));
        Assert.False(service.ValidateTimedToken("Part1|Part2|Part3|Part4"));
    }

    [Fact]
    public void ValidateTimedToken_WithInvalidTimestamp_ReturnsFalse()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";
        var token = service.GenerateToken(testData + "|NotATimestamp");

        // Act
        var isValid = service.ValidateTimedToken($"{testData}|NotATimestamp|{token}");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateTimedToken_WithTamperedData_ReturnsFalse()
    {
        // Arrange
        var service = new HmacTokenService(TestSecretKey);
        var testData = "TestData";
        var timedToken = service.GenerateTimedToken(testData);
        var parts = timedToken.Split('|');

        // Tamper with the data but keep the original timestamp and token
        var tamperedToken = $"TamperedData|{parts[1]}|{parts[2]}";

        // Act
        var isValid = service.ValidateTimedToken(tamperedToken);

        // Assert
        Assert.False(isValid);
    }
}
