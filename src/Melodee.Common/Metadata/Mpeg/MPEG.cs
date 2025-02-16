using Melodee.Common.Utility;

namespace Melodee.Common.Metadata.Mpeg
{
	/// <summary>
	/// Summary description for MPEG.
	/// </summary>
	public class Mpeg
	{
		public long Length;  // in seconds
		public long AudioBytes;
		public long FileSize;
        
		public string Filename;
		public string Version;
		public string Layer;
		public bool Protection;
		public string Bitrate;
		public string Frequency;
		public bool Padding;
		public bool Private;
		public string ChannelMode;
		public string ModeExtension;
		public bool CopyRight;
		public bool Original;
		public string Emphasis;

		public long HeaderPosition;

		private BinaryReader _br;
        public bool IsValid => SafeParser.ToNumber<int>(Bitrate) > 0 && 
                               IsFrequencyOk && 
                               IsLayerOk && 
                               IsVersionOk && 
                               IsMp3MimeType;
        public bool IsLayerOk => Layer.Equals("Layer I") || Layer.Equals("Layer II") || Layer.Equals("Layer III");
        
        public bool IsFrequencyOk => SafeParser.ToNumber<int>(Frequency) > 95 || Frequency.Equals("reserved");
        
        public bool IsMp3MimeType => MimeType is "audio/mpeg" or "audio/mp3";
        
        public string? MimeType => MimeTypes.GetMimeType(Filename);
        
        public bool IsVersionOk => Version.Equals("MPEG Version 1") || Version.Equals("MPEG Version 2") || Version.Equals("MPEG Version 2.5");
        
		public void Read (string fileName)
		{
			this.Filename = fileName;
			Task.Run(async () => await ReadAsync()).Wait();
		}

        public Mpeg( string fileName)
        {
            Filename = fileName;
        }

        public async Task ReadAsync(CancellationToken cancellationToken = default)
        {
            await using (var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                using (_br = new BinaryReader(fs))
                {
                    byte[] headerBytes = FindHeader();
                    ParseHeader(headerBytes);
                }
            }
        }

		private void CalculateLength()
		{
			FileInfo fi = new FileInfo(this.Filename);
			this.FileSize = fi.Length;
			this.AudioBytes = this.FileSize - this.HeaderPosition;
			int bitrate = System.Convert.ToInt32(this.Bitrate);
			if (bitrate > 0)
			{
				this.Length = (this.AudioBytes * 8) / (1000 * bitrate);
			}
		}

		public void  ParseHeader (byte[] headerBytes)
		{
			bool [] boolHeader = BitReader.ToBitBools(headerBytes);
			ParseVersion(boolHeader[11], boolHeader[12]);
			ParseLayer(boolHeader[13], boolHeader[14]);
			this.Protection = boolHeader[15];
			ParseBitRate(boolHeader[16], boolHeader[17], boolHeader[18], boolHeader[19]);
			ParseFrequency(boolHeader[20], boolHeader[21]);
			this.Padding = boolHeader[22];
			this.Private = boolHeader[23];
			ParseChannelMode(boolHeader[24], boolHeader[25]);
			ParseModeExtension(boolHeader[26], boolHeader[27]);
			this.CopyRight = boolHeader[28];
			this.Original = boolHeader[29];
			ParseEmphasis(boolHeader[30], boolHeader[31]);
            
		}

		private void ParseFrequency ( bool b1, bool b2)
		{
			if (b1)
			{
				if(b2)
				{
					//"11"
					this.Frequency = "reserved";
				}
				else 
				{
					// "01"
					switch (this.Version)
					{
						case "MPEG Version 1":
							this.Frequency = "32000";
							break;
						case "MPEG Version 2":
							this.Frequency = "16000";
							break;
						case "MPEG Version 2.5":
							this.Frequency = "8000";
							break;
					}
				}
			}
			else
			{
				if (b2)
				{
					//"01"
					switch (this.Version)
					{
						case "MPEG Version 1":
							this.Frequency = "32000";
							break;
						case "MPEG Version 2":
							this.Frequency = "16000";
							break;
						case "MPEG Version 2.5":
							this.Frequency = "8000";
							break;
					}
				}
				else
				{
					// "00"
					switch (this.Version)
					{
						case "MPEG Version 1":
							this.Frequency = "44100";
							break;
						case "MPEG Version 2":
							this.Frequency = "22050";
							break;
						case "MPEG Version 2.5":
							this.Frequency = "11025";
							break;
					}
				}
			}
		
		}

		private void ParseModeExtension (bool b1, bool b2)
		{

			if (b1)
			{
				if (b2)
				{
					if (this.Layer.Equals("Layer III"))
					{
						// "11", L3
						this.ModeExtension = "IS+MS";
					}
					else
					{
						// "11", L1 or L2
						this.ModeExtension = "16-31";
					}
				}
				else
				{
					if (this.Layer.Equals("Layer III"))
					{
						// "10", L3
						this.ModeExtension = "MS";
					}
					else
					{
						// "10", L1 or L2
						this.ModeExtension = "12-31";
					}
				}
			}
			else
			{
				if (b2)
				{
					if (this.Layer.Equals("Layer III"))
					{
						// "01", L3
						this.ModeExtension = "IS";
					}
					else
					{
						// "01", L1 or L2
						this.ModeExtension = "8-31";
					}
				}
				else
				{
					if (this.Layer.Equals("Layer III"))
					{
						// "00", L3
						this.ModeExtension = "";
					}
					else
					{
						// "00", L1 or L2
						this.ModeExtension = "4-31";
					}
				}
			}
		}

