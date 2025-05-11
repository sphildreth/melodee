namespace Melodee.Common.Metadata.Mpeg;

/// <summary>
///     Summary description for mp3info.
/// </summary>
public class Mp3Info
{
    public string Filename;
    public long FileSize;

    public bool HasId3V1;
    public bool HasId3V2;

    public Id3V1 Id3V1;
    public Id3V2 Id3V2;
    public long Length; // in seconds
    public Mpeg Mpeg;


    public Mp3Info()
    {
        Initialize_Components();
    }

    public Mp3Info(string fileName)
    {
        Initialize_Components();
        Filename = fileName;
    }

    private void Initialize_Components()
    {
        Filename = "";
    }

    private void CalculateLength()
    {
        var fi = new FileInfo(Filename);
        FileSize = fi.Length;
        Mpeg.AudioBytes = FileSize - Mpeg.HeaderPosition;
        try
        {
            var bitrate = Convert.ToInt32(Mpeg.Bitrate);
            if (bitrate > 0)
            {
                if (Id3V1.HasTag)
                {
                    Length = (Mpeg.AudioBytes - 128) * 8 / (1000 * bitrate);
                }
                else
                {
                    Length = Mpeg.AudioBytes * 8 / (1000 * bitrate);
                }
            }
        }
        catch (Exception e)
        {
            Length = 0;
        }
    }

    public async Task ReadAllAsync(string filename, CancellationToken token = default)
    {
        if (Filename.Equals(""))
        {
            // we are fucked, we need a filename
        }
        else
        {
            ReadId3V1(Filename);
            ReadId3V2(Filename);
            await ReadMpegAsync(Filename);
            CalculateLength();
        }
    }

    public void ReadId3V1()
    {
        if (Filename.Equals(""))
        {
            // we are fucked we need a filename
        }
        else
        {
            ReadId3V1(Filename);
        }
    }

    public void ReadId3V1(string file)
    {
        // read id3 stuff
        Id3V1 = new Id3V1(file);
        Id3V1.Read();
        HasId3V1 = Id3V1.HasTag;
        ;
    }

    public void ReadId3V2()
    {
        if (Filename.Equals(""))
        {
            // we are fucked we need a filename
        }
        else
        {
            ReadId3V2(Filename);
        }
    }

    public void ReadId3V2(string file)
    {
        // read id3 stuff
        Id3V2 = new Id3V2(file);
        Id3V2.Read();
        HasId3V2 = Id3V2.HasTag;
    }

    public async Task ReadMpegAsync(string file)
    {
        // read id3 stuff
        Mpeg = new Mpeg(file);
        await Mpeg.ReadAsync();
    }
}
