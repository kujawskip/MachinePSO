using LanguageProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator
{ 
    public class TestSets
    {
        private static Tuple<T1,T2> Inverse<T1, T2>(Tuple<T2, T1> t)
        {
            return new Tuple<T1, T2>(t.Item2, t.Item1);
        }

        private static string SeparatedArray<T>(T[] array, char separator)
        {
            if (array == null) return null;
            if (array.Length == 0) return "";
            var sb = new StringBuilder(array[0].ToString());
            for(int i=1; i<array.Length; i++)
            {
                sb.Append(separator);
                sb.Append(array[i]);
            }
            return sb.ToString();
        }

        private static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (a1 == null && a2 == null) return true;
            if (a1 == null || a2 == null) return false;
            if (a1.Length != a2.Length) return false;
            for (int i = 0; i < a1.Length; i++)
                if (!a1[i].Equals(a2[i])) return false;
            return true;
        }

        int LetterCount;
        public Dictionary<Tuple<int[], int[]>, bool> TestSet
        { get; private set; }
        public Dictionary<Tuple<int[], int[]>, bool> ControlSet
        { get; private set; }
        Random rand = new Random();

        public TestSets(Machine m, int thoroughCount = 5, int randomCount = 50, int controlCount = 500)
        {
            TestSet = new Dictionary<Tuple<int[], int[]>, bool>();
            ControlSet = new Dictionary<Tuple<int[], int[]>, bool>();
            LetterCount = m.LetterCount;
            var shortWords = GenerateAllWords(thoroughCount, m);
            var concats = GenerateRandomConcats(shortWords, randomCount + controlCount, m);
            GenerateSets(m, controlCount, randomCount, shortWords, concats);
        }

        private List<int[]> GenerateAllWords(int wordLength, Machine m)
        {
            var all = new List<int[]> { new int[0] };
            //Do słów długości thoroughCount
            for (int i = 1; i <= wordLength; i++)
            {
                var allCount = all.Count;
                //Dodanie j-tej litery
                for (int j = 0; j < LetterCount; j++)
                {
                    //na koniec k-tego słowa
                    for (int k = 0; k < allCount; k++)
                    {
                        var nArr = new int[all[k].Length + 1];
                        all[k].CopyTo(nArr, 0);
                        nArr[all[k].Length] = j;
                        all.Add(nArr);
                    }
                }
            }

            return all;
        }

        private List<int[]> GenerateRandomConcats(List<int[]> words, int randomCount, Machine m)
        {
            var res = new List<int[]>();
            for (int i=0; i<randomCount; i++)
            {
                var w = new int[0]; //Epsilon
                while (res.Contains(w)|| words.Contains(w))
                    w = words[rand.Next(words.Count)].Concat(words[rand.Next(words.Count)]).ToArray();
                res.Add(w);
            }
            return res;
        }

        private void GenerateSets(Machine m, int controlCount, int testCount, List<int[]> shortWords, List<int[]> randomWords)
        {
            var allWords = shortWords.Concat(randomWords).Select(x => m.alphabet.Translate(x.ToList())).ToArray();
            var testSetTemp = new Dictionary<Tuple<string, string>, bool>();
            for (int i=0; i<shortWords.Count; i++)
                for(int j=i+1; j<shortWords.Count; i++)
                {
                    var rel = m.AreWordsInRelation(shortWords[i], shortWords[j]);
                    var w1 = m.alphabet.Translate(shortWords[i].ToList());
                    var w2 = m.alphabet.Translate(shortWords[j].ToList());
                    testSetTemp.Add(new Tuple<string, string>(w1, w2), rel);
                    testSetTemp.Add(new Tuple<string, string>(w2, w1), rel);
                }
            for (int i = 0; i < testCount; i++)
            {
                var pair = new Tuple<string, string>(null, null);
                while( pair.Item1 == pair.Item2 || testSetTemp.Keys.Contains(pair))
                    pair = new Tuple<string, string>(allWords[rand.Next(allWords.Length)], allWords[rand.Next(allWords.Length)]);
                var rel = m.AreWordsInRelation(pair.Item1, pair.Item2);
                testSetTemp.Add(pair, rel);
                testSetTemp.Add(Inverse(pair), rel);
            }
            var controlSetTemp = new Dictionary<Tuple<string, string>, bool>();
            for (int i = 0; i < controlCount; i++)
            {
                var pair = new Tuple<string, string>(null, null);
                while (pair.Item1 == pair.Item2 || testSetTemp.Keys.Contains(pair) || controlSetTemp.Keys.Contains(pair))
                    pair = new Tuple<string, string>(allWords[rand.Next(allWords.Length)], allWords[rand.Next(allWords.Length)]);
                var rel = m.AreWordsInRelation(pair.Item1, pair.Item2);
                controlSetTemp.Add(pair, rel);
                controlSetTemp.Add(Inverse(pair), rel);
            }
            foreach(var keyVal in testSetTemp)
            {
                var it1 = m.alphabet.Translate(keyVal.Key.Item1).ToArray();
                var it2 = m.alphabet.Translate(keyVal.Key.Item2).ToArray();
                TestSet.Add(new Tuple<int[], int[]>(it1, it2), keyVal.Value);
            }
            foreach (var keyVal in controlSetTemp)
            {
                var it1 = m.alphabet.Translate(keyVal.Key.Item1).ToArray();
                var it2 = m.alphabet.Translate(keyVal.Key.Item2).ToArray();
                ControlSet.Add(new Tuple<int[], int[]>(it1, it2), keyVal.Value);
            }
        }
    }
}
