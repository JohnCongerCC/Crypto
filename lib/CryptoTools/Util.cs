using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CryptoTools
{
    public static class Util
    {
        public static List<List<byte>> Transpose(List<List<byte>> SourceBlocks)
        {
            return SourceBlocks
                .SelectMany(inner => inner.Select((item, index) => new { item, index }))
                .GroupBy(i => i.index, i => i.item)
                .Select(g => g.ToList())
                .ToList();
        }

        public static List<List<T>> ChunkBy<T>(List<T> source, int chunkSize) 
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
        public static string GetFile(int i)
        {
            string[] lines = File.ReadAllLines(@"./"+i+".txt", Encoding.UTF8);
            var SB = new StringBuilder();
            foreach (var line in lines)
                SB.Append(line);
            return SB.ToString();
        }

        public static string GenerateIdenticalString(char c, int x)
        {
            var SB = new StringBuilder();
            for (int i = 0; i < x; i++)
                SB.Append(c);
            return SB.ToString();
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
    }
}