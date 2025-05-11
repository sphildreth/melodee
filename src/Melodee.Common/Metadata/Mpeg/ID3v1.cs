using System.Text;

namespace Melodee.Common.Metadata.Mpeg;

/// <summary>
///     Reads/writes Id3v1 tags.  lots of help from Paul Lockwood's code
///     http://www.csharphelp.com/archives/archive226.html
/// </summary>
public class Id3V1
{
    public string Album;
    public string Artist;
    public string Comment;
    public string Filename;
    public int GenreId;

    public bool HasTag;

    public string Title;
    public int Track;
    public string Year;


    public Id3V1()
    {
        Initialize_Components();
    }

    public Id3V1(string filename)
    {
        Initialize_Components();
        Filename = filename;
    }

    private void Initialize_Components()
    {
        HasTag = false;
        Filename = "";
        Title = "";
        Artist = "";
        Album = "";
        Year = "";
        Comment = "";

        GenreId = 0;
        Track = 0;
    }


    public void Read()
    {
        // Read the 128 byte ID3 tag into a byte array
        FileStream oFileStream;
        oFileStream = new FileStream(Filename, FileMode.Open);
        var bBuffer = new byte[128];
        oFileStream.Seek(-128, SeekOrigin.End);
        oFileStream.Read(bBuffer, 0, 128);
        oFileStream.Close();

        // Convert the Byte Array to a String
        Encoding instEncoding = new ASCIIEncoding(); // NB: Encoding is an Abstract class
        var id3Tag = instEncoding.GetString(bBuffer);

        // If there is an attched ID3 v1.x TAG then read it 
        if (id3Tag.Substring(0, 3) == "TAG")
        {
            Title = id3Tag.Substring(3, 30).Trim();
            Artist = id3Tag.Substring(33, 30).Trim();
            Album = id3Tag.Substring(63, 30).Trim();
            Year = id3Tag.Substring(93, 4).Trim();
            Comment = id3Tag.Substring(97, 28).Trim();

            // Get the track number if TAG conforms to ID3 v1.1
            if (id3Tag[125] == 0)
            {
                Track = bBuffer[126];
            }
            else
            {
                Track = 0;
            }

            GenreId = bBuffer[127];

            HasTag = true;
            // ********* IF USED IN ANGER: ENSURE to test for non-numeric year
        }
        else
        {
            HasTag = false;
        }
    }

    public void UpdateMp3Tag()
    {
        // Trim any whitespace
        Title = Title.Trim();
        Artist = Artist.Trim();
        Album = Album.Trim();
        Year = Year.Trim();
        Comment = Comment.Trim();

        // Ensure all properties are correct size
        if (Title.Length > 30)
        {
            Title = Title.Substring(0, 30);
        }

        if (Artist.Length > 30)
        {
            Artist = Artist.Substring(0, 30);
        }

        if (Album.Length > 30)
        {
            Album = Album.Substring(0, 30);
        }

        if (Year.Length > 4)
        {
            Year = Year.Substring(0, 4);
        }

        if (Comment.Length > 28)
        {
            Comment = Comment.Substring(0, 28);
        }

        // Build a new ID3 Tag (128 Bytes)
        var tagByteArray = new byte[128];
        for (var i = 0; i < tagByteArray.Length; i++)
        {
            tagByteArray[i] = 0; // Initialise array to nulls
        }

        // Convert the Byte Array to a String
        Encoding
            instEncoding =
                new ASCIIEncoding(); // NB: Encoding is an Abstract class // ************ To DO: Make a shared instance of ASCIIEncoding so we don't keep creating/destroying it
        // Copy "TAG" to Array
        var workingByteArray = instEncoding.GetBytes("TAG");
        Array.Copy(workingByteArray, 0, tagByteArray, 0, workingByteArray.Length);
        // Copy Title to Array
        workingByteArray = instEncoding.GetBytes(Title);
        Array.Copy(workingByteArray, 0, tagByteArray, 3, workingByteArray.Length);
        // Copy Artist to Array
        workingByteArray = instEncoding.GetBytes(Artist);
        Array.Copy(workingByteArray, 0, tagByteArray, 33, workingByteArray.Length);
        // Copy Album to Array
        workingByteArray = instEncoding.GetBytes(Album);
        Array.Copy(workingByteArray, 0, tagByteArray, 63, workingByteArray.Length);
        // Copy Year to Array
        workingByteArray = instEncoding.GetBytes(Year);
        Array.Copy(workingByteArray, 0, tagByteArray, 93, workingByteArray.Length);
        // Copy Comment to Array
        workingByteArray = instEncoding.GetBytes(Comment);
        Array.Copy(workingByteArray, 0, tagByteArray, 97, workingByteArray.Length);
        // Copy Track and Genre to Array
        tagByteArray[126] = Convert.ToByte(Track);
        tagByteArray[127] = Convert.ToByte(GenreId);

        // SAVE TO DISK: Replace the final 128 Bytes with our new ID3 tag
        var oFileStream = new FileStream(Filename, FileMode.Open);
        if (HasTag)
        {
            oFileStream.Seek(-128, SeekOrigin.End);
        }
        else
        {
            oFileStream.Seek(0, SeekOrigin.End);
        }

        oFileStream.Write(tagByteArray, 0, 128);
        oFileStream.Close();
        HasTag = true;
    }
}
