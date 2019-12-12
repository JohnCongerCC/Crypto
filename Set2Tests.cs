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
    class Set2Tests
    {
        [Test]
        public void PKCS7_Test()
        {   //https://en.wikipedia.org/wiki/Padding_(cryptography)#PKCS%235_and_PKCS%237
            //https://cryptopals.com/sets/2/challenges/9
            string Text = "YELLOW SUBMARINE";
            var bytes = MyConvert.TextToByteArray(Text);
            var result = Pad.AddPkcs7(bytes, 20);
            for (int i = 16; i < 20; i++)
            {
                Assert.AreEqual(result[i], 4); //four extra bytes, so fill with Hex 04
            }
        }

        [Test]
        public void AESinECB_Encrypt_Decrypt_Test()  //ECB = Electronic Codebook
        {
            string OriginalMessage = "Now is the time for all good men to come to the aid of their country";  
            var bytes = MyConvert.TextToByteArray(OriginalMessage); 
            var PaddedBytes = Pad.AddPkcs7(bytes, 128); //Pad the message to 128 bytes

            //The Key (16 bytes)
            string key = "YELLOW SUBMARINE";
            string HexKey = MyConvert.HexEncodePlainText(key);
            var Keybytes = MyConvert.HexToByteArray(HexKey);

            //Encrypt
            var CiperBytes = MyCrypto.AES_ECB_Encrypt(PaddedBytes, Keybytes);
          
            //Decrypt
            var PlainBytes = MyCrypto.AES_ECB_Decrypt(CiperBytes, Keybytes);
            var UnPaddedBytes = Pad.RemovePkcs7(PlainBytes); //Unpad the message
            var hexPlain = MyConvert.BytesToHex(UnPaddedBytes);
            var PlainText = MyConvert.HexToAscii(hexPlain);

            Assert.IsTrue(OriginalMessage == PlainText);
        }

        [Test]
        public void AESinCBC_Encrypt_Decrypt_Test()  //CBC = Cipher Block Chaining 
        {
            int BLOCKSIZE = 16;

            string OriginalMessage = "Now I rock a house party at the drop of a hat\nAnd I beat a body down with an aluminum bat";
            var bytes = MyConvert.TextToByteArray(OriginalMessage); 
            var PaddedBytes = Pad.AddPkcs7(bytes, 128); //Pad the message to 128 bytes

            //The Key (16 bytes)
            string key = "YELLOW SUBMARINE";
            string HexKey = MyConvert.HexEncodePlainText(key);
            var Keybytes = MyConvert.HexToByteArray(HexKey);

            //Initialization Vector (padded to BLOCKSIZE bytes)
            string IVHex = Pad.PadHex(BLOCKSIZE * 2, "00");
            var IVBytes = MyConvert.HexToByteArray(IVHex);

            //Encrypt
            var CiperBytes = MyCrypto.AES_CBC_Encrypt(PaddedBytes, Keybytes, IVBytes, BLOCKSIZE);
          
            //Decrypt
            var PlainBytes = MyCrypto.AES_CBC_Decrypt(CiperBytes, Keybytes, IVBytes, BLOCKSIZE);
            var UnPaddedBytes = Pad.RemovePkcs7(PlainBytes); //Unpad the message
            var hexPlain = MyConvert.BytesToHex(UnPaddedBytes);
            var PlainText = MyConvert.HexToAscii(hexPlain);

            Assert.IsTrue(OriginalMessage == PlainText);
        }

        [Test]
        public void Implement_CBC_mode_Test()  //CBC = Cipher Block Chaining 
        {
            var BLOCKSIZE = 16;

            //https://cryptopals.com/sets/2/challenges/10
            string str = Util.GetFile(10);  
            string Hex = MyConvert.Base64ToHex(str); 
            var bytes = MyConvert.HexToByteArray(Hex);

            //Initialization Vector (padded to BLOCKSIZE bytes)
            string IVHex = Pad.PadHex(BLOCKSIZE * 2, "00");
            var IVBytes = MyConvert.HexToByteArray(IVHex);

            //The Key 
            string key = "YELLOW SUBMARINE";
            string HexKey = MyConvert.HexEncodePlainText(key);
            var Keybytes = MyConvert.HexToByteArray(HexKey);

            //Perform my version of CBC
            var decryptedBytes = MyCrypto.AES_CBC_Decrypt(bytes, Keybytes, IVBytes, BLOCKSIZE);
            var HexResult = MyConvert.BytesToHex(decryptedBytes);
            var Plain = MyConvert.HexToAscii(HexResult);
            Assert.IsTrue("I'm back and I'm ringin' " == Plain.Substring(0,25));  //25 is at least two blocks (of16) so i know i have the algo correct
        }

        [Test]
        public void GetRandomKey_Test()  
        {
            var key = MyCrypto.GenerateRandomKey();
            Assert.IsTrue(key.Length == 16);
        }

        [Test]
        public void AES_ECB_and_CBC_Oracle_Test()  
        {
            string myInput = "I looked out the window and seen his bald head\nI ran to the fridge and pulled out an egg";
            MyCrypto.EncryptionOracle(myInput);
        }
    }
}