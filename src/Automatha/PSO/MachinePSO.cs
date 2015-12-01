using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LanguageProcessor;
using System.ComponentModel;

namespace PSO

{
    /// <summary>
    /// Delegat opisujący relacje indukowaną przez język
    /// </summary>
    /// <param name="word1"> Słowo 1</param>
    /// <param name="word2">Słowo 2</param>
    /// <returns>Informacje czy dwa słowa są w relacji</returns>
    public delegate bool LanguageRelation(int[] word1, int[] word2);
    /// <summary>
    /// Klasa statyczna realizująca PSO dla cząsteczek zawierających automaty
    /// </summary>
    public static class MachinePSO
    {
        /// <summary>
        /// Lista par słów testowych
        /// </summary>
        public static List<Tuple<int[], int[]>> Words;
        private static LanguageRelation relation;
        /// <summary>
        /// Najlepszy automat na przestrzeni wszystkich obliczeń PSO
        /// </summary>
        public static volatile Machine BestMachine;
        /// <summary>
        /// Błąd najlepszego automatu
        /// </summary>
        public static volatile int BestError;
        private static Alphabet alphabet;
        private static int MaxStates;
        private static List<Particle> Particles;
        public static List<int[]> AllWords { get; private set; }
        public static bool Stop { get; set; }

        public static double Omega, OmegaLocal, OmegaGlobal;
        private static Random random = new Random();

        public static event EventHandler BestErrorChanged;

        private static void RaiseEvent()
        {
            var handler = BestErrorChanged;
            if (handler != null)
                handler(typeof(MachinePSO), EventArgs.Empty);
        }

        public static int PerformTest(Dictionary<Tuple<int[], int[]>, bool> set, out double percentage)
        {
            if (set.Count == 0)
            {
                percentage = 0;
                return 0;
            }
            int err = 0;
            foreach (var key in set.Keys)
            {
                if (set[key] != BestMachine.AreWordsInRelation(key.Item1, key.Item2)) err++;
            }
            percentage = 100 * (double)err / set.Count;
            return err;
        }

        /// <summary>
        /// Metoda inicjalizująca parametry PSO
        /// </summary>
        /// <param name="words">Lista par słów testowych</param>
        /// <param name="relation">Delegat opisujący relacje indukowaną przez język</param>
        /// <param name="MaxStates">Maksymalna ilość stanów osiągalna przez automat</param>
        /// <param name="A">Alfabet dla którego rozwiązywany jest problem</param>
        public static void Initialize(List<Tuple<int[], int[]>> words, LanguageRelation relation, int MaxStates, Alphabet A,List<int[]> allwords)
        {
            Particles = new List<Particle>();
            Words = words;
            AllWords = allwords;
            MachinePSO.relation = relation;
            BestError = int.MaxValue;
            BestMachine = Machine.GenerateRandomMachine(1, A);
            alphabet = A;
            MachinePSO.MaxStates = MaxStates;
        }
        /// <summary>
        /// Metoda wprowadzająca wagi dla PSO
        /// </summary>
        /// <param name="Omega">Waga prędkości</param>
        /// <param name="OmegaLocal">Waga lokalnego maksimum</param>
        /// <param name="OmegaGlobal">Waga globalnego maksimum</param>
        public static void InputParameters(double Omega, double OmegaLocal, double OmegaGlobal)
        {
            MachinePSO.Omega = Omega;
            MachinePSO.OmegaGlobal = OmegaGlobal;
            MachinePSO.OmegaLocal = OmegaLocal;
        }
        /// <summary>
        /// Metoda realizuje jeden krok dla każdej cząsteczki
        /// </summary>
        /// <returns>Informacja czy należy wywoływać następne kroki</returns>
        private static async Task<bool> Step()
        {
            if (!Particle.StartSteps()) return false;
            var steps = Particles.Select(p => Task.Run(() => p.Step())).ToArray();
            await Task.WhenAll(steps);
            var best = Particles.Find(x => x.LocalError == Particles.Min(y => y.LocalError));
            best.TryUpdateGlobal();
           
            Particle.EndStep();
            return true;
        }
        /// <summary>
        /// Metoda realizująca jeden pełny zestaw obliczeń PSO dla danej ilości stanów sprawdzanych automatów
        /// </summary>
        /// <param name="State">Ilość stanów</param>
        /// <param name="ParticlesCount">Ilość cząsteczek</param>
        /// <param name="ProgressCount">Ilość cząsteczek generowanych na podstawie najlepszego obecnie automatu</param>
        /// <param name="DeathChance">Szansa na śmierć cząsteczki</param>
        /// <param name="MaxSteps">Ilość kroków po której algorytm przerwie działanie jeśli nie poprawi się najlepszy automat</param>
        /// <returns>Informacje o tym czy iteracja została wywołana dla ilości stanów większej od maksymalnej</returns>
        public static async Task<bool> Iterate(int State, int ParticlesCount,  int ProgressCount = 5, double DeathChance=0.003, int MaxSteps = 10)
        {

            if (State > MaxStates) return false;
            
            int RandomCount = ParticlesCount - ProgressCount;
          
            Particle.Initialize(MaxSteps, DeathChance);
            List<Machine> machines = new List<Machine>();

            machines.AddRange(BestMachine.GetMachinesWithMoreStates(ProgressCount, State - BestMachine.StateCount));
            for (int i = 0; i < RandomCount; i++)
            {
                machines.Add(Machine.GenerateRandomMachine(State, alphabet));
            }

            Particles.Clear();
            var generation = machines.Select(x => Task.Run(() => new Particle(x)));
            await Task.WhenAll(generation);
            Particles.AddRange(generation.Select(x =>x.Result));
            if (Particle.GlobalError < BestError)
            {
                BestMachine.stateFunction = Particle.GlobalMax;
                BestError = Particle.GlobalError;
                RaiseEvent();
            }
            while (await Step() && Particle.GlobalError != 0)
            {
                if (Particle.GlobalError < BestError)
                {
                    BestMachine.stateFunction = Particle.GlobalMax;
                    BestError = Particle.GlobalError;
                    RaiseEvent();
                }
                var tasks = Particles.Select(x => Task.Run(() =>
                {
                    return random.NextDouble() > Particle.DeathChance ? x : new Particle(Machine.GenerateRandomMachine(x.Core.StateCount, x.Core.alphabet));
                })).ToArray();
                for(int i=0; i<Particles.Count; i++)
                {
                    Particles[i] = await tasks[i];
                }
            }
            return true;
        }
        /// <summary>
        /// Metoda zwracająca informacje czy dla relacji podanej w konstruktorze dwa słowa są ze sobą w relacji
        /// </summary>
        /// <param name="list1">Słowo 1</param>
        /// <param name="list2">Słowo 2</param>
        /// <returns>Informacja czy dwa słowa są ze sobą w relacji</returns>
        public static bool AreWordsInRelation(int[] list1, int[] list2)
        {
            return relation(list1, list2);
        }
    }
}