		private void ParseEmphasis (bool b1, bool b2)
		{
			//00 - none
			//01 - 50/15 ms
			//10 - reserved
			//11 - CCIT J.17

			if (b1)
			{
				if(b2)
				{
					//"11"
					this.Emphasis = "CCIT J.17";
				}
				else
				{
					//"10"
					this.Emphasis = "reserved";
				}
			}
			else
			{
				if (b2)
				{
					//"01"
					this.Emphasis = "50/15 ms";
				}
				else
				{
					//"00"
					this.Emphasis = "none";
				}
			}
		
		}

		private void ParseChannelMode (bool b1, bool b2)
		{
			//00 - Stereo
			//01 - Joint stereo (Stereo)
			//10 - Dual channel (Stereo)
			//11 - Single channel (Mono)
			if (b1)
			{
				if(b2)
				{
					//"11"
					this.ChannelMode = "Single Channel";
				}
				else
				{
					//"10"
					this.ChannelMode = "Dual Channel";
				}
			}
			else
			{
				if (b2)
				{
					//"01"
					this.ChannelMode = "Joint Stereo";
				}
				else
				{
					//"00"
					this.ChannelMode = "Stereo";
				}
			}
		
		}

		private void ParseVersion (bool b1, bool b2)
		{
			// get the MPEG Audio Version ID
			//MPEG Audio version ID
			//00 - MPEG Version 2.5
			//01 - reserved
			//10 - MPEG Version 2 (ISO/IEC 13818-3)
			//11 - MPEG Version 1 (ISO/IEC 11172-3) 
			if (b1)
			{
				if(b2)
				{
					this.Version = "MPEG Version 1";
				}
				else
				{
					this.Version = "MPEG Version 2";
				}
			}
			else
			{
				if (b2)
				{
					this.Version = "reserved";
				}
				else
				{
					this.Version = "MPEG Version 2.5";
				}
			}
		
		}




		private void ParseLayer (bool b1, bool b2)
		{
			if (b1)
			{
				if(b2)
				{
					// if "11"
					this.Layer = "Layer I";
				}
				else
				{
					// "10"
					this.Layer = "Layer II";
				}
			}
			else
			{
				if (b2)
				{
					// "01"
					this.Layer = "Layer III";
				}
				else
				{
					// "00"
					this.Layer = "reserved";
				}
			}
		}

		private void ParseBitRate (bool b1, bool b2, bool b3, bool b4)
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
							this.Bitrate = "bad";
							#endregion
						}
						else
						{
							#region "1110"
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "448";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "384";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "320";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "256";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "160";
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
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "416";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "320";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "256";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "224";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "144";
								}
							}
							#endregion
						}
						else
						{
							#region "1100"
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "384";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "256";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "224";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "192";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "128";
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
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "352";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "224";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "192";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "176";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "112";
								}
							}
							#endregion
						}
						else
						{
							#region "1010"
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "320";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "192";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "160";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "160";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "96";
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
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "288";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "160";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "128";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "144";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "80";
								}
							}
							#endregion
						}
						else
						{
							#region "1000"
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "256";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "128";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "112";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "128";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "64";
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
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "224";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "112";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "96";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "112";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "56";
								}
							}
							#endregion
						}
						else
						{
							#region "0110"
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "192";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "96";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "80";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "96";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "48";
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
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "160";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "80";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "64";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "80";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "40";
								}
							}
							#endregion
						}
						else
						{
							#region "0100"
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "128";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "64";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "56";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "64";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "32";
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
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "96";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "56";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "48";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "56";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "24";
								}
							}
							#endregion
						}
						else
						{
							#region "0010"
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "64";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "48";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "40";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "48";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "16";
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
							if (this.Version.EndsWith("1"))
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V1, L1
									this.Bitrate = "32";
								}
								else if (this.Layer.EndsWith(" II"))
								{
									// v1, L2
									this.Bitrate = "32";
								}
								else 
								{
									// V1, L3
									this.Bitrate = "32";
								}
							}
							else 
							{
								if (this.Layer.EndsWith(" I"))
								{
									// V2, L1
									this.Bitrate = "32";
								}
								else 
								{
									// V2, L2 & L3
									this.Bitrate = "8";
								}
							}
							#endregion
						}
						else
						{
							#region "1000"
							this.Bitrate = "free";
							#endregion
						}
					}
				}
			}
		}





		private byte[] FindHeader ()
		{
			byte thisByte = _br.ReadByte();
			while ( _br.BaseStream.Position < _br.BaseStream.Length )
			{
				if (System.Convert.ToInt32(thisByte) == 255)
				{
					bool[] thatByte = BitReader.ToBitBool(_br.ReadByte());
					_br.BaseStream.Position --;

					if ( thatByte[0] && thatByte[1] && thatByte[2])
					{
						// we found the sync.  
						this.HeaderPosition = _br.BaseStream.Position - 1;
						byte [] retByte = new byte [4];
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
}
