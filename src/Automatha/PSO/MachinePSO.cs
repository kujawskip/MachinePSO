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
        private static Machine BestMachine;
        private static int BestError;
        private static Alphabet alphabet;
        private static int MaxStates;
        private static List<Particle> Particles; 
        public static double Omega, OmegaLocal, OmegaGlobal;
        public static void Initialize(List<Tuple<int[],int[]>> words, LanguageRelation relation,int MaxStates,Alphabet A)
        {
            Words = words;
            MachinePSO.relation = relation;
            BestError = int.MaxValue;
            BestMachine = Machine.GenerateRandomMachine(2,A);
            alphabet = A;
            MachinePSO.MaxStates = MaxStates;
        }

        public static void InputParameters(double Omega, double OmegaLocal, double OmegaGlobal)
        {
            MachinePSO.Omega = Omega;
            MachinePSO.OmegaGlobal = OmegaGlobal;
            MachinePSO.OmegaLocal = OmegaLocal;
        }
        private static bool Step()
        {
            if (!Particle.StartSteps()) return false;
            foreach (var P in Particles)
            {
                P.Step();
                
            }
            foreach (var P in Particles)
            {
                P.Update();
            }
            Particle.EndStep();
            return true;
        }

        public static bool Iterate(int State,int ParticlesCount,int MaxSteps=10,double ProgressRatio =0.2)
        {

            if (State > MaxStates) return false;
            int ProgressCount = (int) (ProgressRatio*ParticlesCount);
            int RandomCount = ParticlesCount - ProgressCount;
            Particle.Initialize(MaxSteps);
            List<Machine> machines = new List<Machine>();
            if (BestMachine == null)
            {
                for (int i = 0; i < RandomCount + ProgressCount; i++)
                {
                  machines.Add(Machine.GenerateRandomMachine(State,alphabet));   
                }
            }
            else
            {
                machines.AddRange(BestMachine.GetMachinesWithMoreStates(ProgressCount,State-BestMachine.StateCount));
                for (int i = 0; i < RandomCount; i++)
                {
                    machines.Add(Machine.GenerateRandomMachine(State, alphabet));
                }
            }
            Particles.Clear();
            Particles.AddRange(machines.Select(M => new Particle(M)));
            while (Step())
            {
                if (Particle.GlobalError==0)
                {
                    break;
                }
            }
            if (Particle.GlobalError < BestError)
            {
                BestMachine.stateFunction = Particle.GlobalMax;
                BestError = Particle.GlobalError;
                if (Particle.GlobalError == 0) return false;
            }
            return true;
        }
        public static bool AreWordsInRelation(int[] list1, int[] list2)
        {
            return relation(list1, list2);
        }
    }
}
