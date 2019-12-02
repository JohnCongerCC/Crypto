using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTools
{
    public static class Stats
    {
        public static int GetHammingDistance(string Hex1, string Hex2)
        {
            var bin1 = MyConvert.HexToBinary(Hex1);
            var bin2 = MyConvert.HexToBinary(Hex2);
            if (bin1.Length != bin2.Length) throw new IndexOutOfRangeException("hex strings need to be the same length");
            int Diff = 0;
            for (int i = 0; i < bin1.Length; i++)
            {
                if(bin1[i] != bin2[i])
                    Diff++;
            }
            return Diff;
        }

        public static double GetEnglishScore(string Message)
        {
            double score = 0;
            var character_frequencies = GetEngCharFreq();
            var character_counts = Message.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            foreach (var CharCount in character_counts)
            {
                double Freq = 0;
                if(character_frequencies.TryGetValue(CharCount.Key, out Freq))
                    score = score + (Freq * (double)CharCount.Value);
            }
            return score;
        }

        public static Dictionary<char, double> GetEngCharFreq()
        {
            return new Dictionary<char,double>
            { 
                {'a', .08167},
                {'b', .01492},
                {'c', .02782}, 
                {'d', .04253},
                {'e', .12702}, 
                {'f', .02228}, 
                {'g', .02015}, 
                {'h', .06094},
                {'i', .06094}, 
                {'j', .00153}, 
                {'k', .00772}, 
                {'l', .04025},
                {'m', .02406}, 
                {'n', .06749}, 
                {'o', .07507}, 
                {'p', .01929},
                {'q', .00095}, 
                {'r', .05987}, 
                {'s', .06327}, 
                {'t', .09056},
                {'u', .02758}, 
                {'v', .00978}, 
                {'w', .02360}, 
                {'x', .00150},
                {'y', .01974}, 
                {'z', .00074}, 
                {' ', .13000}
            };
        }

    }
}