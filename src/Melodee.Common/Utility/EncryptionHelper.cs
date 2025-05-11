using System.Security.Cryptography;
using System.Text;

namespace Melodee.Common.Utility;

public static class EncryptionHelper
{
    public static string Decrypt(string privateKey, string cipherText, string publicKey)
    {
        if (cipherText is not { Length: > 0 })
        {
            throw new ArgumentNullException(nameof(cipherText));
        }

        if (privateKey is not { Length: > 0 })
        {
            throw new ArgumentNullException(nameof(privateKey));
        }

        if (publicKey is not { Length: > 0 })
        {
            throw new ArgumentNullException(nameof(publicKey));
        }

        using var aesAlg = Aes.Create();
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Key = CreateAesKey(privateKey);
        aesAlg.IV = Convert.FromBase64String(publicKey);

        using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
        {
            using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            var plaintext = srDecrypt.ReadToEnd();
            return plaintext;
        }
    }

    public static string Encrypt(string privateKey, string plainText, string publicKey)
    {
        if (plainText is not { Length: > 0 })
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        if (privateKey is not { Length: > 0 })
        {
            throw new ArgumentNullException(nameof(privateKey));
        }

        if (publicKey is not { Length: > 0 })
        {
            throw new ArgumentNullException(nameof(publicKey));
        }

        byte[] encrypted;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Key = CreateAesKey(privateKey);
            aesAlg.IV = Convert.FromBase64String(publicKey);
            using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            {
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
        }

        return Convert.ToBase64String(encrypted);
    }

    public static string GenerateRandomPublicKeyBase64()
    {
        return Convert.ToBase64String(GenerateRandomPublicKey());
    }

    public static byte[] GenerateRandomPublicKey()
    {
        var iv = new byte[16]; // AES > IV > 128 bit
        iv = RandomNumberGenerator.GetBytes(iv.Length);
        return iv;
    }

    private static byte[] CreateAesKey(string inputString)
    {
        return Encoding.UTF8.GetByteCount(inputString) == 32
            ? Encoding.UTF8.GetBytes(inputString)
            : SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }
}
