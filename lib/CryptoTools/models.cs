using System;

namespace CryptoTools
{
    public class MessageScore
    {
        public double Score { get; set; }
        public string Hex { get; set; }
        public string Message {get; set;}
    }
    public class MessageIndex
    {
        public int Index { get; set; }
        public string Message { get; set; }
    }
}