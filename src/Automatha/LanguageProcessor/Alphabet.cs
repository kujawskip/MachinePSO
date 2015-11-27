using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageProcessor
{
    /// <summary>
    /// Klasa realizująca alfabet
    /// </summary>
    public class Alphabet
    {
        private char[] letters;

        /// <summary>
        /// Metoda tłumacząca łańcuch znaków na indeksy tychże znaków w alfabecie
        /// </summary>
        /// <param name="s">Łańcuch znaków</param>
        /// <returns>Lista indeksów</returns>
        public List<int> Translate(string s)
        {
            List<int> L = s.Select(Translate).ToList();
            return L;
        }

        private int Translate(char c)
        {
            for (int i = 0; i < letters.Length; i++) if (letters[i] == c) return i;
            return -1;
        }
        /// <summary>
        /// Metoda tłumaczy indeksy znaków w alfabecie na słowa
        /// </summary>
        /// <param name="L">Lista indeksów znaków</param>
        /// <returns>Słowo</returns>
        public string Translate(List<int> L)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var index in L)
            {
                sb.Append(letters[index]);
            }
            return sb.ToString();
        }
        /// <summary>
        /// Konstruktor alfabetu
        /// </summary>
        /// <param name="Set">Tablica znaków alfabetu</param>
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
