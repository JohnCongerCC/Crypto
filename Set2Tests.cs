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
        {
            //https://cryptopals.com/sets/2/challenges/9
            string Text = "YELLOW SUBMARINE";
            var bytes = MyConvert.TextToByteArray(Text);
            var result = Pad.AddPkcs7(bytes, 20);
            for (int i = 16; i < 20; i++)
            {
                Assert.AreEqual(result[i], 4);
            }
        }

        [Test]
        public void Implement_CBC_mode_Test()
        {
            //https://cryptopals.com/sets/2/challenges/10
            string str = Util.GetFile(10);  
            string Hex = MyConvert.Base64ToHex(str); 
            var bytes = MyConvert.HexToByteArray(Hex);
        }
    }
}