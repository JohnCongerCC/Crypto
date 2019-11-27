using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
            string HexOfChar = SingleByteXORCipher(HexCipher).HexCipher;
            string ExpectedHexOfChar = "58";
            Console.WriteLine(HexOfChar == ExpectedHexOfChar);

            //https://cryptopals.com/sets/1/challenges/4
            var IndexOfRecInFile = DetectSingleCharacterXOR();
            var ExpectedIndex = 170;
            Console.WriteLine(IndexOfRecInFile == ExpectedIndex);

            //https://cryptopals.com/sets/1/challenges/5
            string PlainText1 = "Burning 'em, if you ain't quick and nimble";
            string PlainText2 = "I go crazy when I hear a cymbal";
            string key = "ICE";
            string CipherText = RepeatingOR(PlainText1+'\n'+PlainText2, key);
            string ExpectedCipher = "0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a26226324272765272a282b2f20430a652e2c652a3124333a653e2b2027630c692b20283165286326302e27282f";
            Console.WriteLine(CipherText == ExpectedCipher);
            
            //https://cryptopals.com/sets/1/challenges/6
            string test = "this is a test";
            string woka = "wokka wokka!!!";
            int HamDist = GetHammingDistance(test, woka);
            Console.WriteLine(HamDist == 37);
        }

        static int GetHammingDistance(string str1, string str2)
        {
            var Hex1 = ConvertStringToHexAscii(str1);
            var Hex2 = ConvertStringToHexAscii(str2);
            var bin1 = ConvertHexToBinary(Hex1);
            var bin2 = ConvertHexToBinary(Hex2);
            if (bin1.Length != bin2.Length) throw new IndexOutOfRangeException("hex strings need to be the same length");
            int Diff = 0;
            for (int i = 0; i < bin1.Length; i++)
            {
                if(bin1[i] != bin2[i])
                    Diff++;
            }
            return Diff;
        }

        static int DetectSingleCharacterXOR()
        {   
            string[] lines = File.ReadAllLines(@"./4.txt", Encoding.UTF8);
            int i = 0;
            var ScoreList = new Dictionary<int,double>();
            foreach (var line in lines)
            {
                var result = SingleByteXORCipher(line);
                ScoreList.Add(i,result.Score);
                i++;
                
            }
            var keyOfMaxValue = ScoreList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

            //Debug info (to validate)
            // string ValidHexFromFile = lines[keyOfMaxValue];
            // var Score = SingleByteXORCipher(ValidHexFromFile);
            // var FillHex = FillFullHex(ValidHexFromFile.Length, Score.HexCipher);
            // var Result = FixedOR(ValidHexFromFile, FillHex);
            // var Message = ConvertHexStringToAscii(Result);
            // Console.WriteLine(Message);
            
            return keyOfMaxValue;
        }
        static CipherScore SingleByteXORCipher(string hexcipher)
        {
            var ScoreList = new Dictionary<int,double>();
            for (int i = 0; i < 255; i++)
            {
                string HEX = ConvertIntToHex(i);
                string FullHEX = FillFullHex(hexcipher.Length, HEX);
                var Result = FixedOR(hexcipher, FullHEX);
                
                var Message = ConvertHexStringToAscii(Result);
                var score = GetEnglishScore(Message);
                ScoreList.Add(i,score);
                
            }
            var keyOfMaxValue = ScoreList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            return new CipherScore{ HexCipher = ConvertIntToHex(keyOfMaxValue), Score = ScoreList.GetValueOrDefault(keyOfMaxValue) };
        }
        static double GetEnglishScore(string Message)
        {
            double score = 0;
            var character_frequencies = GetEngCharFreq();
            var character_counts = Message.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            foreach (var CharCount in character_counts)
            {
                double Freq = 0;
                if(character_frequencies.TryGetValue(CharCount.Key, out Freq))
                    score = score + (Freq * (double)CharCount.Value);
            }
            return score;
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
        static string RepeatingOR(string PlainText, string key)
        {
            var Hex1 = ConvertStringToHexAscii(PlainText);
            var Hex2 = PadKeyToSize(ConvertStringToHexAscii(key), Hex1.Length);
            return FixedOR(Hex1, Hex2);
        }

        static string PadKeyToSize(string HexKey, int size)
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

        static string ConvertIntToHex(int i)
        {
            return i.ToString("X").PadLeft(2,'0');
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

        public static string ConvertStringToHexAscii(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach(char c in input)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.ToString().Trim();
        }

        static Dictionary<char, double> GetEngCharFreq()
        {
            return new Dictionary<char,double>
            { 
                {'a', .08167},
                {'b', .01492},
                {'c', .02782}, 
                {'d', .04253},
                {'e', .12702}, 
                {'f', .02228}, 
                {'g', .02015}, 
                {'h', .06094},
                {'i', .06094}, 
                {'j', .00153}, 
                {'k', .00772}, 
                {'l', .04025},
                {'m', .02406}, 
                {'n', .06749}, 
                {'o', .07507}, 
                {'p', .01929},
                {'q', .00095}, 
                {'r', .05987}, 
                {'s', .06327}, 
                {'t', .09056},
                {'u', .02758}, 
                {'v', .00978}, 
                {'w', .02360}, 
                {'x', .00150},
                {'y', .01974}, 
                {'z', .00074}, 
                {' ', .13000}
            };
        }
    }
}
