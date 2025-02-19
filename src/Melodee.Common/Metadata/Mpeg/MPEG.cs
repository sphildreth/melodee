using System.Globalization;
using ATL;
using J2N.Text;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Melodee.Common.Metadata.Mpeg;

/// <summary>
/// Summary description for MPEG.
/// </summary>
public class Mpeg
{
    public double LengthMs { get; set; }
    
    public TimeSpan Length => TimeSpan.FromMilliseconds(LengthMs);
    public long AudioBytes { get; set; }

    public long FileSize { get; set; }

    public string Filename { get; set; }
    public string Version { get; set; }
    public string Layer { get; set; }
    public bool Protection { get; set; }
    public string Bitrate { get; set; }
    public string Frequency { get; set; }
    public bool Padding { get; set; }
    public bool Private { get; set; }
    public string ChannelMode { get; set; }
    
    public int Channels { get; set; }
    
    public string ModeExtension { get; set; }
    public bool CopyRight { get; set; }
    public bool Original { get; set; }
    public string Emphasis { get; set; }

    public long HeaderPosition;

    private BinaryReader _br;

    public bool IsAudioNeedsConversion => IsValid && !IsMp3MimeType && !IsVideoType && IsLengthOk;
    
    public bool IsValid => IsBitrateOk &&
                           IsFrequencyOk &&
                           IsLayerOk &&
                           IsVersionOk;

    public bool IsBitrateOk => SafeParser.ToNumber<int>(Bitrate) > 0;

    public bool IsLayerOk => Layer.Nullify() != null && (Layer.Equals("Layer I") || Layer.Equals("Layer II") || Layer.Equals("Layer III") || string.Equals(Layer, "reserved", StringComparison.OrdinalIgnoreCase));

    public bool IsFrequencyOk => SafeParser.ToNumber<int>(Frequency) > 95 || string.Equals(Frequency, "reserved", StringComparison.OrdinalIgnoreCase);

    public bool IsVideoType => MimeType.Nullify() != null && MimeType!.StartsWith("VIDEO/", StringComparison.OrdinalIgnoreCase);
    
    public bool IsMp3MimeType => MimeType is "audio/mpeg" or "audio/mp3";
    
    public bool IsLengthOk => LengthMs > 0;

    public string? MimeType => MimeTypes.GetMimeType(Filename);

    public bool IsVersionOk => Version.Nullify() != null && (Version.Equals("MPEG Version 1") || Version.Equals("MPEG Version 2") || Version.Equals("MPEG Version 2.5"));

    public void Read(string fileName)
    {
        Filename = fileName;
        Task.Run(async () => await ReadAsync()).Wait();
    }

    public Mpeg(string fileName)
    {
        Filename = fileName;
    }

    public async Task ReadAsync(CancellationToken cancellationToken = default)
    {
        await using (var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read))
        {
            using (_br = new BinaryReader(fs))
            {
                var headerBytes = FindHeader();
                ParseHeader(headerBytes);
            }
        }
        
