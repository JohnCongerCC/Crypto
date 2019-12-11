using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace CryptoTools
{
    public static class MyCrypto
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
            var FillHex = Pad.PadHex(ValidHexFromFile.Length, Score.Hex);
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
            string FullHEX = Pad.PadHex(Hex.Length, HEX);
            var Result = FixedXOR(Hex, FullHEX);
                
            return MyConvert.HexToAscii(Result);
        }
        
        public static string RepeatingXOR(string PlainText, string key)
        {
            var Hex1 = MyConvert.HexEncodePlainText(PlainText);
            var Hex2 = Pad.PadKey(MyConvert.HexEncodePlainText(key), Hex1.Length);
            return FixedXOR(Hex1, Hex2);
        }
        
        public static string FixedXOR(string Hex, string hexKey)
        {
            var SB = new StringBuilder();
            var bin1 = MyConvert.HexToBinary(Hex).ToCharArray();
            var bin2 = MyConvert.HexToBinary(hexKey).ToCharArray();
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
            var Hex2 = Pad.PadKey(HexKey, Hex1.Length);
            var HexResult = MyCrypto.FixedXOR(Hex1, Hex2);
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

        

        public static byte[] AES_ECB_Decrypt(byte[] data, byte[] key)
        {
            MemoryStream ms = new MemoryStream();
            RijndaelManaged AES = new RijndaelManaged();
            AES.Mode = CipherMode.ECB;
            AES.Key = key;
            AES.Padding = PaddingMode.None;
            
            CryptoStream cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();

            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        public static byte[] AES_ECB_Encrypt(byte[] data, byte[] key)
        {
            MemoryStream ms = new MemoryStream();
            RijndaelManaged AES = new RijndaelManaged();
            AES.Mode = CipherMode.ECB;
            AES.Key = key;
            AES.Padding = PaddingMode.None;
            
            CryptoStream cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();

            byte[] EncryptedData = ms.ToArray();
            return EncryptedData;
        }

        public static byte[] AES_CBC_Encrypt(byte[] data, byte[] key, byte[] IV, int blocksize)
        {
            var SB = new StringBuilder();
            var Chunks = Util.ChunkBy<byte>(data.ToList(), blocksize);

            //Initialize ORHex to the IV (for the first iteration of the loop)
            //See CBC in: https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation
            var ORHex = MyConvert.BytesToHex(IV);

            foreach (var chunk in Chunks)
            {
                var EncryptedBytes = chunk.ToArray();
                var decryptedBytes = AES_ECB_Decrypt(EncryptedBytes, key);
                
                var DecryptedHex = MyConvert.BytesToHex(decryptedBytes);
                var PlainHex = FixedXOR(DecryptedHex, ORHex);

                //Next ORHex will be the previous blocks encrypted Hex (used in the next iteration of the loop)
                ORHex = MyConvert.BytesToHex(EncryptedBytes); 
                SB.Append(PlainHex);

                //Just to see Ascii of decrypted text for debugging purposes
                var plaintext = MyConvert.HexToAscii(PlainHex);
            }
            
            var DecryptedHexString = SB.ToString();

            //Just to see Ascii of decrypted text for debugging purposes
            var c = MyConvert.HexToAscii(DecryptedHexString);

            return MyConvert.HexToByteArray(DecryptedHexString);
        }
    }
}