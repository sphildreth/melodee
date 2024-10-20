using System.Security.Cryptography;
using System.Text;
using K4os.Hash.xxHash;

namespace Melodee.Common.Utility;

public static class HashHelper
{
    public static string? CreateMd5(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        return CreateMd5(Encoding.UTF8.GetBytes(input));
    }

    public static string? CreateMd5(FileInfo file)
    {
        return CreateMd5(File.ReadAllBytes(file.FullName));
    }

    private static string? CreateMd5(byte[]? bytes)
    {
        if (bytes == null || !bytes.Any())
        {
            return null;
        }

        using (var md5 = MD5.Create())
        {
            var data = md5.ComputeHash(bytes);

            // Create a new StringBuilder to collect the bytes and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

    public static uint GetHash(string file)
    {
        return XXH32.DigestOf(File.ReadAllBytes(file));
    }
}
