using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTools
{
    public static class MyConvert
    {
        public static string BytesToHex(IEnumerable<byte> bytes)
        {
            return BitConverter.ToString(bytes.ToArray()).Replace("-", string.Empty);
        }

        public static string Base64ToHex(string Base64string)
        {
            var Base64AsMultipleOfFourChars = Base64string.PadRight(Base64string.Length + (4 - Base64string.Length % 4) % 4,'=');
            var bytes = Convert.FromBase64String(Base64AsMultipleOfFourChars);
            var SB = new StringBuilder();
            foreach (var b in bytes)
                SB.Append(b.ToString("X2"));
            
            return SB.ToString().ToLower();
        }
        
        public static string HexToBase64(string HexString)
        {
            var base64String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            var base64Array = base64String.ToCharArray();
            var SB = new StringBuilder();
            string binarystring = HexToBinary(HexString);
            int six = 6;
            var Sextets = Enumerable.Range(0, binarystring.Length / six).Select(i => binarystring.Substring(i * six, six));
            foreach (var binary in Sextets)
            {
                int Decimal = Convert.ToInt32(binary, 2);
                SB.Append(base64Array[Decimal]);
            }
            return(SB.ToString());
        }

        public static string HexToBinary(string HexString)
        {
            return String.Join(String.Empty,  HexString
                .Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
                )
            );
        }

        public static string IntToHex(int i)
        {
            return i.ToString("X").PadLeft(2,'0');
        }

        public static string BinaryToHex(string binary)
        {
            if (string.IsNullOrEmpty(binary))
                return binary;

            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            int mod4Len = binary.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binary = binary.PadLeft(((binary.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString().ToLower();
        }   

        public static byte[] StringToAscii(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public static string HexToAscii(String hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    String hs = string.Empty;

                    hs   = hexString.Substring(i,2);
                    uint decval =   System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;
                }
                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return string.Empty;
        }

        public static string HexEncodePlainText(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach(char c in input)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.ToString().Trim();
        }

        public static byte[] HexToByteArray(string hex) 
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }   
    }
}
