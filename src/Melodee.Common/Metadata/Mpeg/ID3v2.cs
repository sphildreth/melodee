using System.Collections;
using System.Text;

namespace Melodee.Common.Metadata.Mpeg;

/// <summary>
///     Summary description for ID3v2.
/// </summary>
public class Id3V2
{
    private byte[] _ba;


    private BinaryReader _br;

    private bool _fileOpen;
    public string Album;
    public string Artist;
    public string Comment;

    public bool EbUpdate;
    public bool EcCrc;
    public bool EdRestrictions;

    public ulong ExtCCrc;
    public byte ExtDRestrictions;

    // ext header 
    public ulong ExtHeaderSize;
    public int ExtNumFlagBytes;

    public bool FaUnsynchronisation;
    public bool FbExtendedHeader;
    public bool FcExperimentalIndicator;
    public bool FdFooter;
    public string Filename;
    public ArrayList Frames;

    public Hashtable FramesHash;
    public string Genre;
    public bool HasTag;

    public ulong HeaderSize;

    // id3v2 header
    public int MajorVersion;
    public int MinorVersion;

    //public bool header;

    // song info
    public string Title;
    public string TotalTracks;
    public string Track;
    public string Year;


    public Id3V2()
    {
        Initialize_Components();
    }

    public Id3V2(string fileName)
    {
        Initialize_Components();
        Filename = fileName;
    }


    private void Initialize_Components()
    {
        Filename = "";


        MajorVersion = 0;
        MinorVersion = 0;

        FaUnsynchronisation = false;
        FbExtendedHeader = false;
        FcExperimentalIndicator = false;

        _fileOpen = false;

        Frames = new ArrayList();
        FramesHash = new Hashtable();

        Album = "";
        Artist = "";
        Comment = "";
        ExtCCrc = 0;
        EbUpdate = false;
        EcCrc = false;
        EdRestrictions = false;
        ExtDRestrictions = 0;
        ExtHeaderSize = 0;
        ExtNumFlagBytes = 0;
        _fileOpen = false;
        HasTag = false;
        HeaderSize = 0;
        Title = "";
        TotalTracks = "";
        Track = "";
        Year = "";
    }


    private void CloseFile()
    {
        _br.Close();
    }


    private void ReadHeader()
    {
        // bring in the first three bytes.  it must be ID3 or we have no tag
        // TODO add logic to check the end of the file for "3D1" and other
        // possible starting spots
        var id3Start = new string(_br.ReadChars(3));

        // check for a tag
        if (!id3Start.Equals("ID3"))
        {
            // TODO we are fucked.
            //throw id3v2ReaderException;
            HasTag = false;
        }
        else
        {
            HasTag = true;

            // read id3 version.  2 bytes:
            // The first byte of ID3v2 version is it's major version,
            // while the second byte is its revision number
            MajorVersion = Convert.ToInt32(_br.ReadByte());
            MinorVersion = Convert.ToInt32(_br.ReadByte());

            //read next byte for flags
            var boolar = BitReader.ToBitBool(_br.ReadByte());
            // set the flags
            FaUnsynchronisation = boolar[0];
            FbExtendedHeader = boolar[1];
            FcExperimentalIndicator = boolar[2];

            // read teh size
            // this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
            //Dan Code 
            var tagSize = _br.ReadChars(4); // I use this to read the bytes in from the file
            var bytes = new int[4]; // for bit shifting
            ulong newSize = 0; // for the final number
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

            newSize = (10 + (ulong)bytes[3]) |
                      ((ulong)bytes[2] << 8) |
                      ((ulong)bytes[1] << 16) |
                      ((ulong)bytes[0] << 24);
            //End Dan Code

            HeaderSize = newSize;
        }
    }

    private void ReadFrames()
    {
        var f = new Id3V2Frame();
        do
        {
            f = f.ReadFrame(_br, MajorVersion);

            // check if we have hit the padding.
            if (f.Padding)
            {
                //we hit padding.  lets advance to end and stop reading.
                _br.BaseStream.Position = Convert.ToInt64(HeaderSize);
                break;
            }

            Frames.Add(f);
            FramesHash.Add(f.FrameName, f);

            #region frameprocessing

            /*
            else
            {
                // figure out which type it is
                if (f.frameName.StartsWith("T"))
                {
                    if (f.frameName.Equals("TXXX"))
                    {
                        ProcessTXXX(f);
                    }
                    else
                    {
                        ProcessTTYPE(f);
                    }
                }
                else
                {
                    if (f.frameName.StartsWith("W"))
                    {
                        if (f.frameName.Equals("WXXX"))
                        {
                            ProcessWXXX(f);
                        }
                        else
                        {
                            ProcessWTYPE(f);
                        }
                    }
                    else
                    {
                        // if it isn't  a muliple reader case (above) then throw it into the switch to process
                        switch (f.frameName)
                        {

                            case "IPLS":
                                ProcessIPLS(f);
                                break;
                            case "MCDI":
                                ProcessMCDI(f);
                                break;
                            case "UFID":
                                ProcessUFID(f);
                                break;
                            case "COMM":
                                ProcessCOMM(f);
                                break;

                            default:
                                frames.Add(f.frameName, f.frameContents);
                                AddItemToList(f.frameName, "non text");
                                break;
                        }
            }

    }


        }*/

            #endregion
        } while (_br.BaseStream.Position <= Convert.ToInt64(HeaderSize));
    }

