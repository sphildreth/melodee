using System.Text;

namespace Melodee.Common.Metadata.Mpeg
{
	/// <summary>
	/// Reads/writes Id3v1 tags.  lots of help from Paul Lockwood's code
	/// http://www.csharphelp.com/archives/archive226.html
	/// </summary>
	public class Id3V1
	{
		public string Filename;

		public string Title;
		public string Artist;
		public string Album;
		public string Year;
		public string Comment;
		public int GenreId;
		public int Track;

		public bool HasTag;

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


		public Id3V1()
		{
			Initialize_Components();
		}
		
		public Id3V1( string filename )
		{
			Initialize_Components();
			this.Filename = filename;
		}


		public void Read () 
		{
			// Read the 128 byte ID3 tag into a byte array
			FileStream oFileStream;
			oFileStream = new FileStream( this.Filename, FileMode.Open);
			byte[] bBuffer = new byte[128];
			oFileStream.Seek(-128, SeekOrigin.End);
			oFileStream.Read(bBuffer,0, 128);
			oFileStream.Close();
  
			// Convert the Byte Array to a String
			Encoding  instEncoding = new ASCIIEncoding();   // NB: Encoding is an Abstract class
			string id3Tag = instEncoding.GetString(bBuffer);
  
			// If there is an attched ID3 v1.x TAG then read it 
			if (id3Tag .Substring(0,3) == "TAG") 
			{
				this.Title      = id3Tag.Substring(  3, 30).Trim();
				this.Artist     = id3Tag.Substring( 33, 30).Trim();
				this.Album      = id3Tag.Substring( 63, 30).Trim();
				this.Year     = id3Tag.Substring( 93, 4).Trim();
				this.Comment    = id3Tag.Substring( 97,28).Trim();
  
				// Get the track number if TAG conforms to ID3 v1.1
				if (id3Tag[125]==0)
					this.Track = bBuffer[126];
				else
					this.Track = 0;
				this.GenreId = bBuffer[127];

				this.HasTag    = true;
				// ********* IF USED IN ANGER: ENSURE to test for non-numeric year
			}
			else 
			{
				this.HasTag      = false;
			}
		}
  
		public void UpdateMp3Tag () 
		{
			// Trim any whitespace
			this.Title = this.Title.Trim();
			this.Artist = this.Artist.Trim();
			this.Album = this.Album.Trim();
			this.Year   = this.Year.Trim();
			this.Comment = this.Comment.Trim();
  
			// Ensure all properties are correct size
			if (this.Title.Length > 30)   this.Title    = this.Title.Substring(0,30);
			if (this.Artist.Length > 30)  this.Artist   = this.Artist.Substring(0,30);
			if (this.Album.Length > 30)   this.Album    = this.Album.Substring(0,30);
			if (this.Year.Length > 4)     this.Year     = this.Year.Substring(0,4);
			if (this.Comment.Length > 28) this.Comment  = this.Comment.Substring(0,28);
      
			// Build a new ID3 Tag (128 Bytes)
			byte[] tagByteArray = new byte[128];
			for ( int i = 0; i < tagByteArray.Length; i++ ) tagByteArray[i] = 0; // Initialise array to nulls
  
			// Convert the Byte Array to a String
			Encoding  instEncoding = new ASCIIEncoding();   // NB: Encoding is an Abstract class // ************ To DO: Make a shared instance of ASCIIEncoding so we don't keep creating/destroying it
			// Copy "TAG" to Array
			byte[] workingByteArray = instEncoding.GetBytes("TAG"); 
			Array.Copy(workingByteArray, 0, tagByteArray, 0, workingByteArray.Length);
			// Copy Title to Array
			workingByteArray = instEncoding.GetBytes(this.Title);
			Array.Copy(workingByteArray, 0, tagByteArray, 3, workingByteArray.Length);
			// Copy Artist to Array
			workingByteArray = instEncoding.GetBytes(this.Artist);
			Array.Copy(workingByteArray, 0, tagByteArray, 33, workingByteArray.Length);
			// Copy Album to Array
			workingByteArray = instEncoding.GetBytes(this.Album);
			Array.Copy(workingByteArray, 0, tagByteArray, 63, workingByteArray.Length);
			// Copy Year to Array
			workingByteArray = instEncoding.GetBytes(this.Year);
			Array.Copy(workingByteArray, 0, tagByteArray, 93, workingByteArray.Length);
			// Copy Comment to Array
			workingByteArray = instEncoding.GetBytes(this.Comment);
			Array.Copy(workingByteArray, 0, tagByteArray, 97, workingByteArray.Length);
			// Copy Track and Genre to Array
			tagByteArray[126] = System.Convert.ToByte(this.Track);
			tagByteArray[127] = System.Convert.ToByte(this.GenreId);
  
			// SAVE TO DISK: Replace the final 128 Bytes with our new ID3 tag
			FileStream oFileStream = new FileStream(this.Filename , FileMode.Open);
			if (this.HasTag)
				oFileStream.Seek(-128, SeekOrigin.End);
			else
				oFileStream.Seek(0, SeekOrigin.End);
			oFileStream.Write(tagByteArray,0, 128);
			oFileStream.Close();
			this.HasTag  = true;
		}
  
	}
}

