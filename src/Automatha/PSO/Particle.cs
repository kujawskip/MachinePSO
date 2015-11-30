using LanguageProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGenerator;

namespace PSO
{
    /// <summary>
    /// Klasa realizująca cząsteczke PSO.
    /// </summary>
    public class Particle
    {
       /// <summary>
       /// Funkcja przejścia najlepszego globalnie automatu
       /// </summary>
        internal static double[,] GlobalMax;
        /// <summary>
        /// Błąd najlepszego globalnie automatu
        /// </summary>
        internal static int GlobalError;

        private static int LastChange;
        private static int MaxChange;
        private static Random random;
        internal static double DeathChance;
        const double EPS = 0.000000000001;
        /// <summary>
        /// Metoda inicjalizuje parametry wspólne dla cząsteczek
        /// </summary>
        /// <param name="maxSteps">Maksymalna ilość kroków, po której w razie braku poprawy rozwiązania globalnego przerywane są obliczenia</param>
        /// <param name="deathChance">Szansa na śmierć cząsteczki w każdym kroku - martwa cząsteczka nie wykonuje obliczeń</param>
        public static void Initialize(int maxSteps, double deathChance=0)
        {
            DeathChance = deathChance;
            MaxChange = maxSteps;
            LastChange = 0;
            GlobalError = int.MaxValue;
            random = new Random();
        }
        /// <summary>
        /// Konstruktor tworzący cząsteczke
        /// </summary>
        /// <param name="core">Automat reprezentujący stan początkowy cząsteczki</param>
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
        /// <summary>
        /// Metoda potwierdzająca możliwość wykonania kroku przez cząsteczki.
        /// </summary>
        /// <returns>Wartość boolean - false oznacza przerwanie iteracji</returns>
        public static bool StartSteps()
        {
            return LastChange != MaxChange;
        }
        /// <summary>
        /// Metoda realizująca jeden krok cząsteczki w PSO (asynchroniczna)
        /// </summary>
        /// <returns>Wartość błędu cząsteczki</returns>
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
        private int GetBetterError()
        {
            Dictionary<int[], int> FinishedStates = new Dictionary<int[], int>(new TestSets.WordEqualityComparer());
            foreach (var word in MachinePSO.AllWords)
            {
                FinishedStates.Add(word, Core.GetFinishedState(word.ToList()));
            }
            int i = 0;
            foreach (var WordPair in MachinePSO.Words)
            {
                var rel = FinishedStates[WordPair.Item1] == FinishedStates[WordPair.Item2];
                i += rel == MachinePSO.AreWordsInRelation(WordPair.Item1, WordPair.Item2) ? 0 : 1;
                if (i > LocalError) break;
            }
            return i;
        }
        /// <summary>
        /// Metoda licząca błąd i aktualizująca najlepszą wartość lokalną
        /// </summary>
        /// <returns>Błąd dla danej cząsteczki</returns>
        public int UpdateLocal()
        {
            var actualError = GetBetterError();
            if (actualError < LocalError)
            {
                LocalError = actualError;
                Max = Core.GetFunctionCopy();
            }
            return actualError;
        }
        /// <summary>
        /// Metoda aktualizująca najlepszą wartość globalną
        /// </summary>
        public void TryUpdateGlobal()
        {
            if (LocalError < GlobalError)
            {
                GlobalError = LocalError;
                LastChange = -1;
                GlobalMax = Core.GetFunctionCopy();
            }
        }
        /// <summary>
        /// Metoda wywoływana po wykonaniu kroków przez wszystkie cząsteczki. Podwyższa wartość LastChange związaną
        /// </summary>
        public static void EndStep()
        {
            LastChange++;
        }

        /// <summary>
        /// Automat (pozycja w przestrzeni rozwiązań) cząsteczki
        /// </summary>
        internal volatile Machine Core;
        /// <summary>
        /// Prędkość ruchu cząsteczki
        /// </summary>
        internal volatile double[,] velocity;
        /// <summary>
        /// Funkcja przejścia najlepszego lokalnie automatu
        /// </summary>
        internal volatile double[,] Max;
        /// <summary>
        /// Błąd najlepszego lokalnie automatu
        /// </summary>
        internal volatile int LocalError;
    }
}
