using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PSO
{
    public delegate bool LanguageRelation(List<int> word1, List<int> word2);
    public static class MachinePSO
    {
        public static List<Tuple<List<int>, List<int>>> Words;
        private static LanguageRelation relation;
        public static readonly double Omega, OmegaLocal, OmegaGlobal;
        public static void Initialise(List<Tuple<List<int>, List<int>>> words,LanguageRelation relation)
        {
            Words = words;
            MachinePSO.relation = relation;
        }

        public static bool AreWordsInRelation(List<int> list1, List<int> list2)
        {
            return relation(list1, list2);
        }
    }
}
