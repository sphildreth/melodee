using System.Diagnostics;
using Serilog;

namespace Melodee.Common.Utility;

public static class Crc32
{
    private const int BufferSize = 8192;
    private static readonly uint[] Crc32Table = new uint[256];

    static Crc32()
    {
        unchecked
        {
            // This is the official polynomial used by CRC32 in PKZip.
            // Often the polynomial is shown reversed as 0x04C11DB7.
            const uint dwPolynomial = 0xEDB88320;

            for (uint i = 0; i < 256; i++)
            {
                var dwCrc = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((dwCrc & 1) == 1)
                    {
                        dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                    }
                    else
                    {
                        dwCrc >>= 1;
                    }
                }

                Crc32Table[i] = dwCrc;
            }
        }
    }

    /// <summary>
    ///     Returns the CRC32 Checksum of a specified file as a string.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>CRC32 Checksum as a string.</returns>
    public static string Calculate(FileInfo file)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }
        if (!file.Exists)
        {
            Trace.WriteLine($"Unable to calculate CRC32. File [{file.FullName}] not found. Returning default value.");
            return string.Empty;
        }
        return $"{CalculateInt32(file):X8}";
    }

    /// <summary>
    ///     Returns the CRC32 Checksum of a byte array as a string.
    /// </summary>
    /// <param name="data">The byte array.</param>
    /// <returns>CRC32 Checksum as a string.</returns>
    public static string Calculate(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        return $"{CalculateInt32(data):X8}";
    }

    /// <summary>
    ///     Returns the CRC32 Checksum of a specified file as a four byte signed integer (Int32).
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>CRC32 Checksum as a four byte signed integer (Int32).</returns>
    public static uint CalculateInt32(FileInfo file)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        using (var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return CalculateInt32(fileStream);
        }
    }

    /// <summary>
    ///     Returns the CRC32 Checksum of an input stream as a four byte signed integer (Int32).
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>CRC32 Checksum as a four byte signed integer (Int32).</returns>
    public static uint CalculateInt32(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        if (stream.Length == 0 || !stream.CanRead)
        {
            return 0;
        }
        try
        {
            unchecked
            {
                stream.Position = 0;
                var crc32Result = 0xFFFFFFFF;
                var buffer = new byte[BufferSize];

                var count = stream.Read(buffer, 0, BufferSize);
                while (count > 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        crc32Result = (crc32Result >> 8) ^ Crc32Table[buffer[i] ^ (crc32Result & 0x000000FF)];
                    }
                    count = stream.Read(buffer, 0, BufferSize);
                }

                return ~crc32Result;
            }
        }
        catch (IOException ex)
        {
            Log.Error(ex, "Error while calculating CRC32.");
        }
        return 0;
    }

    /// <summary>
    ///     Returns the CRC32 Checksum of a byte array as a four byte signed integer (Int32).
    /// </summary>
    /// <param name="data">The byte array.</param>
    /// <returns>CRC32 Checksum as a four byte signed integer (Int32).</returns>
    public static uint CalculateInt32(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        using (var memoryStream = new MemoryStream(data))
        {
            return CalculateInt32(memoryStream);
        }
    }
}
