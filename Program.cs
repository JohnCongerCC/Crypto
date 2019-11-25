using System;
using System.Linq;
using System.Text;
namespace crypto
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://cryptopals.com/sets/1/challenges/1
            string hexstring = "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
            string Base64String = ConvertToBase64(hexstring);
            string ExpectedB64 = "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";
            Console.WriteLine(ExpectedB64 == Base64String);

            //https://cryptopals.com/sets/1/challenges/2
            string hexstring1  = "1c0111001f010100061a024b53535009181c";
            string hexstringOR = "686974207468652062756c6c277320657965";
            string ResultOR = FixedOR(hexstring1, hexstringOR);
            string ExpectedHex1 = "746865206b696420646f6e277420706c6179";
            Console.WriteLine(ResultOR == ExpectedHex1);

            //https://cryptopals.com/sets/1/challenges/3
            string HexCipher = "1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b3736";
            SingleByteXORCipher(HexCipher);
        }

        static void SingleByteXORCipher(string hexcipher)
        {
            for (int i = 0; i < 255; i++)
            {
                string HEX = i.ToString("X").PadLeft(2,'0');
                string FullHEX = FillFullHex(hexcipher.Length, HEX);
                var Result = FixedOR(hexcipher, FullHEX);
                Console.WriteLine(i);
                Console.WriteLine(ConvertHexStringToAscii(Result));
            }
        }

        static string ConvertHexStringToAscii(String hexString)
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

        static string FillFullHex(int length, string Hex)
        {
            var SB = new StringBuilder();
            for (int i = 0; i < length/2; i++)
            {
                SB.Append(Hex);
            }
            return SB.ToString();
        }
        static string FixedOR(string hex1, string hex2)
        {
            var SB = new StringBuilder();
            var bin1 = ConvertHexToBinary(hex1).ToCharArray();
            var bin2 = ConvertHexToBinary(hex2).ToCharArray();
            if (bin1.Length != bin2.Length) throw new IndexOutOfRangeException("hex strings need to be the same length");
            for (int i = 0; i < bin1.Length; i++)
            {
                bool XOR = Convert.ToBoolean(Convert.ToInt32(bin1[i].ToString())) ^ Convert.ToBoolean(Convert.ToInt32(bin2[i].ToString()));
                SB.Append(Convert.ToInt32(XOR).ToString());
            }
            string binarystring = SB.ToString();
            return(BinaryStringToHexString(binarystring));
        }
        static string ConvertToBase64(string HexString)
        {
            var base64String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            var base64Array = base64String.ToCharArray();
            var SB = new StringBuilder();
            string binarystring = ConvertHexToBinary(HexString);
            int six = 6;
            var Sextets = Enumerable.Range(0, binarystring.Length / six).Select(i => binarystring.Substring(i * six, six));
            foreach (var binary in Sextets)
            {
                int Decimal = Convert.ToInt32(binary, 2);
                SB.Append(base64Array[Decimal]);
            }
            return(SB.ToString());
        }

        static string ConvertHexToBinary(string HexString)
        {
            return String.Join(String.Empty,  HexString
                .Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
                )
            );
        }

        public static string BinaryStringToHexString(string binary)
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
    }
}
