using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CryptoTools;
using NUnit.Framework;

namespace crypto
{
    [TestFixture]
    class CryptoTests
    {
        [Test]
        public void HexToBase64_Test()
        {
            //https://cryptopals.com/sets/1/challenges/1
            string hexstring = "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
            string Base64 = "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";
            string Result = MyConvert.HexToBase64(hexstring);
            
            Assert.IsTrue(Base64 == Result);
        }

        [Test]
        public void Base64ToHex_Test()
        {
            string hexstring = "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
            string Base64 = "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";
            string Result = MyConvert.Base64ToHex(Base64);

            Assert.IsTrue(hexstring == Result);
        }

        [Test]
        public void FixedXOR_Test1()
        {
            //https://cryptopals.com/sets/1/challenges/2
                    
            string Encrypted  = "1c0111001f010100061a024b53535009181c";  //Encrypted Text
            string HexEncodedKey = "686974207468652062756c6c277320657965";  //hit the bull's eye
            string ResultOR = MyCrypto.FixedXOR(Encrypted, HexEncodedKey);
            string Decrypted = "746865206b696420646f6e277420706c6179";  //the kid don't play
            Assert.IsTrue(ResultOR == Decrypted);
        }


        [Test]
        public void SingleByteXORCipher_Test()
        {
            //https://cryptopals.com/sets/1/challenges/3
            string HexCipher = "1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b3736";
            string HexOfChar = MyCrypto.SingleByteXOR(HexCipher).Hex;
            string ExpectedHexOfChar = "58";
            Assert.IsTrue(HexOfChar == ExpectedHexOfChar);

        }

        [Test]
        public void DetectSingleCharacterXOR_Test()
        {
            //https://cryptopals.com/sets/1/challenges/4
            string[] lines = File.ReadAllLines(@"./4.txt", Encoding.UTF8);
            var messageIndex = MyCrypto.DetectSingleCharacterXOR(lines);
            var ExpectedIndex = 170;
            var ExpectedMsg = "Now that the party is jumping\n";
            Assert.IsTrue(messageIndex.Index == ExpectedIndex);
            Assert.IsTrue(messageIndex.Message == ExpectedMsg);
        }

        [Test]
        public void RepeatingXOR_Test()
        {
            //https://cryptopals.com/sets/1/challenges/5
            string PlainText1 = "Burning 'em, if you ain't quick and nimble";
            string PlainText2 = "I go crazy when I hear a cymbal";
            string key = "ICE";
            string CipherText = MyCrypto.RepeatingXOR(PlainText1+'\n'+PlainText2, key);
            string ExpectedCipher = "0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a26226324272765272a282b2f20430a652e2c652a3124333a653e2b2027630c692b20283165286326302e27282f";
            Assert.IsTrue(CipherText == ExpectedCipher);
        }

        [Test]
        public void GetHammingDistance_Test()
        {
            //https://cryptopals.com/sets/1/challenges/6
            string test = "this is a test";
            string woka = "wokka wokka!!!";
            var TestHex = MyConvert.HexEncodePlainText(test);
            var WokaHex = MyConvert.HexEncodePlainText(woka);
            int HamDist = Stats.GetHammingDistance(TestHex, WokaHex);
            Assert.IsTrue(HamDist == 37);
        }

        [Test]
        public void BreakRepeatingKeyXOR_Test()
        {
            //https://cryptopals.com/sets/1/challenges/6
            string str = GetFile(6);
            var Expected = "Terminator X: Bring the noise";
            var result = MyCrypto.BreakRepeatingKeyXOR(str);
            Assert.IsTrue(Expected == result);
        }

        

        [Test]
        public void FixedXOR_Test2()
        {
            //https://cryptopals.com/sets/1/challenges/6
            string str = GetFile(6);
            string HextoDecrypt = MyConvert.Base64ToHex(str);
            var Key = "Terminator X: Bring the noise";
            var HexKey = Pad.KeyToSize(MyConvert.HexEncodePlainText(Key), HextoDecrypt.Length);
            var DecryptedHex = MyCrypto.FixedXOR(HextoDecrypt, HexKey);
            var Plain = MyConvert.HexToAscii(DecryptedHex);
            Assert.IsTrue("I'm back and I'm ringin' " == Plain.Substring(0,25));
        }