    public void Read()
    {
        var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read);
        _br = new BinaryReader(fs);

        ReadHeader();

        if (HasTag)
        {
            if (FbExtendedHeader)
            {
                ReadExtendedHeader();
            }


            ReadFrames();

            if (FdFooter)
            {
                ReadFooter();
            }

            ParseCommonHeaders();
        }

        fs.Close();
        _br.Close();


        #region tag reader

        /*if (!fileOpen)
                {
                    if (!this.filename == "")
                    {
                        OpenFile();
                    }
                    else
                    {
                        // we are fucked.  throw an exception or something
                    }
                }
                // bring in the first three bytes.  it must be ID3 or we have no tag
                // TODO add logic to check the end of the file for "3D1" and other
                // possible starting spots
                string id3start = new string (br.ReadChars(3));

                // check for a tag
                if  (!id3start.Equals("ID3"))
                {
                    // TODO we are fucked.  not really we just don't ahve a tag
                    // and we need to bail out gracefully.
                    throw id3v23ReaderException;
                }
                else
                {
                    // we have a tag
                    this.header = true;
                }

                // read id3 version.  2 bytes:
                // The first byte of ID3v2 version is it's major version,
                // while the second byte is its revision number
                this.MajorVersion = System.Convert.ToInt32(br.ReadByte());
                this.MinorVersion = System.Convert.ToInt32(br.ReadByte());

                // here is where we get fancy.  I am useing silisoft's php code as
                // a reference here.  we are going to try and parse for 2.2, 2.3 and 2.4
                // in one pass.  hold on!!

                if ((this.header) && (this.MajorVersion <= 4)) // probably won't work on higher versions
                {
                    // (%ab000000 in v2.2, %abc00000 in v2.3, %abcd0000 in v2.4.x)
                    //read next byte for flags
                    bool [] boolar = BitReader.ToBitBool(br.ReadByte());
                    // set the flags
                    if (this.MajorVersion == 2)
                    {
                        this.FA_Unsyncronisation = boolar[0];
                        this.FB_ExtendedHeader = boolar[1];
                    }
                    else if ( this.MajorVersion == 3 )
                    {
                        // set the flags
                        this.FA_Unsyncronisation = boolar[0];
                        this.FB_ExtendedHeader = boolar[1];
                        this.FC_ExperimentalIndicator = boolar[2];
                    }
                    else if (this.MajorVersion == 4)
                    {
                        // set the flags
                        this.FA_Unsyncronisation = boolar[0];
                        this.FB_ExtendedHeader = boolar[1];
                        this.FC_ExperimentalIndicator = boolar[2];
                        this.FD_Footer = boolar[3];
                    }

                    // read teh size
                    // this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
                    //Dan Code
                    char[] tagSize = br.ReadChars(4);    // I use this to read the bytes in from the file
                    int[] bytes = new int[4];      // for bit shifting
                    ulong newSize = 0;    // for the final number
                    // The ID3v2 tag size is encoded with four bytes
                    // where the most significant bit (bit 7)
                    // is set to zero in every byte,
                    // making a total of 28 bits.
                    // The zeroed bits are ignored
                    //
                    // Some bit grinding is necessary.  Hang on.



                    bytes[3] =  tagSize[3]             | ((tagSize[2] & 1) << 7) ;
                    bytes[2] = ((tagSize[2] >> 1) & 63) | ((tagSize[1] & 3) << 6) ;
                    bytes[1] = ((tagSize[1] >> 2) & 31) | ((tagSize[0] & 7) << 5) ;
                    bytes[0] = ((tagSize[0] >> 3) & 15) ;

                    newSize  = ((UInt64)10 +	(UInt64)bytes[3] |
                        ((UInt64)bytes[2] << 8)  |
                        ((UInt64)bytes[1] << 16) |
                        ((UInt64)bytes[0] << 24)) ;
                    //End Dan Code

                    this.id3v2HeaderSize = newSize;


                }
                */

