using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageProcessor
{
    public class Machine
    {
        public double[,] stateFunction;
        public readonly int States;

        public Alphabet alphabet;

        public int GetFinishedState(string s)
        {
            return GetFinishedState(alphabet.Translate(s));
        }

        public int GetFinishedState(List<int> Word)
        {
            int state = 0;
            foreach (var L in Word)
            {
                double c = stateFunction[state, L];
                c = Math.Round(c);
                state = (int)c;
            }
            return state;
        }

        private Machine(Alphabet A, double[,] SF, int states)
        {
            alphabet = A;
            stateFunction = SF;
            States = states;
        }
        public static Machine RandomMachineFactory(int states, Alphabet A)
        {
            double[,] SF = new double[states, A.Letters.Length];
            Random R = new Random();
            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < A.Letters.Length; j++)
                {
                    SF[i,j] =(double) R.Next(states);
                }
            }
            return new Machine(A,SF,states);
        }

        public List<Machine> GetMachinesWithMoreStates(int number)
        {
            List<Machine> result = new List<Machine>();
            for(int i=0;i<number;i++) result.Add(RandomMachineFactory(States+1,alphabet));
            foreach (var M in result)
            {
                for (int i = 0; i < States; i++)
                {
                    for (int j = 0; j < alphabet.Letters.Length; j++)
                    {
                        M.stateFunction[i, j] = stateFunction[i, j];
                    }
                }
            }
            return result;
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
            double[,] D = new double[States,alphabet.Letters.Length];
            for(int i=0;i<States;i++) for (int j = 0; j < alphabet.Letters.Length; j++) D[i, j] = stateFunction[i, j];
            return D;
        }
    }
}
