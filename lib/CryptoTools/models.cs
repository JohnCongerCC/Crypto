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

    public class RandomEncrypt
    {
        public AESEncryptionType EType { get; set; }
        public byte[] EncryptedBytes { get; set; }
    }

    public class EncryptECB
    {
        
        public byte[] EncryptedBytes { get; set; }
    }

    public enum AESEncryptionType
    {
        ECB = 0, CBC = 1
    }
}