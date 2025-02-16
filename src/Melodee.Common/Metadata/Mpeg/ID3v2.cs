using System.Collections;
using System.Text;

namespace Melodee.Common.Metadata.Mpeg
{
	/// <summary>
	/// Summary description for ID3v2.
	/// </summary>
	public class Id3V2
	{
		public string Filename;

		// id3v2 header
		public int MajorVersion;
		public int MinorVersion;
		
		public bool FaUnsynchronisation;
		public bool FbExtendedHeader;
		public bool FcExperimentalIndicator;
		public bool FdFooter;

		// ext header 
		public ulong ExtHeaderSize;
		public int ExtNumFlagBytes;

		public bool EbUpdate;
		public bool EcCrc;
		public bool EdRestrictions;

		public ulong ExtCCrc;
		public byte ExtDRestrictions;



		private BinaryReader _br;
		private byte[] _ba;
		public bool HasTag;

		public ulong HeaderSize;
		
		public Hashtable FramesHash;
		public ArrayList Frames;

		private bool _fileOpen;

		//public bool header;

		// song info
		public string Title;
		public string Artist;
		public string Album;
		public string Year;
		public string Comment;
		public string Genre;
		public string Track;
		public string TotalTracks;
		



		private void Initialize_Components()
		{
			this.Filename = "";

		
			this.MajorVersion = 0;
			this.MinorVersion = 0;

			this.FaUnsynchronisation = false;
			this.FbExtendedHeader = false;
			this.FcExperimentalIndicator = false;

			this._fileOpen = false;

			this.Frames = new ArrayList();
			this.FramesHash = new Hashtable();

			this.Album = "";
			this.Artist = "";
			this.Comment = "";
			this.ExtCCrc = 0;
			this.EbUpdate = false;
			this.EcCrc = false;
			this.EdRestrictions = false;
			this.ExtDRestrictions = 0;
			this.ExtHeaderSize = 0;
			this.ExtNumFlagBytes = 0;
			this._fileOpen = false;
			this.HasTag = false;
			this.HeaderSize = 0;
			this.Title = "";
			this.TotalTracks = "";
			this.Track = "";
			this.Year = "";
			
		}


		public Id3V2()
		{
			Initialize_Components();
		}
		 
		public Id3V2( string fileName)
		{
			Initialize_Components();
			this.Filename = fileName;
		}



	
		private void CloseFile()
		{
			_br.Close();
			
		}



		private void ReadHeader ( )
		{


			// bring in the first three bytes.  it must be ID3 or we have no tag
			// TODO add logic to check the end of the file for "3D1" and other
			// possible starting spots
			string id3Start = new string (_br.ReadChars(3));

			// check for a tag
			if  (!id3Start.Equals("ID3"))
			{
				// TODO we are fucked.
				//throw id3v2ReaderException;
				this.HasTag = false;
				return;

			}
			else
			{
				this.HasTag = true;

				// read id3 version.  2 bytes:
				// The first byte of ID3v2 version is it's major version,
				// while the second byte is its revision number
				this.MajorVersion = System.Convert.ToInt32(_br.ReadByte());
				this.MinorVersion = System.Convert.ToInt32(_br.ReadByte());
		
				//read next byte for flags
				bool [] boolar = BitReader.ToBitBool(_br.ReadByte());
				// set the flags
				this.FaUnsynchronisation= boolar[0];
				this.FbExtendedHeader = boolar[1];
				this.FcExperimentalIndicator = boolar[2];

				// read teh size
				// this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
				//Dan Code 
				char[] tagSize = _br.ReadChars(4);    // I use this to read the bytes in from the file
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
		
				this.HeaderSize = newSize;
			}
		}

		private void ReadFrames ()
		{
			Id3V2Frame f = new Id3V2Frame();
			do 
			{
				f = f.ReadFrame(_br, this.MajorVersion);
				
				// check if we have hit the padding.
				if (f.Padding == true)
				{
					//we hit padding.  lets advance to end and stop reading.
					_br.BaseStream.Position = System.Convert.ToInt64(HeaderSize);
					break;
				}
				this.Frames.Add(f);
				this.FramesHash.Add(f.FrameName, f);
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
			} while (_br.BaseStream.Position  <= System.Convert.ToInt64(this.HeaderSize));
		}		
		
