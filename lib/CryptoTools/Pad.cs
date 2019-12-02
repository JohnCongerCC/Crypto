using System;
using System.Text;

namespace CryptoTools
{
    public static class Pad
    {
        public static string FillFullHex(int length, string Hex)
        {
            var SB = new StringBuilder();
            for (int i = 0; i < length/2; i++)
            {
                SB.Append(Hex);
            }
            return SB.ToString();
        }

        public static string KeyToSize(string HexKey, int size)
        {
            var SB = new StringBuilder();
            for (int i = 0; i < size/2; i++)
            {
                string Hex = GetNextHex(HexKey, i);
                SB.Append(Hex);
            }
            return SB.ToString();
        }

        static string GetNextHex(string HexKey, int i)
        {
            int HexSize = HexKey.Length / 2;
            int Index = i % HexSize;
            return HexKey.Substring(Index * 2, 2);
        }
    }
}