        var fileInfo = new FileInfo(Filename);
        FileSize = fileInfo.Length;
        AudioBytes = FileSize - HeaderPosition;
        var bitrate = SafeParser.ToNumber<int>(Bitrate);
        if (bitrate > 0)
        {
            LengthMs = AudioBytes * 8 / (1000 * bitrate);
        }        
        if (!IsValid)
        {
            var track = new Track(Filename);
            AudioBytes = track.TechnicalInformation.AudioDataSize;
            Bitrate = track.Bitrate.ToString();
            ChannelMode = track.ChannelsArrangement?.Description;
            Channels = track.ChannelsArrangement?.NbChannels ?? 0;
            Frequency = track.SampleRate.ToString(CultureInfo.InvariantCulture);
            LengthMs = track.DurationMs;
        }        
    }

    private void CalculateLength()
    {
        var fi = new FileInfo(Filename);
        FileSize = fi.Length;
        AudioBytes = FileSize - HeaderPosition;
        var bitrate = Convert.ToInt32(Bitrate);
        if (bitrate > 0)
        {
            LengthMs = AudioBytes * 8 / (1000 * bitrate);
        }
    }

    public void ParseHeader(byte[]? headerBytes)
    {
        var hb = BitReader.ToBitBools(headerBytes);
        if (hb.Length < 1)
        {
            return;
        }
        var boolHeader = BitReader.ToBitBools(headerBytes);
        ParseVersion(boolHeader[11], boolHeader[12]);
        ParseLayer(boolHeader[13], boolHeader[14]);
        Protection = boolHeader[15];
        ParseBitRate(boolHeader[16], boolHeader[17], boolHeader[18], boolHeader[19]);
        ParseFrequency(boolHeader[20], boolHeader[21]);
        Padding = boolHeader[22];
        Private = boolHeader[23];
        ParseChannelMode(boolHeader[24], boolHeader[25]);
        ParseModeExtension(boolHeader[26], boolHeader[27]);
        CopyRight = boolHeader[28];
        Original = boolHeader[29];
        ParseEmphasis(boolHeader[30], boolHeader[31]);
    }

    private void ParseFrequency(bool b1, bool b2)
    {
        if (b1)
        {
            if (b2)
            {
                //"11"
                Frequency = "reserved";
            }
            else
            {
                // "01"
                switch (Version)
                {
                    case "MPEG Version 1":
                        Frequency = "32000";
                        break;
                    case "MPEG Version 2":
                        Frequency = "16000";
                        break;
                    case "MPEG Version 2.5":
                        Frequency = "8000";
                        break;
                }
            }
        }
        else
        {
            if (b2)
            {
                //"01"
                switch (Version)
                {
                    case "MPEG Version 1":
                        Frequency = "32000";
                        break;
                    case "MPEG Version 2":
                        Frequency = "16000";
                        break;
                    case "MPEG Version 2.5":
                        Frequency = "8000";
                        break;
                }
            }
            else
            {
                // "00"
                switch (Version)
                {
                    case "MPEG Version 1":
                        Frequency = "44100";
                        break;
                    case "MPEG Version 2":
                        Frequency = "22050";
                        break;
                    case "MPEG Version 2.5":
                        Frequency = "11025";
                        break;
                }
            }
        }
    }

    private void ParseModeExtension(bool b1, bool b2)
    {
        if (b1)
        {
            if (b2)
            {
                if (Layer.Equals("Layer III"))
                {
                    // "11", L3
                    ModeExtension = "IS+MS";
                }
                else
                {
                    // "11", L1 or L2
                    ModeExtension = "16-31";
                }
            }
            else
            {
                if (Layer.Equals("Layer III"))
                {
                    // "10", L3
                    ModeExtension = "MS";
                }
                else
                {
                    // "10", L1 or L2
                    ModeExtension = "12-31";
                }
            }
        }
        else
        {
            if (b2)
            {
                if (Layer.Equals("Layer III"))
                {
                    // "01", L3
                    ModeExtension = "IS";
                }
                else
                {
                    // "01", L1 or L2
                    ModeExtension = "8-31";
                }
            }
            else
            {
                if (Layer.Equals("Layer III"))
                {
                    // "00", L3
                    ModeExtension = "";
                }
                else
                {
                    // "00", L1 or L2
                    ModeExtension = "4-31";
                }
            }
        }
    }

    private void ParseEmphasis(bool b1, bool b2)
    {
        //00 - none
        //01 - 50/15 ms
        //10 - reserved
        //11 - CCIT J.17

        if (b1)
        {
            if (b2)
            {
                //"11"
                Emphasis = "CCIT J.17";
            }
            else
            {
                //"10"
                Emphasis = "reserved";
            }
        }
        else
        {
            if (b2)
            {
                //"01"
                Emphasis = "50/15 ms";
            }
            else
            {
                //"00"
                Emphasis = "none";
            }
        }
    }

    private void ParseChannelMode(bool b1, bool b2)
    {
        //00 - Stereo
        //01 - Joint stereo (Stereo)
        //10 - Dual channel (Stereo)
        //11 - Single channel (Mono)
        if (b1)
        {
            if (b2)
            {
                //"11"
                ChannelMode = "Single Channel";
                Channels = 1;
            }
            else
            {
                //"10"
                ChannelMode = "Dual Channel";
                Channels = 2;
            }
        }
        else
        {
            if (b2)
            {
                //"01"
                ChannelMode = "Joint Stereo";
                Channels = 2;
            }
            else
            {
                //"00"
                ChannelMode = "Stereo";
                Channels = 2;
            }
        }
    }

    private void ParseVersion(bool b1, bool b2)
    {
        // get the MPEG Audio Version ID
        //MPEG Audio version ID
        //00 - MPEG Version 2.5
        //01 - reserved
        //10 - MPEG Version 2 (ISO/IEC 13818-3)
        //11 - MPEG Version 1 (ISO/IEC 11172-3) 
        if (b1)
        {
            if (b2)
            {
                Version = "MPEG Version 1";
            }
            else
            {
                Version = "MPEG Version 2";
            }
        }
        else
        {
            if (b2)
            {
                Version = "reserved";
            }
            else
            {
                Version = "MPEG Version 2.5";
            }
        }
    }


    private void ParseLayer(bool b1, bool b2)
    {
        if (b1)
        {
            if (b2)
            {
                // if "11"
                Layer = "Layer I";
            }
            else
            {
                // "10"
                Layer = "Layer II";
            }
        }
        else
        {
            if (b2)
            {
                // "01"
                Layer = "Layer III";
            }
            else
            {
                // "00"
                Layer = "reserved";
            }
        }
    }

    private void ParseBitRate(bool b1, bool b2, bool b3, bool b4)
    {
        // I know there is a more elegant way than this.
        if (b1)
        {
            if (b2)
            {
                if (b3)
                {
                    if (b4)
                    {
                        #region "1111"

                        Bitrate = "bad";

                        #endregion
                    }
                    else
                    {
                        #region "1110"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "448";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "384";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "320";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "256";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "160";
                            }
                        }

                        #endregion
                    }
                }
                else
                {
                    if (b4)
                    {
                        #region "1101"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "416";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "320";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "256";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "224";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "144";
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region "1100"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "384";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "256";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "224";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "192";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "128";
                            }
                        }

                        #endregion
                    }
                }
            }
            else //b2 not set
            {
                if (b3)
                {
                    if (b4)
                    {
                        #region "1011"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "352";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "224";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "192";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "176";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "112";
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region "1010"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "320";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "192";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "160";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "160";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "96";
                            }
                        }

                        #endregion
                    }
                }
                else
                {
                    if (b4)
                    {
                        #region "1001"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "288";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "160";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "128";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "144";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "80";
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region "1000"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "256";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "128";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "112";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "128";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "64";
                            }
                        }

                        #endregion
                    }
                }
            }
        }
        else
        {
            if (b2)
            {
                if (b3)
                {
                    if (b4)
                    {
                        #region "0111"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "224";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "112";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "96";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "112";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "56";
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region "0110"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "192";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "96";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "80";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "96";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "48";
                            }
                        }

                        #endregion
                    }
                }
                else
                {
                    if (b4)
                    {
                        #region "0101"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "160";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "80";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "64";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "80";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "40";
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region "0100"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "128";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "64";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "56";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "64";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "32";
                            }
                        }

                        #endregion
                    }
                }
            }
            else //b2 not set
            {
                if (b3)
                {
                    if (b4)
                    {
                        #region "0011"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "96";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "56";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "48";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "56";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "24";
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region "0010"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "64";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "48";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "40";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "48";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "16";
                            }
                        }

                        #endregion
                    }
                }
                else
                {
                    if (b4)
                    {
                        #region "0001"

                        if (Version.EndsWith("1"))
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V1, L1
                                Bitrate = "32";
                            }
                            else if (Layer.EndsWith(" II"))
                            {
                                // v1, L2
                                Bitrate = "32";
                            }
                            else
                            {
                                // V1, L3
                                Bitrate = "32";
                            }
                        }
                        else
                        {
                            if (Layer.EndsWith(" I"))
                            {
                                // V2, L1
                                Bitrate = "32";
                            }
                            else
                            {
                                // V2, L2 & L3
                                Bitrate = "8";
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region "1000"

                        Bitrate = "free";

                        #endregion
                    }
                }
            }
        }
    }


    private byte[]? FindHeader()
    {
        var thisByte = _br.ReadByte();
        while (_br.BaseStream.Position < _br.BaseStream.Length)
        {
            if (Convert.ToInt32(thisByte) == 255)
            {
                var thatByte = BitReader.ToBitBool(_br.ReadByte());
                _br.BaseStream.Position--;

                if (thatByte[0] && thatByte[1] && thatByte[2])
                {
                    // we found the sync.  
                    HeaderPosition = _br.BaseStream.Position - 1;
                    var retByte = new byte [4];
                    retByte[0] = thisByte;
                    retByte[1] = _br.ReadByte();
                    retByte[2] = _br.ReadByte();
                    retByte[3] = _br.ReadByte();
                    return retByte;
                }
                else
                {
                    thisByte = _br.ReadByte();
                }
            }
            else
            {
                thisByte = _br.ReadByte();
            }
        }

        return null;
    }
}