		public void Read()
		{
						
			FileStream fs = new FileStream(this.Filename, FileMode.Open, FileAccess.Read);
			_br = new BinaryReader (fs);

			ReadHeader();

			if (this.HasTag)
			{
				if (this.FbExtendedHeader)
				{
					ReadExtendedHeader();
				}

			
				
			
				ReadFrames();
		
				if (this.FdFooter)
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
			if (this.MajorVersion == 2)
			{
				if(this.FramesHash.Contains("TT2"))
				{
					
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TT2"]).FrameContents;
					StringBuilder sb = new StringBuilder();
					byte textEncoding;
					

					for (int i = 1; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						
						//this.Title = myString.Substring(1);
						this.Title = sb.ToString();
					}
				}
			
		

				if(this.FramesHash.Contains("TP1"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TP1"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Artist = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("TAL"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TAL"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Album = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("TYE"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TYE"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Year = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("TRK"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TRK"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Track = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("TCO"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TCO"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Genre = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("COM"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["COM"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Comment = sb.ToString();
					}
				}
			}
			else 
			{ // $id3info["majorversion"] > 2
				if(this.FramesHash.Contains("TIT2"))
				{
					
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TIT2"]).FrameContents;
					StringBuilder sb = new StringBuilder();
					byte textEncoding;
					

					for (int i = 1; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						
						//this.Title = myString.Substring(1);
						this.Title = sb.ToString();
					}
				}
			
		

				if(this.FramesHash.Contains("TPE1"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TPE1"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Artist = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("TALB"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TALB"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Album = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("TYER"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TYER"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Year = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("TRCK"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TRCK"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Track = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("TCON"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["TCON"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Genre = sb.ToString();
					}
				}
				if(this.FramesHash.Contains("COMM"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((Id3V2Frame)this.FramesHash["COMM"]).FrameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Comment = sb.ToString();
					}
				}
			}
	
			string [] trackHolder = this.Track.Split('/');
			this.Track = trackHolder[0];
			if (trackHolder.Length > 1)
				this.TotalTracks = trackHolder[1];
				
		}

		private void ReadFooter()
		{
			
			// bring in the first three bytes.  it must be ID3 or we have no tag
			// TODO add logic to check the end of the file for "3D1" and other
			// possible starting spots
			string id3Start = new string (_br.ReadChars(3));

			// check for a tag
			if  (!id3Start.Equals("3DI"))
			{
				// TODO we are fucked.  not really we just don't ahve a tag
				// and we need to bail out gracefully.
				//throw id3v23ReaderException;
			}
			else 
			{
				// we have a tag
				this.HasTag = true;
			}

			// read id3 version.  2 bytes:
			// The first byte of ID3v2 version is it's major version,
			// while the second byte is its revision number
			this.MajorVersion = System.Convert.ToInt32(_br.ReadByte());
			this.MinorVersion = System.Convert.ToInt32(_br.ReadByte());

			// here is where we get fancy.  I am useing silisoft's php code as 
			// a reference here.  we are going to try and parse for 2.2, 2.3 and 2.4
			// in one pass.  hold on!!

			if ((this.HasTag) && (this.MajorVersion <= 4)) // probably won't work on higher versions
			{
				// (%ab000000 in v2.2, %abc00000 in v2.3, %abcd0000 in v2.4.x)
				//read next byte for flags
				bool [] boolar = BitReader.ToBitBool(_br.ReadByte());
				// set the flags
				if (this.MajorVersion == 2)
				{
					this.FaUnsynchronisation = boolar[0];
					this.FbExtendedHeader = boolar[1];
				}
				else if ( this.MajorVersion == 3 )
				{
					// set the flags
					this.FaUnsynchronisation = boolar[0];
					this.FbExtendedHeader = boolar[1];
					this.FcExperimentalIndicator = boolar[2];
				}
				else if (this.MajorVersion == 4)
				{
					// set the flags
					this.FaUnsynchronisation = boolar[0];
					this.FbExtendedHeader = boolar[1];
					this.FcExperimentalIndicator = boolar[2];
					this.FdFooter = boolar[3];
				}

				// read teh size
				// this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
				//Dan Code 
				char[] tagSize = _br.ReadChars(4);    // I use this to read the bytes in from the file
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
		
				this.HeaderSize = newSize;
			

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
			char[] extHeaderSize = _br.ReadChars(4);    // I use this to read the bytes in from the file
			int[] bytes = new int[4];      // for bit shifting
			ulong newSize = 0;    // for the final number
			// The ID3v2 tag size is encoded with four bytes
			// where the most significant bit (bit 7)
			// is set to zero in every byte,
			// making a total of 28 bits.
			// The zeroed bits are ignored
			//
			// Some bit grinding is necessary.  Hang on.
			

			bytes[3] =  extHeaderSize[3]              | ((extHeaderSize[2] & 1) << 7) ;
			bytes[2] = ((extHeaderSize[2] >> 1) & 63) | ((extHeaderSize[1] & 3) << 6) ;
			bytes[1] = ((extHeaderSize[1] >> 2) & 31) | ((extHeaderSize[0] & 7) << 5) ;
			bytes[0] = ((extHeaderSize[0] >> 3) & 15) ;

			newSize  = ((UInt64)10 +	(UInt64)bytes[3] |
				((UInt64)bytes[2] << 8)  |
				((UInt64)bytes[1] << 16) |
				((UInt64)bytes[0] << 24)) ;
			//End Dan Code

			this.ExtHeaderSize = newSize;
			// next we read the number of flag bytes

			this.ExtNumFlagBytes = System.Convert.ToInt32(_br.ReadByte());

			// read the flag byte(s) and set the flags
			bool[] extFlags = BitReader.ToBitBools(_br.ReadBytes(this.ExtNumFlagBytes));

			this.EbUpdate = extFlags[1];
			this.EcCrc = extFlags[2];
			this.EdRestrictions = extFlags[3];

			// check for flags
			if (this.EbUpdate)
			{
				// this tag has no data but will have a null byte so we need to read it in
				//Flag data length      $00

				_br.ReadByte();
			}

			if (this.EcCrc)
			{
				//        Flag data length       $05
				//       Total frame CRC    5 * %0xxxxxxx
				// read the first byte and check to make sure it is 5.  if not the header is corrupt
				// we will still try to process but we may be funked.

				int extCFlagDataLength = System.Convert.ToInt32(_br.ReadByte());
				if (extCFlagDataLength == 5)
				{


					extHeaderSize = _br.ReadChars(5);    // I use this to read the bytes in from the file
					bytes = new int[4];      // for bit shifting
					newSize = 0;    // for the final number
					// The ID3v2 tag size is encoded with four bytes
					// where the most significant bit (bit 7)
					// is set to zero in every byte,
					// making a total of 28 bits.
					// The zeroed bits are ignored
					//
					// Some bit grinding is necessary.  Hang on.
			

					bytes[4] = extHeaderSize[4]		|  ((extHeaderSize[3] & 1) << 7 ) ;
					bytes[3] = ((extHeaderSize[3] >> 1) & 63) | ((extHeaderSize[2] & 3) << 6) ;
					bytes[2] = ((extHeaderSize[2] >> 2) & 31) | ((extHeaderSize[1] & 7) << 5) ;
					bytes[1] = ((extHeaderSize[1] >> 3) & 15) | ((extHeaderSize[0] & 15) << 4) ;
					bytes[0] = ((extHeaderSize[0] >> 4) & 7);

					newSize  = ((UInt64)10 +	(UInt64)bytes[4] |
						((UInt64)bytes[3] << 8)  |
						((UInt64)bytes[2] << 16) |
						((UInt64)bytes[1] << 24) |
						((UInt64)bytes[0] << 32)) ;
				
					this.ExtHeaderSize = newSize;
				}
				else
				{
					// we are fucked.  do something
				}
			}

			if (this.EdRestrictions)
			{
				// Flag data length       $01
				//Restrictions           %ppqrrstt
				
				// advance past flag data lenght byte
				_br.ReadByte();

				this.ExtDRestrictions = _br.ReadByte();

			}

		}

	}

}
