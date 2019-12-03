using System;

namespace CryptoTools
{
    public class CipherScore
    {
        public double Score { get; set; }
        public string HexCipher { get; set; }
        public string Message {get; set;}
    }
    public class MessageIndex
    {
        public int Index { get; set; }
        public string Message { get; set; }
    }
}