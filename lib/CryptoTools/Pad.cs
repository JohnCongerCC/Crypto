using System;
using System.Text;

namespace CryptoTools
{
    public static class Pad
    {
        public static string PadHex(int length, string Hex)
        {
            var SB = new StringBuilder();
            for (int i = 0; i < length/2; i++)
            {
                SB.Append(Hex);
            }
            return SB.ToString();
        }

        public static string PadKey(string HexKey, int size)
        {
            var SB = new StringBuilder();
            for (int i = 0; i < size/2; i++)
            {
                string Hex = GetNextHex(HexKey, i);
                SB.Append(Hex);
            }
            return SB.ToString();
        }

        //https://github.com/bitbeans/helper-net/blob/master/helper-net/PaddingHelper.cs
        //https://en.wikipedia.org/wiki/Padding_(cryptography)#PKCS%235_and_PKCS%237
        public static byte[] AddPkcs7(byte[] data, int paddingLength)
        {
            if (data.Length > 256)
                throw new ArgumentOutOfRangeException("data", "data must be <= 256 in length");
            
            if (paddingLength > 256)
                throw new ArgumentOutOfRangeException("paddingLength", "paddingLength must be <= 256");

            if (paddingLength <= data.Length)
                return data;

            var output = new byte[paddingLength];
            Buffer.BlockCopy(data, 0, output, 0, data.Length);
            for (var i = data.Length; i < output.Length; i++)
            {
                output[i] = (byte) (paddingLength - data.Length);
            }
            return output;
        }

        public static byte[] RemovePkcs7(byte[] paddedByteArray)
        {
            if (paddedByteArray == null)
            {
                throw new ArgumentNullException("paddedByteArray", "paddedByteArray can not be null");
            }

            var last = paddedByteArray[paddedByteArray.Length - 1];
            if (paddedByteArray.Length <= last)
            {
                // there is no padding
                return paddedByteArray;
            }

            return ArrayHelper.SubArray(paddedByteArray, 0, (paddedByteArray.Length - last));
        }

        static string GetNextHex(string HexKey, int i)
        {
            int HexSize = HexKey.Length / 2;
            int Index = i % HexSize;
            return HexKey.Substring(Index * 2, 2);
        }
    }
}