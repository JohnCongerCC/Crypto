﻿using System;
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
        public void FixedXOR_Test()
        {
            //https://cryptopals.com/sets/1/challenges/2
                    
            string hexstring1  = "1c0111001f010100061a024b53535009181c";
            string hexstringOR = "686974207468652062756c6c277320657965";
            string ResultOR = XOR.FixedXOR(hexstring1, hexstringOR);
            string ExpectedHex1 = "746865206b696420646f6e277420706c6179";
            Assert.IsTrue(ResultOR == ExpectedHex1);
        }

        [Test]
        public void SingleByteXORCipher_Test()
        {
            //https://cryptopals.com/sets/1/challenges/3
            string HexCipher = "1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b3736";
            string HexOfChar = XOR.SingleByteXORCipher(HexCipher).HexCipher;
            string ExpectedHexOfChar = "58";
            Assert.IsTrue(HexOfChar == ExpectedHexOfChar);

        }

        [Test]
        public void DetectSingleCharacterXOR_Test()
        {
            //https://cryptopals.com/sets/1/challenges/4
            string[] lines = File.ReadAllLines(@"./4.txt", Encoding.UTF8);
            var messageIndex = XOR.DetectSingleCharacterXOR(lines);
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
            string CipherText = XOR.RepeatingXOR(PlainText1+'\n'+PlainText2, key);
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
            string str = GetFile6();
            XOR.BreakRepeatingKeyXOR(str);
        }

        static string GetFile6()
        {
            string[] lines = File.ReadAllLines(@"./6.txt", Encoding.UTF8);
            var SB = new StringBuilder();
            foreach (var line in lines)
                SB.Append(line);
            return SB.ToString();
        }
    }
}