        [Test]
        public void AESinECB_Test()
        {
            //https://cryptopals.com/sets/1/challenges/7
            string str = GetFile(7);  
            string Hex = MyConvert.Base64ToHex(str); 
            var bytes = MyConvert.HexToByteArray(Hex);

            string key = "YELLOW SUBMARINE";
            string HexKey = MyConvert.HexEncodePlainText(key);
            var Keybytes = MyConvert.HexToByteArray(HexKey);

            var result = MyCrypto.AESDecrypt(bytes, Keybytes);
            var HexResult = MyConvert.BytesToHex(result);
            var Plain = MyConvert.HexToAscii(HexResult);
            Assert.IsTrue("I'm back and I'm ringin' " == Plain.Substring(0,25));
        }
        
        [Test]
        public void DetectAES_ECB_Test()
        {
            //https://cryptopals.com/sets/1/challenges/8
            string str = GetFile(8); 
            var Chunks = Util.Split(str, 32);
            var Duplicates = Chunks.GroupBy(x => x).Where(g => g.Count() > 1).Select(s => s.Key).ToList();
           
            var bytes = MyConvert.HexToByteArray(Duplicates.First());
        }

        [Test]
        public void FixedOR_String_Short_Test()
        {
            //https://trustedsignal.blogspot.com/2015/06/xord-play-normalized-hamming-distance.html
            string str = "fuse fuel";
            string key = "few";
            var ExpectedResult = new byte[] { 000, 016, 004, 003,069,017,019,000,027 };

            var Result = MyCrypto.SingleByteXOR_String(str, key);
            
            for (int i = 0; i < Result.Count(); i++)
            {
                Assert.AreEqual(Result[i], ExpectedResult[i]);
            }
        }

        [Test]
        public void FixedOR_String_Long_Test()
        {
            //https://trustedsignal.blogspot.com/2015/06/xord-play-normalized-hamming-distance.html
            string str = "fuse fuel for falling flocks";
            string key = "few";
            var ExpectedResult = new byte[] { 000,016,004,003,069,017,019,000,027,070,003,024,020,069,017,007,009,027,015,011,016,070,003,027,009,006,028,021 };

            var Result = MyCrypto.SingleByteXOR_String(str, key);
            
            for (int i = 0; i < Result.Count(); i++)
            {
                Assert.AreEqual(Result[i], ExpectedResult[i]);
            }
        }

        [Test]
        public void HammingDistance_Test()
        {
            //https://trustedsignal.blogspot.com/2015/06/xord-play-normalized-hamming-distance.html
            string str = "fuse fuel for falling flocks";
            string key = "few";
            var ExpectedResults = new Dictionary<int, double> { 
                {2,2.88461538461538},
                {3,2.54166666666667},
                {4,3.08333333333333},
                {5,2.65},
                {6,2.44444444444444},
                {7,2.80952380952381},
                {8,2.8125},
                {9,2.22222222222222},
                {10,2.6},
                {11,2.18181818181818},
                {12,2.16666666666667},
                {13,2.61538461538462},
                {14,2.71428571428571}
            };

            var bytes = MyCrypto.SingleByteXOR_String(str, key);
            var ScoreList = Stats.GetHammingDistances(bytes.ToList(), 2, 14);
            for (int i = 2; i < 14; i++)
            {   //Assert within a rounded range b/c of doubles (close enough)
                Assert.That(Math.Abs(ScoreList[i] - ExpectedResults[i]), Is.LessThan(0.00001D));
            }
        }

        static string GetFile(int i)
        {
            string[] lines = File.ReadAllLines(@"./"+i+".txt", Encoding.UTF8);
            var SB = new StringBuilder();
            foreach (var line in lines)
                SB.Append(line);
            return SB.ToString();
        }

    }
}
