using LanguageProcessor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TestGenerator
{ 

    /// <summary>
    /// Klasa opisująca zbiory testowe
    /// </summary>
    public class TestSets
    {
        public class WordEqualityComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] t1, int[] t2)
            {
                if (t1 == null && t2 == null) return true;
                if (t1 == null || t2 == null) return false;
                if (t1.Length != t2.Length) return false;
                for (int i = 0; i < t1.Length; i++) 
                    if (t1[i] != t2[i]) 
                        return false;
                return true;
            }

            public int GetHashCode(int[] obj)
            {
                return obj.GetHashCode();
            }
        }

        private class WordPairEqualityComparer : IEqualityComparer<Tuple<int[],int[]>>
        {
            public bool Equals(int[] t1, int[] t2)
            {
                if (t1 == null && t2 == null) return true;
                if (t1 == null || t2 == null) return false;
                if (t1.Length != t2.Length) return false;
                for (int i = 0; i < t1.Length; i++) 
                    if (t1[i] != t2[i]) 
                        return false;
                return true;
            }

            public bool Equals(Tuple<int[], int[]> x, Tuple<int[], int[]> y)
            {
                return (Equals(x.Item1, y.Item1) && Equals(x.Item2, y.Item2)) || (Equals(x.Item1, y.Item2) && Equals(x.Item2, y.Item1));
            }

            public int GetHashCode(Tuple<int[], int[]> obj)
            {
                return obj.GetHashCode();
            }
        }
        private static Tuple<T1,T2> Inverse<T1, T2>(Tuple<T2, T1> t)
        {
            return new Tuple<T1, T2>(t.Item2, t.Item1);
        }
        /// <summary>
        /// Konstruktor tworzący TestSets
        /// </summary>
        /// <param name="trainingSet">Zbiór testowy</param>
        /// <param name="testSet">Zbiór kontrolny</param>
        /// <param name="m">Automat zawierajacy alfabet testu</param>
        private TestSets(Dictionary<Tuple<int[], int[]>, bool> trainingSet, Dictionary<Tuple<int[], int[]>, bool> testSet, Machine m)
        {
            TrainingSet = trainingSet;
            TestSet = testSet;
            LetterCount = m.LetterCount;
            AllWords = GetAllWords();
        }

        private List<int[]> GetAllWords()
        {
            var allWords = TrainingSet.Keys.ToArray().Select(x => x.Item1).ToList();
            var p = TrainingSet.Keys.ToArray().Select(x => x.Item2).Where(x => !allWords.Contains(x, new WordEqualityComparer())).ToList();
            allWords = allWords.Concat(p).ToList();
            return allWords;
        }
        /// <summary>
        /// Metoda Ładuje zbiór testowy z pliku
        /// </summary>
        /// <param name="path">Ścieżka do pliku</param>
        /// <param name="m">Automat zawierający informacje o alfabecie</param>
        /// <param name="sets">Wyjściowy parametr - utworzony zbiór</param>
        /// <returns>Informacja o pomyślnym załadowaniu automatu</returns>
        public static bool LoadSetFromFile(string path, Machine m, out TestSets sets)
        {
            bool result = false;
            sets = null;
            using (var reader = new StreamReader(path, Encoding.UTF8))
            {
                try
                {
                    var filetext = reader.ReadToEnd();
                    var localSets = filetext.Split(new string[] { "!!TrainingSet:", "!!TestSet:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (localSets.Length != 2) throw new ArgumentException("Wrong paragraph format");
                    var testSet = ParseSets(localSets[0]);
                    var controlSet = ParseSets(localSets[1]);

                    if (testSet.Keys.Any(x => x.Item1.Any(i => i >= m.LetterCount || i < 0) || x.Item2.Any(i => i >= m.LetterCount || i < 0)))
                        throw new ArgumentException("Alphabets of the machine and file do not match");

                    sets = new TestSets(testSet, controlSet, m);
                    result = true;
                }
                catch (Exception e)
                {
                    sets = null;
                    result = false;
                    Debug.WriteLine(e.Message);
                }
            }
            return result;
        }
        /// <summary>
        /// Metoda przetwarza ciąg znaków w zbiór par słów
        /// </summary>
        /// <param name="v">łańcuch znaków</param>
        /// <returns>Słownik zawierający pary słów i ich relacje</returns>
        private static Dictionary<Tuple<int[], int[]>, bool> ParseSets(string v)
        {
            var result = new Dictionary<Tuple<int[], int[]>, bool>();
            var lines = v.Split('\n');
            foreach(var l in lines)
            {
                var line = l.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var split = line.Split(new char[] { ';' });
                if (split.Length != 3) throw new ArgumentException("Wrong line format");
                var item1temp = split[0].Split(new char[] { ',' });
                var item1 = item1temp.Length == 1 && string.IsNullOrWhiteSpace(item1temp[0]) ? new int[0] :
                    item1temp.Select(x => int.Parse(x)).ToArray();
                var item2temp = split[1].Split(new char[] { ',' });
                var item2 = item2temp.Length == 1 && string.IsNullOrWhiteSpace(item2temp[0]) ? new int[0] :
                    item2temp.Select(x => int.Parse(x)).ToArray();
                var val = bool.Parse(split[2]);
                result.Add(new Tuple<int[], int[]>(item1, item2), val);
            }
            return result;
        }
        /// <summary>
        /// Metoda przetwarza słownik w ciąg znaków
        /// </summary>
        /// <param name="dict">Słownik do przetworzenia</param>
        /// <returns>ciąg znaków</returns>
        private static string ConvertToString(Dictionary<Tuple<int[], int[]>, bool> dict)
        {
            var sb = new StringBuilder();
            foreach(var pair in dict)
            {
                var lb = new StringBuilder();
                lb.Append(SeparatedArray(pair.Key.Item1, ','));
                lb.Append(';');
                lb.Append(SeparatedArray(pair.Key.Item2, ','));
                lb.Append(';');
                lb.Append(pair.Value);
                sb.AppendLine(lb.ToString());
            }
            return sb.ToString();
        }
        /// <summary>
        /// Metoda zapisuje zbiór do pliku
        /// </summary>
        /// <param name="path">Ścieżka pliku</param>
        /// <returns>Informacja o pomyślnym zakończeniu</returns>
        public bool SaveSetToFile(string path)
        {
            bool result = false;
            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("!!TrainingSet:");
                    sb.AppendLine(ConvertToString(TrainingSet));
                    sb.AppendLine("!!TestSet:");
                    sb.AppendLine(ConvertToString(TestSet));
                    writer.Write(sb.ToString());
                    result = true;
                }
                catch (Exception e)
                {
                    result = false;
                    Debug.WriteLine(e.Message);
                }
            }
            return result;
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
        /// <summary>
        /// Zbiór treningowy
        /// </summary>
        public Dictionary<Tuple<int[], int[]>, bool> TrainingSet
        { get; private set; }

        public List<int[]> AllWords
        { get; private set; }

        /// <summary>
        /// Zbiór kontrolny
        /// </summary>
        public Dictionary<Tuple<int[], int[]>, bool> TestSet
        { get; private set; }
        Random rand = new Random();

        /// <summary>
        /// Konstruktor klasy TestSets
        /// </summary>
        /// <param name="m">Automat</param>
        /// <param name="shortWordMaxLength">Maksymalna długość "krótkiego" słowa</param>
        /// <param name="trainingSetLongWordsCount">Liczba "długich" słów w zbiorze treningowym</param>
        /// <param name="testSetSize">Wielkość zbioru testowego</param>
        public TestSets(Machine m, int shortWordMaxLength = 4, int trainingSetLongWordsCount = 303810, int testSetSize = 607620)
            : this(new Dictionary<Tuple<int[], int[]>, bool>(), new Dictionary<Tuple<int[], int[]>, bool>(), m)
        {
            List<int[]> shortWords;
            List<int[]> longWords;
            GenerateAllWords(shortWordMaxLength, trainingSetLongWordsCount + testSetSize, out shortWords, out longWords);
            longWords = GenerateRandomConcats(longWords, trainingSetLongWordsCount, m.alphabet);
            GenerateSets(m, testSetSize, trainingSetLongWordsCount, shortWords, longWords);
        }

       /// <summary>
       /// Metoda generuje wszystkie słowa o długości niewiększej niż parametr shortWordMaxLength
       /// </summary>
       /// <param name="shortWordMaxLength">Maksymalna długość "krótkiego" słowa</param>
       /// <param name="longWordsCount">Liczba "długich" słów do wygenerowania</param>
       /// <returns>Lista wszystkich słów</returns>
        private void GenerateAllWords(int shortWordMaxLength, int longWordsCount,
            out List<int[]> shortWords, out List<int[]> longWords)
        {
            shortWords = new List<int[]>();
            longWords = new List<int[]>();
            var previousStep = new List<int[]>();
            var actualStep = new List<int[]> { new int[0] };
            // Do słów długości shortWordMaxLength
            for (int i = 1; i <= shortWordMaxLength + 1 || actualStep.Count < longWordsCount; i++)
            {
                previousStep.AddRange(actualStep);
                actualStep.Clear();
                // Dodanie j-tej litery
                for (int j = 0; j < LetterCount; j++)
                {
                    // Na koniec k-tego słowa
                    for (int k = 0; k < previousStep.Count; k++)
                    {
                        var nArr = new int[previousStep[k].Length + 1];
                        previousStep[k].CopyTo(nArr, 0);
                        nArr[previousStep[k].Length] = j;
                        actualStep.Add(nArr);
                    }
                }
                if (i > shortWordMaxLength)
                    longWords.AddRange(actualStep);
                else
                    shortWords.AddRange(actualStep);
                previousStep.Clear();
            }
        }
        /// <summary>
        /// Metoda generuje losowe połączenia słów w celu utworzenia dłuższych słów
        /// </summary>
        /// <param name="words">Słowa do łączenia</param>
        /// <param name="generateCount">Ilość słów do wygenerowania</param>
        /// <param name="alphabet">Alfabet</param>
        /// <returns>Lista słów</returns>
        private List<int[]> GenerateRandomConcats(List<int[]> words, int generateCount, Alphabet alphabet)
        {
            var wordsCount = words.Count;
            var wordsArray = words.Select(x => alphabet.Translate(x.ToList())).ToArray();
            var wordsSet = new HashSet<string>(wordsArray);
            var res = new HashSet<string>();
            for (int i = 0; i < generateCount; i++)
            {
                var w = "";
                while (res.Contains(w) || wordsSet.Contains(w))
                    w = wordsArray[rand.Next(wordsCount)] + wordsArray[rand.Next(wordsCount)];
                res.Add(w);
            }
            return res.Select(x => alphabet.Translate(x).ToArray()).ToList();
        }
       
      
        /// <summary>
        /// Metoda łączy słowa w pary
        /// </summary>
        /// <param name="m">Automat</param>
        /// <param name="testSetSize">Rozmiar zbioru testowego</param>
        /// <param name="trainingSetLongWordsCount">Liczba "długich" słów w zbiorze treningowym</param>
        /// <param name="shortWords">Lista "krótkich" słów</param>
        /// <param name="longWords">Lista "długich" słów generowanych losowo</param>
        private void GenerateSets(Machine m, int testSetSize, int trainingSetLongWordsCount, List<int[]> shortWords, List<int[]> longWords)
        {
            AllWords = shortWords.Concat(longWords).ToList();
            var Comparer = new WordPairEqualityComparer();
            var trainingSet = new Dictionary<Tuple<int[], int[]>, bool>(Comparer);

            // Zbiór treningowy - generowanie wszystkich słów krótkich
            for (int i = 0; i < shortWords.Count; i++)
                for (int j = i+1; j < shortWords.Count; j++)
                {
                    var rel = m.AreWordsInRelation(shortWords[i], shortWords[j]);
                    var w1 = shortWords[i];
                    var w2 = shortWords[j];
                    trainingSet.Add(new Tuple<int[], int[]>(w1, w2), rel);
                }

            // Zbiór treningowy - generowanie słów długich
            for (int i = 0; i < trainingSetLongWordsCount; i++)
            {
                var pair = new Tuple<int[], int[]>(null, null);
                while (Comparer.Equals(pair.Item2, pair.Item1) || trainingSet.ContainsKey(pair))
                    pair = new Tuple<int[], int[]>(longWords[rand.Next(longWords.Count)], longWords[rand.Next(longWords.Count)]);
                var rel = m.AreWordsInRelation(pair.Item1, pair.Item2);
                trainingSet.Add(pair, rel);
            }

            // Zbiór testowy - generowanie słów długich
            var testSet = new Dictionary<Tuple<int[], int[]>, bool>(Comparer);
            for (int i = 0; i < testSetSize; i++)
            {
                var pair = new Tuple<int[], int[]>(null, null);
                while (Comparer.Equals(pair.Item2, pair.Item1) || testSet.ContainsKey(pair) || trainingSet.ContainsKey(pair))
                    pair = new Tuple<int[], int[]>(longWords[rand.Next(longWords.Count)], longWords[rand.Next(longWords.Count)]);
                var rel = m.AreWordsInRelation(pair.Item1, pair.Item2);
                testSet.Add(pair, rel);
            }

            TrainingSet = trainingSet;
            TestSet = testSet;
        }
    }
}