        #endregion
    }

    private void ParseCommonHeaders()
    {
        if (MajorVersion == 2)
        {
            if (FramesHash.Contains("TT2"))
            {
                var bytes = ((Id3V2Frame)FramesHash["TT2"]).FrameContents;
                var sb = new StringBuilder();
                byte textEncoding;


                for (var i = 1; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    //this.Title = myString.Substring(1);
                    Title = sb.ToString();
                }
            }


            if (FramesHash.Contains("TP1"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TP1"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Artist = sb.ToString();
                }
            }

            if (FramesHash.Contains("TAL"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TAL"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Album = sb.ToString();
                }
            }

            if (FramesHash.Contains("TYE"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TYE"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Year = sb.ToString();
                }
            }

            if (FramesHash.Contains("TRK"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TRK"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Track = sb.ToString();
                }
            }

            if (FramesHash.Contains("TCO"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TCO"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Genre = sb.ToString();
                }
            }

            if (FramesHash.Contains("COM"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["COM"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Comment = sb.ToString();
                }
            }
        }
        else
        {
            // $id3info["majorversion"] > 2
            if (FramesHash.Contains("TIT2"))
            {
                var bytes = ((Id3V2Frame)FramesHash["TIT2"]).FrameContents;
                var sb = new StringBuilder();
                byte textEncoding;


                for (var i = 1; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    //this.Title = myString.Substring(1);
                    Title = sb.ToString();
                }
            }


            if (FramesHash.Contains("TPE1"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TPE1"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Artist = sb.ToString();
                }
            }

            if (FramesHash.Contains("TALB"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TALB"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Album = sb.ToString();
                }
            }

            if (FramesHash.Contains("TYER"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TYER"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Year = sb.ToString();
                }
            }

            if (FramesHash.Contains("TRCK"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TRCK"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Track = sb.ToString();
                }
            }

            if (FramesHash.Contains("TCON"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["TCON"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Genre = sb.ToString();
                }
            }

            if (FramesHash.Contains("COMM"))
            {
                var sb = new StringBuilder();
                var bytes = ((Id3V2Frame)FramesHash["COMM"]).FrameContents;
                byte textEncoding;

                for (var i = 0; i < bytes.Length; i++)
                {
                    if (i == 0)
                    {
                        //read the text encoding.
                        textEncoding = bytes[i];
                    }
                    else
                    {
                        sb.Append(Convert.ToChar(bytes[i]));
                    }

                    Comment = sb.ToString();
                }
            }
        }

        var trackHolder = Track.Split('/');
        Track = trackHolder[0];
        if (trackHolder.Length > 1)
        {
            TotalTracks = trackHolder[1];
        }
    }

    private void ReadFooter()
    {
        // bring in the first three bytes.  it must be ID3 or we have no tag
        // TODO add logic to check the end of the file for "3D1" and other
        // possible starting spots
        var id3Start = new string(_br.ReadChars(3));

        // check for a tag
        if (!id3Start.Equals("3DI"))
        {
            // TODO we are fucked.  not really we just don't ahve a tag
            // and we need to bail out gracefully.
            //throw id3v23ReaderException;
        }
        else
        {
            // we have a tag
            HasTag = true;
        }

        // read id3 version.  2 bytes:
        // The first byte of ID3v2 version is it's major version,
        // while the second byte is its revision number
        MajorVersion = Convert.ToInt32(_br.ReadByte());
        MinorVersion = Convert.ToInt32(_br.ReadByte());

        // here is where we get fancy.  I am useing silisoft's php code as 
        // a reference here.  we are going to try and parse for 2.2, 2.3 and 2.4
        // in one pass.  hold on!!

        if (HasTag && MajorVersion <= 4) // probably won't work on higher versions
        {
            // (%ab000000 in v2.2, %abc00000 in v2.3, %abcd0000 in v2.4.x)
            //read next byte for flags
            var boolar = BitReader.ToBitBool(_br.ReadByte());
            // set the flags
            if (MajorVersion == 2)
            {
                FaUnsynchronisation = boolar[0];
                FbExtendedHeader = boolar[1];
            }
            else if (MajorVersion == 3)
            {
                // set the flags
                FaUnsynchronisation = boolar[0];
                FbExtendedHeader = boolar[1];
                FcExperimentalIndicator = boolar[2];
            }
            else if (MajorVersion == 4)
            {
                // set the flags
                FaUnsynchronisation = boolar[0];
                FbExtendedHeader = boolar[1];
                FcExperimentalIndicator = boolar[2];
                FdFooter = boolar[3];
            }

            // read teh size
            // this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
            //Dan Code 
            var tagSize = _br.ReadChars(4); // I use this to read the bytes in from the file
            var bytes = new int[4]; // for bit shifting
            ulong newSize = 0; // for the final number
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

            newSize = (10 + (ulong)bytes[3]) |
                      ((ulong)bytes[2] << 8) |
                      ((ulong)bytes[1] << 16) |
                      ((ulong)bytes[0] << 24);
            //End Dan Code

            HeaderSize = newSize;
        }
    }

    private void ReadExtendedHeader()
    {
        //			Extended header size   4 * %0xxxxxxx
        //			Number of flag bytes       $01
        //			Extended Flags             $xx
        //			Where the 'Extended header size' is the size of the whole extended header, stored as a 32 bit synchsafe integer.
        // read teh size
        // this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
        //Dan Code 
        var extHeaderSize = _br.ReadChars(4); // I use this to read the bytes in from the file
        var bytes = new int[4]; // for bit shifting
        ulong newSize = 0; // for the final number
        // The ID3v2 tag size is encoded with four bytes
        // where the most significant bit (bit 7)
        // is set to zero in every byte,
        // making a total of 28 bits.
        // The zeroed bits are ignored
        //
        // Some bit grinding is necessary.  Hang on.


        bytes[3] = extHeaderSize[3] | ((extHeaderSize[2] & 1) << 7);
        bytes[2] = ((extHeaderSize[2] >> 1) & 63) | ((extHeaderSize[1] & 3) << 6);
        bytes[1] = ((extHeaderSize[1] >> 2) & 31) | ((extHeaderSize[0] & 7) << 5);
        bytes[0] = (extHeaderSize[0] >> 3) & 15;

        newSize = (10 + (ulong)bytes[3]) |
                  ((ulong)bytes[2] << 8) |
                  ((ulong)bytes[1] << 16) |
                  ((ulong)bytes[0] << 24);
        //End Dan Code

        ExtHeaderSize = newSize;
        // next we read the number of flag bytes

        ExtNumFlagBytes = Convert.ToInt32(_br.ReadByte());

        // read the flag byte(s) and set the flags
        var extFlags = BitReader.ToBitBools(_br.ReadBytes(ExtNumFlagBytes));

        EbUpdate = extFlags[1];
        EcCrc = extFlags[2];
        EdRestrictions = extFlags[3];

        // check for flags
        if (EbUpdate)
        {
            // this tag has no data but will have a null byte so we need to read it in
            //Flag data length      $00

            _br.ReadByte();
        }

        if (EcCrc)
        {
            //        Flag data length       $05
            //       Total frame CRC    5 * %0xxxxxxx
            // read the first byte and check to make sure it is 5.  if not the header is corrupt
            // we will still try to process but we may be funked.

            var extCFlagDataLength = Convert.ToInt32(_br.ReadByte());
            if (extCFlagDataLength == 5)
            {
                extHeaderSize = _br.ReadChars(5); // I use this to read the bytes in from the file
                bytes = new int[4]; // for bit shifting
                newSize = 0; // for the final number
                // The ID3v2 tag size is encoded with four bytes
                // where the most significant bit (bit 7)
                // is set to zero in every byte,
                // making a total of 28 bits.
                // The zeroed bits are ignored
                //
                // Some bit grinding is necessary.  Hang on.


                bytes[4] = extHeaderSize[4] | ((extHeaderSize[3] & 1) << 7);
                bytes[3] = ((extHeaderSize[3] >> 1) & 63) | ((extHeaderSize[2] & 3) << 6);
                bytes[2] = ((extHeaderSize[2] >> 2) & 31) | ((extHeaderSize[1] & 7) << 5);
                bytes[1] = ((extHeaderSize[1] >> 3) & 15) | ((extHeaderSize[0] & 15) << 4);
                bytes[0] = (extHeaderSize[0] >> 4) & 7;

                newSize = (10 + (ulong)bytes[4]) |
                          ((ulong)bytes[3] << 8) |
                          ((ulong)bytes[2] << 16) |
                          ((ulong)bytes[1] << 24) |
                          ((ulong)bytes[0] << 32);

                ExtHeaderSize = newSize;
            }
            // we are fucked.  do something
        }

        if (EdRestrictions)
        {
            // Flag data length       $01
            //Restrictions           %ppqrrstt

            // advance past flag data lenght byte
            _br.ReadByte();

            ExtDRestrictions = _br.ReadByte();
        }
    }
}
