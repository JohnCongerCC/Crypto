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
                var result = SingleByteXOR(line);
                ScoreList.Add(i,result.Score);
                i++;
                
            }

            var keyOfMaxValue = ScoreList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            
            string ValidHexFromFile = lines[keyOfMaxValue];
            var Score = SingleByteXOR(ValidHexFromFile);
            var FillHex = Pad.FillFullHex(ValidHexFromFile.Length, Score.Hex);
            var Result = FixedXOR(ValidHexFromFile, FillHex);
            var message = MyConvert.HexToAscii(Result);
            
            return new MessageIndex{ Index= keyOfMaxValue, Message = message };
        }
        public static MessageScore SingleByteXOR(string HEX)
        {
            var ScoreList = new Dictionary<int,double>();
            for (int i = 0; i < 255; i++)
            {
                var Message = SingleByteXOR(HEX, i);
                var score = Stats.GetEnglishScore(Message);
                ScoreList.Add(i,score);
                
            }
            
            int keyOfMaxValue = ScoreList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            double MaxScore = 0;
            if(ScoreList.TryGetValue(keyOfMaxValue, out MaxScore))
            {
                return new MessageScore
                {
                    Hex = MyConvert.IntToHex(keyOfMaxValue), 
                    Score = MaxScore,
                    Message = SingleByteXOR(HEX, keyOfMaxValue) 
                };
            }
            else return null;
        }
        static string SingleByteXOR(string Hex, int i)
        {
            string HEX = MyConvert.IntToHex(i);
            string FullHEX = Pad.FillFullHex(Hex.Length, HEX);
            var Result = FixedXOR(Hex, FullHEX);
                
            return MyConvert.HexToAscii(Result);
        }
        
        public static string RepeatingXOR(string PlainText, string key)
        {
            var Hex1 = MyConvert.HexEncodePlainText(PlainText);
            var Hex2 = Pad.KeyToSize(MyConvert.HexEncodePlainText(key), Hex1.Length);
            return FixedXOR(Hex1, Hex2);
        }
        
        public static string FixedXOR(string Text, string Key)
        {
            var SB = new StringBuilder();
            var bin1 = MyConvert.HexToBinary(Text).ToCharArray();
            var bin2 = MyConvert.HexToBinary(Key).ToCharArray();
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
            var ScoreList = Stats.GetHammingDistances(bytes, 2, 40);
            
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
                var score = SingleByteXOR(TransHex);
                var Hex = MyConvert.HexToAscii(score.Hex);
                SB.Append(Hex);
            }
            var RepeatingKeyXOR = SB.ToString();
            
            return RepeatingKeyXOR;
        }
    }
}