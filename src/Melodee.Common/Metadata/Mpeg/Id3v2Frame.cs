using System.Text;

namespace Melodee.Common.Metadata.Mpeg;

/// <summary>
///     Piece of ID3v2 reader.  reads frmes one at a time given the proper position in a binary stream
/// </summary>
public class Id3V2Frame
{
    public bool FCompression;
    public bool FDataLengthIndicator;
    public bool FEncryption;
    public bool FFileAlterPreservation;
    public bool FGroupingIdentity;
    public byte[] FrameContents;
    public string FrameName;
    public ulong FrameSize;
    public bool FReadOnly;


    public bool FTagAlterPreservation;
    public bool FUnsynchronisation;
    public int MajorVersion;
    public bool Padding;


    public Id3V2Frame()
    {
        Padding = false;
        FrameName = "";
        FrameSize = 0;
        MajorVersion = 0;

        FTagAlterPreservation = false;
        FFileAlterPreservation = false;
        FReadOnly = false;
        FCompression = false;
        FEncryption = false;
        FGroupingIdentity = false;
        FUnsynchronisation = false;
        FDataLengthIndicator = false;
    }

    public Id3V2Frame ReadFrame(BinaryReader br, int version)
    {
        char[] tagSize; // I use this to read the bytes in from the file
        int[] bytes; // for bit shifting
        ulong newSize = 0; // for the final number

        var nameSize = 4;
        if (version == 2)
        {
            nameSize = 3;
        }
        else if (version == 3 || version == 4)
        {
            nameSize = 4;
        }

        var f1 = new Id3V2Frame();
        f1.FrameName = new string(br.ReadChars(nameSize));
        f1.MajorVersion = version;


        // in order to check for padding I have to build a string of 4 (or 3 if v2.2) null bytes
        // there must be a better way to do this
        var nullChar = Convert.ToChar(0);
        var sb = new StringBuilder(0, nameSize);
        sb.Append(nullChar);
        sb.Append(nullChar);
        sb.Append(nullChar);
        if (nameSize == 4)
        {
            sb.Append(nullChar);
        }

        if (f1.FrameName == sb.ToString())
        {
            f1.Padding = true;
            return f1;
        }


        if (version == 2)
        {
            // only have 3 bytes for size ;


            tagSize = br.ReadChars(3); // I use this to read the bytes in from the file
            bytes = new int[3]; // for bit shifting
            newSize = 0; // for the final number
            // The ID3v2 tag size is encoded with four bytes
            // where the most significant bit (bit 7)
            // is set to zero in every byte,
            // making a total of 28 bits.
            // The zeroed bits are ignored
            //
            // Some bit grinding is necessary.  Hang on.


            bytes[3] = tagSize[2] | ((tagSize[1] & 1) << 7);
            bytes[2] = ((tagSize[1] >> 1) & 63) | ((tagSize[0] & 3) << 6);
            bytes[1] = (tagSize[0] >> 2) & 31;

            newSize = (ulong)bytes[3] |
                      ((ulong)bytes[2] << 8) |
                      ((ulong)bytes[1] << 16);
            //End Dan Code
        }


        else if (version == 3 || version == 4)
        {
            // version  2.4
            tagSize = br.ReadChars(4); // I use this to read the bytes in from the file
            bytes = new int[4]; // for bit shifting
            newSize = 0; // for the final number

            // The ID3v2 tag size is encoded with four bytes
            // where the most significant bit (bit 7)
            // is set to zero in every byte,
            // making a total of 28 bits.
            // The zeroed bits are ignored
            //
            // Some bit grinding is necessary.  Hang on.


            bytes[3] = tagSize[3] | ((tagSize[2] & 1) << 7);
            bytes[2] = ((tagSize[2] >> 1) & 63) | ((tagSize[1] & 3) << 6);
            bytes[1] = ((tagSize[1] >> 2) & 31) | ((tagSize[0] & 7) << 5);
            bytes[0] = (tagSize[0] >> 3) & 15;

            newSize = (ulong)bytes[3] |
                      ((ulong)bytes[2] << 8) |
                      ((ulong)bytes[1] << 16) |
                      ((ulong)bytes[0] << 24);
            //End Dan Code
        }

        f1.FrameSize = newSize;


        if (version > 2)
        {
            // versions 3+ have frame tags.
            if (version == 3)
            {
                bool[] c;
                // read teh tags
                c = BitReader.ToBitBool(br.ReadByte());
                f1.FTagAlterPreservation = c[0];
                f1.FFileAlterPreservation = c[1];
                f1.FReadOnly = c[2];

                c = BitReader.ToBitBool(br.ReadByte());
                f1.FCompression = c[0];
                f1.FEncryption = c[1];
                f1.FGroupingIdentity = c[2];
            }
            else if (version == 4)
            {
                //%0abc0000 %0h00kmnp
                //a - Tag alter preservation
                //     0     Frame should be preserved.
                //     1     Frame should be discarded.
                // b - File alter preservation
                //  0     Frame should be preserved.
                //1     Frame should be discarded.
                //  c - Read only
                //  h - Grouping identity
                //    0     Frame does not contain group information
                //    1     Frame contains group information
                //   k - Compression
                //     0     Frame is not compressed.
                //     1     Frame is compressed using zlib [zlib] deflate method.
                //           If set, this requires the 'Data Length Indicator' bit
                //           to be set as well.
                //  m - Encryption
                //     0     Frame is not encrypted.
                //     1     Frame is encrypted.
                //  n - Unsynchronisation
                //      0     Frame has not been unsynchronised.
                //      1     Frame has been unsyrchronised.
                //   p - Data length indicator
                //     0      There is no Data Length Indicator.
                //    1      A data length Indicator has been added to the frame.


                bool[] c;
                // read teh tags
                c = BitReader.ToBitBool(br.ReadByte());
                f1.FTagAlterPreservation = c[1];
                f1.FFileAlterPreservation = c[2];
                f1.FReadOnly = c[3];

                c = BitReader.ToBitBool(br.ReadByte());
                f1.FGroupingIdentity = c[1];
                f1.FCompression = c[4];
                f1.FEncryption = c[5];
                f1.FUnsynchronisation = c[6];
                f1.FDataLengthIndicator = c[7];
            }

            if (f1.FrameSize > 0)
            {
                f1.FrameContents = br.ReadBytes((int)f1.FrameSize);
            }
        }

        return f1;
    }
}
