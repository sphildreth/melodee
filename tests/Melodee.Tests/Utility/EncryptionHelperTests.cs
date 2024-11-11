using Melodee.Common.Constants;
using Melodee.Common.Utility;

namespace Melodee.Tests.Utility;

public class EncryptionHelperTests
{
    [Fact]
    public void EncryptAndDecryptString()
    {
        var configuration = TestsBase.NewConfiguration();
        var publicKey = Convert.ToBase64String(EncryptionHelper.GenerateRandomPublicKey());
        var privateKey = configuration[SettingRegistry.EncryptionPrivateKey]!.ToString()!;
        
        var shouldBe = "Hello World!";
        var encrypted = EncryptionHelper.Encrypt(privateKey, shouldBe, publicKey);
        var decrypted = EncryptionHelper.Decrypt(privateKey, encrypted, publicKey);

        Assert.Equal(shouldBe, decrypted);
    }
}
