using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CryptoTools
{
    public static class XOR
    {
        public static MessageIndex DetectSingleCharacterXOR(string[] lines)
        {   
            int i = 0;
            var ScoreList = new Dictionary<int,double>();

            foreach (var line in lines)
            {
                var result = SingleByteXORCipher(line);
                ScoreList.Add(i,result.Score);
                i++;
                
            }

            var keyOfMaxValue = ScoreList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            
            string ValidHexFromFile = lines[keyOfMaxValue];
            var Score = SingleByteXORCipher(ValidHexFromFile);
            var FillHex = Pad.FillFullHex(ValidHexFromFile.Length, Score.HexCipher);
            var Result = FixedXOR(ValidHexFromFile, FillHex);
            var message = MyConvert.HexToAscii(Result);
            
            return new MessageIndex{ Index= keyOfMaxValue, Message = message };
        }
        public static CipherScore SingleByteXORCipher(string hexcipher)
        {
            var ScoreList = new Dictionary<int,double>();
            for (int i = 0; i < 255; i++)
            {
                var Message = SingleByteXORCipher(hexcipher, i);
                var score = Stats.GetEnglishScore(Message);
                ScoreList.Add(i,score);
                
            }
            
            int keyOfMaxValue = ScoreList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            double MaxScore = 0;
            if(ScoreList.TryGetValue(keyOfMaxValue, out MaxScore))
            {
                return new CipherScore
                {
                    HexCipher = MyConvert.IntToHex(keyOfMaxValue), 
                    Score = MaxScore,
                    Message = SingleByteXORCipher(hexcipher, keyOfMaxValue) 
                };
            }
            else return null;
        }
        static string SingleByteXORCipher(string hexcipher, int i)
        {
            string HEX = MyConvert.IntToHex(i);
            string FullHEX = Pad.FillFullHex(hexcipher.Length, HEX);
            var Result = FixedXOR(hexcipher, FullHEX);
                
            return MyConvert.HexToAscii(Result);
        }
        
        public static string RepeatingXOR(string PlainText, string key)
        {
            var Hex1 = MyConvert.HexEncodePlainText(PlainText);
            var Hex2 = Pad.KeyToSize(MyConvert.HexEncodePlainText(key), Hex1.Length);
            return FixedXOR(Hex1, Hex2);
        }
        
        public static string FixedXOR(string hex1, string hex2)
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

        public static byte[] SingleByteXOR_String(string str, string key)
        {
            var Hex1 = MyConvert.HexEncodePlainText(str);
            var HexKey = MyConvert.HexEncodePlainText(key);
            var Hex2 = Pad.KeyToSize(HexKey, Hex1.Length);
            var HexResult = XOR.FixedXOR(Hex1, Hex2);
            var Result = MyConvert.HexToByteArray(HexResult);
            return Result;
        }

        public static string BreakRepeatingKeyXOR(string str)
        {
            //Step 3
            var bytes = Convert.FromBase64String(str).ToList();
            var ScoreList = GetHammingDistances(bytes, 2, 40);
            
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
            
            return RepeatingKeyXOR;
        }

        public static Dictionary<int, double> GetHammingDistances(List<byte> bytes, int start, int end)
        {
            var ScoreList = new Dictionary<int,double>();

            for (int KEYSIZE = start; KEYSIZE < end; KEYSIZE++)
            {
                var ListOfHamDist = new List<double>();
                for (int i = 0; i < bytes.Count(); i++)
                {
                    var SkipAmount = i * KEYSIZE;
                    if(SkipAmount + (KEYSIZE * 2) <= bytes.Count())
                    {
                        var Chunk1 = bytes.Skip(SkipAmount).Take(KEYSIZE);
                        var Chunk2 = bytes.Skip(SkipAmount + KEYSIZE).Take(KEYSIZE);
                        var Hex1 = MyConvert.BytesToHex(Chunk1);
                        var Hex2 = MyConvert.BytesToHex(Chunk2);
                        int HamDist = Stats.GetHammingDistance(Hex1, Hex2);
                        ListOfHamDist.Add((double)HamDist);
                    }
                }
                var AverageHamDist = ListOfHamDist.Average();
                var NormalizedHamDist = (double)AverageHamDist / (double)KEYSIZE;    
                ScoreList.Add(KEYSIZE, NormalizedHamDist);
            }
            return ScoreList;
        }
    }
}