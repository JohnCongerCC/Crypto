using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CryptoTools;

namespace crypto
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://cryptopals.com/sets/1/challenges/1
            string hexstring = "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
            string Base64String = MyConvert.HexToBase64(hexstring);
            string ExpectedB64 = "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";
            Console.WriteLine(ExpectedB64 == Base64String);

            //Used for Challenge6
            string b64 = MyConvert.Base64ToHex(ExpectedB64);
            Console.WriteLine(b64 == hexstring);

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
            var TestHex = MyConvert.HexEncodePlainText(test);
            var WokaHex = MyConvert.HexEncodePlainText(woka);
            int HamDist = Stats.GetHammingDistance(TestHex, WokaHex);
            Console.WriteLine(HamDist == 37);

            BreakRepeatingKeyXOR();
            
        }

        static string BreakRepeatingKeyXOR()
        {
            string str = GetFile6();
            

            //Step 3
            var ScoreList = new Dictionary<int,double>();
            var bytes = Convert.FromBase64String(str).ToList();
            for (int KEYSIZE = 2; KEYSIZE < 40; KEYSIZE++)
            {
                var Chunk1 = bytes.Take(KEYSIZE);
                var Chunk2 = bytes.Skip(KEYSIZE).Take(KEYSIZE);
                var Hex1 = MyConvert.BytesToHex(Chunk1);
                var Hex2 = MyConvert.BytesToHex(Chunk2);
                int HamDist2 = Stats.GetHammingDistance(Hex1, Hex2);
                var NormalizedHamDist = (double)HamDist2 / (double)KEYSIZE;    
                ScoreList.Add(KEYSIZE, NormalizedHamDist);
            }

            //Step 4
            var keyOfMinValue = ScoreList.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;

            //Step 5
            var Chunks = Util.ChunkBy<byte>(bytes, keyOfMinValue);

            //Step 6
            var TransposedBlocks = Util.Transpose(Chunks);

            var SB = new StringBuilder();
            foreach (var block in TransposedBlocks)
            {
                var TransHex = MyConvert.BytesToHex(block);
                var temp = SingleByteXORCipher(TransHex);
                var temp2 = MyConvert.HexToAscii(temp.HexCipher);
                SB.Append(temp2);
            }
            var RepeatingKeyXOR = SB.ToString();
            var p =09;
            return RepeatingKeyXOR;
        }

  


        

        static string GetFile6()
        {
            string[] lines = File.ReadAllLines(@"./6.txt", Encoding.UTF8);
            var SB = new StringBuilder();
            foreach (var line in lines)
                SB.Append(line);
            return SB.ToString();
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
                var Message = SingleByteXORCipher(hexcipher, i);
                var score = Stats.GetEnglishScore(Message);
                ScoreList.Add(i,score);
                
            }
            var keyOfMaxValue = ScoreList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            return new CipherScore
                                {
                                    HexCipher = MyConvert.IntToHex(keyOfMaxValue), 
                                    Score = ScoreList.GetValueOrDefault(keyOfMaxValue), 
                                    Message = SingleByteXORCipher(hexcipher, keyOfMaxValue) 
                                };
        }
        static string SingleByteXORCipher(string hexcipher, int i)
        {
            string HEX = MyConvert.IntToHex(i);
            string FullHEX = Pad.FillFullHex(hexcipher.Length, HEX);
            var Result = FixedOR(hexcipher, FullHEX);
                
            return MyConvert.HexToAscii(Result);
        }
        

        
        static string RepeatingOR(string PlainText, string key)
        {
            var Hex1 = MyConvert.HexEncodePlainText(PlainText);
            var Hex2 = Pad.KeyToSize(MyConvert.HexEncodePlainText(key), Hex1.Length);
            return FixedOR(Hex1, Hex2);
        }

        

        
        static string FixedOR(string hex1, string hex2)
        {
            var SB = new StringBuilder();
            var bin1 = MyConvert.HexToBinary(hex1).ToCharArray();
            var bin2 = MyConvert.HexToBinary(hex2).ToCharArray();
            if (bin1.Length != bin2.Length) throw new IndexOutOfRangeException("hex strings need to be the same length");
            for (int i = 0; i < bin1.Length; i++)
            {
                bool XOR = Convert.ToBoolean(Convert.ToInt32(bin1[i].ToString())) ^ Convert.ToBoolean(Convert.ToInt32(bin2[i].ToString()));
                SB.Append(Convert.ToInt32(XOR).ToString());
            }
            string binarystring = SB.ToString();
            return(MyConvert.BinaryToHex(binarystring));
        }
        
        
    }
}
