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
        public static double DeathChance;
        const double EPS = 0.000000000001;

        public static void Initialize(int maxSteps, double deathChance=0)
        {
            DeathChance = deathChance;
            MaxChange = maxSteps;
            LastChange = 0;
            GlobalError = int.MaxValue;
            random = new Random();
        }

        public Particle(Machine core)
        {
            LocalError = int.MaxValue;
            Core = core;
            if (GlobalMax == null) GlobalMax = core.GetFunctionCopy();
            velocity = new double[core.StateCount,core.alphabet.Letters.Length];
            for (int i = 0; i < core.StateCount; i++)
            {
                for (int j = 0; j < core.alphabet.Letters.Length; j++)
                {
                    velocity[i, j] = random.NextDouble() * 2 * core.StateCount - core.StateCount;//velocity może mieć dowolne wartości, nawet ujemne
                }
            }
            LocalError = UpdateLocal();
            TryUpdateGlobal();
        }

        public static bool StartSteps()
        {
            return LastChange != MaxChange;
        }

        public async Task<int> Step()
        {
            int actualError = int.MaxValue;
            await Task.Run(() =>
            {
                for (int i = 0; i < Core.StateCount; i++)
                {
                    for (int j = 0; j < Core.alphabet.Letters.Length; j++)
                    {
                        var newv = (MachinePSO.Omega * velocity[i, j]) +
                                         (MachinePSO.OmegaLocal * random.NextDouble() * (Max[i, j] - Core.stateFunction[i, j])) +
                                         (MachinePSO.OmegaGlobal * random.NextDouble() * (GlobalMax[i, j] - Core.stateFunction[i, j]));
                        velocity[i, j] = newv;
                        Core.stateFunction[i, j] += velocity[i, j];
                        if (Core.stateFunction[i, j] < 0) Core.stateFunction[i, j] = 0;
                        if (Core.stateFunction[i, j] > Core.StateCount - EPS)
                            Core.stateFunction[i, j] = ((double)Core.StateCount - EPS);
                    }

                }
                actualError = UpdateLocal();
            });
            return actualError;
        }

        public int UpdateLocal()
        {
            var actualError = MachinePSO.Words.Sum(WordPair => Core.AreWordsInRelation(WordPair.Item1, WordPair.Item2) == MachinePSO.AreWordsInRelation(WordPair.Item1, WordPair.Item2) ? 0 : 1);
            if (actualError < LocalError)
            {
                LocalError = actualError;
                Max = Core.GetFunctionCopy();
            }
            return actualError;
        }

        public void TryUpdateGlobal()
        {
            if (LocalError < GlobalError)
            {
                GlobalError = LocalError;
                LastChange = -1;
                GlobalMax = Core.GetFunctionCopy();
            }
        }
        public static void EndStep()
        {
            LastChange++;
        }

        public volatile Machine Core;
        public volatile double[,] velocity;
        public volatile double[,] Max;
        public volatile int LocalError;
    }
}
