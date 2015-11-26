using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageProcessor
{
    public class Machine
    {
        public double[,] stateFunction; //private?
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

        public Alphabet alphabet; //private?

        public int GetFinishedState(string s)
        {
            return GetFinishedState(alphabet.Translate(s));
        }

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

        public Machine(Alphabet A, double[,] SF)
        {
            alphabet = A;
            stateFunction = SF;
        }

        public static Machine GenerateRandomMachine(int states, Alphabet alphabet)
        {
            double[,] SF = new double[states, alphabet.Letters.Length];
            Random R = new Random();
            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < alphabet.Letters.Length; j++)
                {
                    SF[i, j] = R.Next(states);
                }
            }
            return new Machine(alphabet, SF);
        }

        public List<Machine> GetMachinesWithMoreStates(int machineCount, int statesToAdd = 1)
        {
            if (machineCount < 0 || statesToAdd < 1) throw new ArgumentException();
            //List<Machine> result = new List<Machine>();
            //for (int i = 0; i < machineCount; i++) result.Add(GenerateRandomMachine(States + statesToAdd, alphabet));
            //foreach (var M in result)
            //{
            //    for (int i = 0; i < States; i++)
            //    {
            //        for (int j = 0; j < alphabet.Letters.Length; j++)
            //        {
            //            M.stateFunction[i, j] = stateFunction[i, j];
            //        }
            //    }
            //}
            //return result;
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

        public bool AreWordsInRelation(int[] Word1, int[] Word2)
        {
            return AreWordsInRelation(Word1.ToList(), Word2.ToList());
        }

        public bool AreWordsInRelation(List<int> Word1, List<int> Word2)
        {
            return GetFinishedState(Word1) == GetFinishedState(Word2);
        }

        public bool AreWordsInRelation(string W1, string W2)
        {
            return AreWordsInRelation(alphabet.Translate(W1), alphabet.Translate(W2));
        }

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
