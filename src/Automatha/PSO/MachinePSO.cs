using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LanguageProcessor;

namespace PSO
{
    public delegate bool LanguageRelation(int[] word1, int[] word2);
    public static class MachinePSO
    {
        public static List<Tuple<int[], int[]>> Words;
        private static LanguageRelation relation;
        public static Machine BestMachine;
        public static int BestError;
        private static Alphabet alphabet;
        private static int MaxStates;
        private static List<Particle> Particles;
        public static double Omega, OmegaLocal, OmegaGlobal;
        public static void Initialize(List<Tuple<int[], int[]>> words, LanguageRelation relation, int MaxStates, Alphabet A)
        {
            Particles = new List<Particle>();
            Words = words;
            MachinePSO.relation = relation;
            BestError = int.MaxValue;
            BestMachine = Machine.GenerateRandomMachine(1, A);
            alphabet = A;
            MachinePSO.MaxStates = MaxStates;
        }

        public static void InputParameters(double Omega, double OmegaLocal, double OmegaGlobal)
        {
            MachinePSO.Omega = Omega;
            MachinePSO.OmegaGlobal = OmegaGlobal;
            MachinePSO.OmegaLocal = OmegaLocal;
        }
        private static async Task<bool> Step()
        {
            if (!Particle.StartSteps()) return false;
            var steps = Particles.Select(p => Task.Factory.StartNew(() => p.Step())).ToArray();
            await Task.WhenAll(steps);
            var best = Particles.Find(x => x.LocalError == Particles.Min(y => y.LocalError));
            best.TryUpdateGlobal();
            //foreach (var P in Particles)
            //{
            //    P.Update();
            //}
            Particle.EndStep();
            return true;
        }

        public static async Task<bool> Iterate(int State, int ParticlesCount, int MaxSteps = 10, double ProgressRatio = 0.2)
        {

            if (State > MaxStates) return false;
            int ProgressCount = (int)(ProgressRatio * ParticlesCount);
            int RandomCount = ParticlesCount - ProgressCount;
            Particle.Initialize(MaxSteps, 0.03);
            List<Machine> machines = new List<Machine>();

            machines.AddRange(BestMachine.GetMachinesWithMoreStates(ProgressCount, State - BestMachine.StateCount));
            for (int i = 0; i < RandomCount; i++)
            {
                machines.Add(Machine.GenerateRandomMachine(State, alphabet));
            }

            Particles.Clear();
            Particles.AddRange(machines.Select(M => new Particle(M)));
            bool block = false;
            var updates = new List<Task>();
            while (await Step() && Particle.GlobalError != 0)
            {
                if (!block)
                    updates.Add(Task.Factory.StartNew(() =>
                    {
                        if (Particle.GlobalError < BestError)
                        {
                            BestMachine.stateFunction = Particle.GlobalMax;
                            BestError = Particle.GlobalError;
                        }
                        block = false;
                    }));
            }
            await Task.WhenAll(updates.ToArray());
            //if (Particle.GlobalError < BestError)
            //{
            //    BestMachine.stateFunction = Particle.GlobalMax;
            //    BestError = Particle.GlobalError;
            //    if (Particle.GlobalError == 0) return false;
            //}
        //}

            //if (Particle.GlobalError < BestError)
            //{
            //    BestMachine.stateFunction = Particle.GlobalMax;
            //    BestError = Particle.GlobalError;
            //    if (Particle.GlobalError == 0) return false;
            //}
            return true;
        }
        public static bool AreWordsInRelation(int[] list1, int[] list2)
        {
            return relation(list1, list2);
        }
    }
}
