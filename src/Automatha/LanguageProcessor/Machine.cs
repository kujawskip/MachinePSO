using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageProcessor
{
    /// <summary>
    /// Klasa realizująca automat skończony
    /// </summary>
    public class Machine
    {
        /// <summary>
        /// Funkcja przejścia automatu
        /// </summary>
        public double[,] stateFunction;
        private static Random R = new Random();
        public int StateCount
        {
            get
            {
                return stateFunction.GetLength(0);
            }
        }
        public int LetterCount
        {
            get { return stateFunction.GetLength(1); }
        }

        /// <summary>
        /// Alfabet przypisany do automatu
        /// </summary>
        public Alphabet alphabet;

        /// <summary>
        /// Metoda uzyskuje stan na którym automat kończy obliczenia dla łańcucha znaków 
        /// </summary>
        /// <param name="s">Obliczany łańuch znaków</param>
        /// <returns>Stan na którym zakończyły się obliczenia</returns>
        public int GetFinishedState(string s)
        {
            return GetFinishedState(alphabet.Translate(s));
        }
        /// <summary>
        /// Metoda uzyskuje stan na którym automat kończy obliczenia dla słowa
        /// </summary>
        /// <param name="word">Obliczane słowo</param>
        /// <returns>Stan na którym zakończyły się obliczenia</returns>
        public int GetFinishedState(List<int> word)
        {
            int state = 0;
            foreach (var symbol in word)
            {
                double c = stateFunction[state, symbol];
                //c = Math.Round(c);
                state = (int)c;
            }
            return state;
        }

        /// <summary>
        /// Konstruktor Automatu
        /// </summary>
        /// <param name="A"> Alfabet przypisany do automatu</param>
        /// <param name="SF"> Funkcja przejścia automatu </param>
        public Machine(Alphabet A, double[,] SF)
        {
            alphabet = A;
            stateFunction = SF;
        }
        /// <summary>
        /// Metoda generuje losowy automat (o losowej poprawnej funkcji przejścia) o zadanej ilości stanów i alfabecie
        /// </summary>
        /// <param name="states">Ilość stanów</param>
        /// <param name="alphabet">alfabet</param>
        /// <returns>Losowy automat</returns>
        public static Machine GenerateRandomMachine(int states, Alphabet alphabet)
        {
            double[,] SF = new double[states, alphabet.Letters.Length];
            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < alphabet.Letters.Length; j++)
                {
                    SF[i, j] = R.Next(states);
                }
            }
            return new Machine(alphabet, SF);
        }
        /// <summary>
        /// Metoda tworzy automaty na podstawie istniejącego automatu mającego mniej stanów. Funkcja przejścia jest przenoszona ze starego automatu a nowe wiersze wypełniane są losowo
        /// </summary>
        /// <param name="machineCount">Ilość automatów pochodnych do utworzenia</param>
        /// <param name="statesToAdd">Wartość określająca o ile więcej stanów posiadają nowe automaty</param>
        /// <returns>Lista automatów</returns>
        public List<Machine> GetMachinesWithMoreStates(int machineCount, int statesToAdd = 1)
        {
            if (machineCount < 0 || statesToAdd < 1) throw new ArgumentException();
            var rand = new Random();
            List<Machine> result = new List<Machine>();
            for(int i=0; i<machineCount; i++)
            {
                var SF = new double[StateCount + statesToAdd, LetterCount];
                for (int stateIndex = 0; stateIndex < SF.GetLength(0); stateIndex++)
                {
                    for (int letterIndex = 0; letterIndex < SF.GetLength(1); letterIndex++)
                    {
                        SF[stateIndex, letterIndex] =
                            stateIndex >= StateCount ?
                            rand.Next(StateCount + statesToAdd) :
                            stateFunction[stateIndex, letterIndex];
                    }
                }
                result.Add(new Machine(alphabet, SF));
            }
            return result;
        }
        /// <summary>
        /// Metoda odpawiada na pytanie czy dwa słowa są w relacji indukowanej przez język odpowiadającej automatowi
        /// </summary>
        /// <param name="Word1">Słowo 1</param>
        /// <param name="Word2">Słowo 2</param>
        /// <returns>Wartość boolean określająca czy słowa są ze sobą w relacji</returns>
        public bool AreWordsInRelation(int[] Word1, int[] Word2)
        {
            return AreWordsInRelation(Word1.ToList(), Word2.ToList());
        }
        /// <summary>
        /// Metoda odpawiada na pytanie czy dwa słowa są w relacji indukowanej przez język odpowiadającej automatowi
        /// </summary>
        /// <param name="Word1">Słowo 1</param>
        /// <param name="Word2">Słowo 2</param>
        /// <returns>Wartość boolean określająca czy słowa są ze sobą w relacji</returns>
        public bool AreWordsInRelation(List<int> Word1, List<int> Word2)
        {
            return GetFinishedState(Word1) == GetFinishedState(Word2);
        }
        /// <summary>
        /// Metoda odpawiada na pytanie czy dwa słowa są w relacji indukowanej przez język odpowiadającej automatowi
        /// </summary>
        /// <param name="W1">Słowo 1</param>
        /// <param name="W2">Słowo 2</param>
        /// <returns>Wartość boolean określająca czy słowa są ze sobą w relacji</returns>
        public bool AreWordsInRelation(string W1, string W2)
        {
            return AreWordsInRelation(alphabet.Translate(W1), alphabet.Translate(W2));
        }
        /// <summary>
        /// Funkcja zwraca kopię tabeli zawierającej funkcję przejścia automatu
        /// </summary>
        /// <returns>Kopia funkcji przejścia</returns>
        public double[,] GetFunctionCopy()
        {
            double[,] D = new double[StateCount, LetterCount];
            for (int i = 0; i < StateCount; i++)
                for (int j = 0; j < LetterCount; j++)
                    D[i, j] = stateFunction[i, j];
            return D;
        }
    }
}
