using LanguageProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSO
{
    public class Particle
    {
        
        public static double[,] GlobalMax;
        public static int GlobalError;
        private static int LastChange;
        private static int MaxChange;
        private static Random random;
        public static void Initialise(int maxSteps)
        {
            MaxChange = maxSteps;
            LastChange = 0;
            GlobalError = int.MaxValue;
            random = new Random();
        }

        public Particle(Machine core)
        {
            LocalError = int.MaxValue;
            Core = core;
            velocity = new double[core.States,core.alphabet.Letters.Length];
            for (int i = 0; i < core.States; i++)
            {
                for (int j = 0; j < core.alphabet.Letters.Length; j++)
                {
                    velocity[i, j] = random.NextDouble()*(core.States - 1);
                }
            }
            int Error = MachinePSO.Words.Sum(WordPair => Core.AreWordsInRelation(WordPair.Item1, WordPair.Item2) == MachinePSO.AreWordsInRelation(WordPair.Item1, WordPair.Item2) ? 0 : 1);
            if (Error < LocalError)
            {
                LocalError = Error;
                Max = Core.GetFunctionCopy();

            }
            if (LocalError < GlobalError)
            {
                LastChange = -1;
                GlobalMax = Max;
            }
        }
        public static bool StartSteps()
        {
            return LastChange != MaxChange;
        }

        public void Step()
        {
            
            for (int i = 0; i < Core.States; i++)
            {
                for (int j = 0; j < Core.alphabet.Letters.Length; j++)
                {
                    velocity[i, j] = MachinePSO.Omega*velocity[i, j] +
                                     MachinePSO.OmegaLocal*random.NextDouble()*(Max[i, j] - Core.stateFunction[i, j])
                                     +
                                     MachinePSO.OmegaGlobal*random.NextDouble()*
                                     (GlobalMax[i, j] - Core.stateFunction[i, j]);
                    Core.stateFunction[i, j] += velocity[i, j];
                    if (Core.stateFunction[i, j] < 0) Core.stateFunction[i, j] = 0;
                    if (Core.stateFunction[i, j] > Core.States-1) Core.stateFunction[i, j] = (double) (Core.States-1);
                }

            }
            int Error = MachinePSO.Words.Sum(WordPair => Core.AreWordsInRelation(WordPair.Item1, WordPair.Item2) == MachinePSO.AreWordsInRelation(WordPair.Item1, WordPair.Item2) ? 0 : 1);
            if (Error < LocalError)
            {
                LocalError = Error;
                Max = Core.GetFunctionCopy();

            }
            if (LocalError < GlobalError)
            {
                LastChange = -1;
                GlobalMax = Max;
            }
        }

        public static void EndStep()
        {
            LastChange++;
        }
        public Machine Core;
        public double[,] velocity;
        public double[,] Max;
        public int LocalError;

    }
}
