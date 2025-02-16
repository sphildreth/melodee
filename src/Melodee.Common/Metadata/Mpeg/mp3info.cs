namespace Melodee.Common.Metadata.Mpeg
{
	/// <summary>
	/// Summary description for mp3info.
	/// </summary>
	public class Mp3Info
	{
		public string Filename;
		public long FileSize;
		public long Length; // in seconds
		
		public Id3V1 Id3V1;
		public Id3V2 Id3V2;
		public Mpeg Mpeg;

		public bool HasId3V1;
		public bool HasId3V2;

		private void Initialize_Components()
		{
			Filename = "";
		}



		public Mp3Info()
		{
			Initialize_Components();
		
		}

		public Mp3Info(string fileName)
		{
			Initialize_Components();
			this.Filename = fileName;
		}

		private void CalculateLength()
		{
			FileInfo fi = new FileInfo(this.Filename);
			this.FileSize = fi.Length;
			this.Mpeg.AudioBytes = this.FileSize - this.Mpeg.HeaderPosition;
			try
			{
				int bitrate = System.Convert.ToInt32(this.Mpeg.Bitrate);
				if (bitrate > 0)
				{
					if (this.Id3V1.HasTag)
					{
						this.Length = ((this.Mpeg.AudioBytes - 128 )* 8) / (1000 * bitrate);
					} 
					else 
					{
						this.Length = (this.Mpeg.AudioBytes * 8) / (1000 * bitrate);
					}
				}

			}
			catch (Exception e)
			{
				this.Length = 0;
			}


		}
		public async Task ReadAllAsync(string filename, CancellationToken token = default)
		{
			if (this.Filename.Equals(""))
			{
				// we are fucked, we need a filename
				return;
			}
			else
			{
				ReadId3V1(this.Filename);
				ReadId3V2(this.Filename);
                await ReadMpegAsync(this.Filename);
				CalculateLength();
			}
		}

		public void ReadId3V1()
		{
			if (this.Filename.Equals(""))
			{
				// we are fucked we need a filename
			}
			else
			{
				ReadId3V1 (this.Filename);
			}
		}

		public void ReadId3V1(string file)
		{
			// read id3 stuff
			Id3V1 = new Id3V1(file);
			Id3V1.Read();
			this.HasId3V1 = Id3V1.HasTag;;
		}

		public void ReadId3V2()
		{
			if (this.Filename.Equals(""))
			{
				// we are fucked we need a filename
			}
			else
			{
				ReadId3V2 (this.Filename);
			}
		}

		public void ReadId3V2(string file)
		{
			// read id3 stuff
			Id3V2 = new Id3V2(file);
			Id3V2.Read();
			this.HasId3V2 = Id3V2.HasTag;
		}

		public async Task ReadMpegAsync(string file)
		{
			// read id3 stuff
			Mpeg = new Mpeg(file);
            await Mpeg.ReadAsync();
        }

	}
}
