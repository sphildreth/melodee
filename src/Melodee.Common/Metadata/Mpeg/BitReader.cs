namespace Melodee.Common.Metadata.Mpeg;

/// <summary>
/// Summary description for BitReader.
/// </summary>
public class BitReader
{
    public static bool GetBitAtPosition(byte b, int position)
    {
        var bitArray = ToBitBool(b);
        return bitArray[position];
    }

    public static bool[] ToBitBool(byte newByte)
    {
        // get the value of hte byte
        var myNum = Convert.ToInt32(newByte);

        // make a bool array for the bits.
        // 0 == falseu
        // 1 == true; duh
        var myByte = new bool[8];
        for (var i = 0; i < 8; i++)
        {
            // ANDing against each bit to set the bool value
            if ((myNum & (1 << (7 - i))) != 0)
            {
                myByte[i] = true;
            }
        }

        return myByte;
    }

    public static char[] ToBitChar(byte newByte)
    {
        var myChar = new char[8];

        var myBool = ToBitBool(newByte);
        for (var i = 0; i < 8; i++)
        {
            if (myBool[i])
            {
                myChar[i] = Convert.ToChar("1");
            }
            else
            {
                myChar[i] = Convert.ToChar("0");
            }
        }

        return myChar;
    }

    public static char[] ToBitChars(byte[] myBytes)
    {
        var myChars = new char[myBytes.Length * 8];
        var i = 0;
        foreach (var b in myBytes)
        {
            var bit = ToBitBool(b);
            for (var j = 0; j < 8; j++)
            {
                if (bit[j])
                {
                    myChars[i++] = Convert.ToChar("1");
                }
                else
                {
                    myChars[i++] = Convert.ToChar("0");
                }
            }
        }

        return myChars;
    }

    public static bool[] ToBitBools(byte[] myBytes)
    {
        var myBools = new bool[myBytes.Length * 8];
        var i = 0;
        foreach (var b in myBytes)
        {
            var bit = ToBitBool(b);
            for (var j = 0; j < 8; j++)
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

    public static byte ToByteChar(char[] myChar)
    {
        var myInt = 0;
        for (var i = 0; i < 8; i++)
        {
            if (myChar[i].Equals("1"))
            {
                myInt += (int)Math.Pow(2, 7 - i);
            }
        }

        return Convert.ToByte(myInt);
    }

    public static byte ToByteBool(bool[] myBool)
    {
        var myInt = 0;
        for (var i = 0; i < 8; i++)
        {
            if (myBool[i])
            {
                myInt += (int)Math.Pow(2, 7 - i);
            }
        }

        return Convert.ToByte(myInt);
    }


    public BitReader()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}
