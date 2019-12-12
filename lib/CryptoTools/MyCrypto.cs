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

        public static byte[] AES_CBC_Decrypt(byte[] data, byte[] key, byte[] IV, int blocksize)
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

        public static byte[] AES_CBC_Encrypt(byte[] data, byte[] key, byte[] IV, int blocksize)
        {
             var SB = new StringBuilder();
            var Chunks = Util.ChunkBy<byte>(data.ToList(), blocksize);

            //Initialize ORHex to the IV (for the first iteration of the loop)
            //See CBC in: https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation
            var ORHex = MyConvert.BytesToHex(IV);

            foreach (var chunk in Chunks)
            {
                var PlainBytes = chunk.ToArray();
                var PlainHex = MyConvert.BytesToHex(PlainBytes);

                //First XOR the bytes
                var OredHex = FixedXOR(PlainHex, ORHex);
                var OredBytes = MyConvert.HexToByteArray(OredHex);

                //Next Encrypt with ECB the Orded Bytes
                var EncryptedBytes = AES_ECB_Encrypt(OredBytes, key);
                var EncryptedHex = MyConvert.BytesToHex(EncryptedBytes);

                //Append Block to the larger result
                SB.Append(EncryptedHex);

                //Next ORHex will be the previous blocks encrypted Hex (used in the next iteration of the loop)
                ORHex = EncryptedHex;
            }
            var EncryptedHexString = SB.ToString();
            return MyConvert.HexToByteArray(EncryptedHexString);
        }

        public static byte[] GenerateRandomKey()
        {
            var result = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                Random rnd = new Random();
                int BYTE   = rnd.Next(255);
                result.Add((byte)BYTE);
            }
            return result.ToArray();
        }

        public static byte[] AddBytes(string PlainText)
        {
            string HexPlain = MyConvert.HexEncodePlainText(PlainText);
            string HexWithExtra = AddRandomHex(HexPlain);
            var bytes = MyConvert.HexToByteArray(HexWithExtra);
            return bytes;
        }

        public static string AddRandomHex(string baseHex)
        {
            var SB = new StringBuilder();
            Random rnd = new Random();
            int before = rnd.Next(5, 11);  // creates a number between 5 and 10
            int after = rnd.Next(5, 11); 

            for (int i = 0; i <= before; i++)
                SB.Append(GetRandomhex());
            
            SB.Append(baseHex);

            for (int i = 0; i <= after; i++)
                SB.Append(GetRandomhex());

            return SB.ToString();
        }

        public static string GetRandomhex()
        {
            Random rnd = new Random();
            int BYTE   = rnd.Next(255);
            return MyConvert.IntToHex(BYTE);
        }

        public static RandomEncrypt RandomlyEncrypt(string PlainText)
        {
            int BLOCKSIZE = 16;
            var IVBytes = GenerateRandomKey(); //"just use random IVs each time..."

            var KeyBytes = GenerateRandomKey();
            var PlainTextBytesWithExtra = AddBytes(PlainText);

            var PaddedBytes = Pad.AddPkcs7(PlainTextBytesWithExtra, 128);

             Random rnd = new Random();
            int SWITCH   = rnd.Next(2);
            if (SWITCH == 0)
                return new RandomEncrypt { EType = AESEncryptionType.ECB, EncryptedBytes = AES_ECB_Encrypt(PaddedBytes, KeyBytes) };
            else
                return new RandomEncrypt { EType = AESEncryptionType.CBC, EncryptedBytes = AES_CBC_Encrypt(PaddedBytes, KeyBytes, IVBytes, BLOCKSIZE)};
        }

        public static AESEncryptionType Encryption_Oracle(byte[] Cipher)
        {
            int BLOCKSIZE = 16;
            var Hex = MyConvert.BytesToHex(Cipher);
            var Chunks = Util.Split(Hex, BLOCKSIZE);
            var Duplicates = Chunks.GroupBy(x => x).Where(g => g.Count() > 1).Select(s => s.Key).ToList();
            if(Duplicates.Count > 0)
                return AESEncryptionType.ECB;
            else
                return AESEncryptionType.CBC;
        }

    }
}