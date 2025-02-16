namespace Melodee.Common.Metadata.Mpeg
{
	/// <summary>
	/// Summary description for BitReader.
	/// </summary>
	public class BitReader
    {
		public static bool GetBitAtPosition (byte b, int position)
		{
			bool[] bitArray = ToBitBool(b);
			return bitArray[position];
		}

		public static bool[] ToBitBool( byte newByte )
		{

			// get the value of hte byte
			int myNum = System.Convert.ToInt32(newByte);

			// make a bool array for the bits.
			// 0 == falseu
			// 1 == true; duh
			bool[] myByte = new bool[8];
			for (int i = 0; i < 8; i++)
			{
				// ANDing against each bit to set the bool value
				if ((myNum & (1 << (7 - i))) != 0)	
				{
					myByte[i] = true;
				}
			}
			return myByte;
		}

		public static char[] ToBitChar( byte newByte )
		{
			char[] myChar = new char[8];

			bool[] myBool = ToBitBool(newByte);
			for (int i = 0; i < 8; i++) 
			{
				if (myBool[i])
				{
					myChar[i] = System.Convert.ToChar("1");
				}
				else
				{
					myChar[i] = System.Convert.ToChar("0");
				}
			}
			return myChar;	
			
		}

		public static char[] ToBitChars( byte[] myBytes )
		{
			char[] myChars = new char[myBytes.Length * 8];
			int i = 0;
			foreach (byte b in myBytes)
			{
				bool[] bit = ToBitBool( b );
				for (int j = 0; j < 8; j++)
				{
					if (bit[j])
					{
						myChars[i++] = System.Convert.ToChar("1");
					}
					else
					{
						myChars[i++] = System.Convert.ToChar("0");
					}
				}
			}
			return myChars;
		}

		public static bool[] ToBitBools( byte[] myBytes )
		{
			bool[] myBools = new bool[myBytes.Length * 8 ];
			int i = 0;
			foreach (byte b in myBytes)
			{
				bool[] bit = ToBitBool( b );
				for (int j = 0; j < 8; j++) 
				{
					if (bit[j])
					{
						myBools[i++] = true;
					}
					else
					{
						myBools[i++] = false;
					}
				}
			}
			return myBools;
		}

		public static byte ToByteChar( char[] myChar )
		{
			int myInt = 0;
			for (int i = 0; i < 8 ; i++ )
			{
				if ( myChar[i].Equals("1"))
					myInt += (int)Math.Pow(2, 7 - i);
			}
			return System.Convert.ToByte(myInt);
		}

		public static byte ToByteBool( bool[] myBool )
		{
			int myInt = 0;
			for (int i = 0; i < 8 ; i++ )
			{
				if ( myBool[i])
					myInt += (int)Math.Pow(2, 7 - i);
			}
			return System.Convert.ToByte(myInt);
		}



		public BitReader()
		{
			//
			// TODO: Add constructor logic here
			//

		}
	}
}
