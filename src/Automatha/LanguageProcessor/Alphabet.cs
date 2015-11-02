using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageProcessor
{
    public class Alphabet
    {
        private char[] letters;

        public List<int> Translate(string s)
        {
            List<int> L = s.Select(Translate).ToList();
            return L;
        }

        private int Translate(char c)
        {
            for(int i=0;i<letters.Length;i++) if (letters[i] == c) return i;
            return -1;
        }
        public string Translate(List<int> L)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var index in L)
            {
                sb.Append(letters[index]);
            }
            return sb.ToString();
        }
        public Alphabet(char[] Set)
        {
            letters = Set.ToArray();
        }

        public char[] Letters
        {
            get
            {
                return letters;
            }
        }

        public char this[int k]
        {
            get { return letters[k]; }
        }
        
    }
}
