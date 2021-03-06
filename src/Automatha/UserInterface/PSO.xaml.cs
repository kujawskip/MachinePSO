﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LanguageProcessor;
using PSO;
using TestGenerator;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for PSO.xaml
    /// </summary>
    public partial class PSO : Window, INotifyPropertyChanged
    {
        private int particleCount;
        int TestCount;
        private int ProgressCount;
        private double DeathChance;
        TestSets set;
        private Machine Base;
        private Grid machine1, machine2;
        public int ParticleCount
        {
            get { return particleCount; }
        }
        public PSO(double Omega, double OmegaLocal, double OmegaGlobal, int ParticleCount, TestSets set, 
            Machine Base, int MaxStates=15, double DeathChance=0.003, int ProgressCount = 5)
        {
            this.ProgressCount = ProgressCount;
            this.DeathChance = DeathChance;
            this.Base = Base;
            machine2 = GenerateMachineRepresentation(Base);
            this.particleCount = ParticleCount;
            InitializeComponent();
            TestCount = set.TrainingSet.Count;
            DataContext = this;
            this.set = set;
            MachinePSO.Initialize(set.TrainingSet.Keys.ToList(), (w1, w2) => (set.TrainingSet[new Tuple<int[], int[]>(w1, w2)]), MaxStates, Base.alphabet, set.AllWords);
            MachinePSO.InputParameters(Omega, OmegaLocal, OmegaGlobal);
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StartPSO_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Collapsed;
            StartButton.IsEnabled = false;
            StartPSO();
        }

        Visibility logVisible = Visibility.Hidden;
        public Visibility LogVisible
        {
            get { return logVisible; }
            set
            {
                logVisible = value;
                NotifyPropertyChanged("LogVisible");
            }
        }

        public int State
        {
            get { return state; }
            set
            {
                state = value;
                NotifyPropertyChanged("State");
            }
        }

        public float TrainingSetError
        {
            get { return TrainingSetAbsoluteError == int.MaxValue ? 100 : 100f * (float)TrainingSetAbsoluteError / TestCount; }
        }
        private float _trainingSetAbsoluteErrorAbs;
        public float TrainingSetAbsoluteError
        {
            get { return _trainingSetAbsoluteErrorAbs; }
            set
            {
                _trainingSetAbsoluteErrorAbs = value;
                NotifyPropertyChanged("TrainingSetError");
                NotifyPropertyChanged("TrainingSetAbsoluteError");
            }
        }

        private string _trainingSetShortError = "";
        public string TrainingSetShortError
        {
            get { return _trainingSetShortError; }
            set
            {
                _trainingSetShortError = value;
                NotifyPropertyChanged("TrainingSetShortError");
            }
        }
        private string _trainingSetShortAbsoluteError = "";
        public string TrainingSetShortAbsoluteError
        {
            get { return _trainingSetShortAbsoluteError; }
            set
            {
                _trainingSetShortAbsoluteError = value;
                NotifyPropertyChanged("TrainingSetShortAbsoluteError");
            }
        }

        private string _trainingSetLongError = "";
        public string TrainingSetLongError
        {
            get { return _trainingSetLongError; }
            set
            {
                _trainingSetLongError = value;
                NotifyPropertyChanged("TrainingSetLongError");
            }
        }
        private string _trainingSetLongAbsoluteError = "";
        public string TrainingSetLongAbsoluteError
        {
            get { return _trainingSetLongAbsoluteError; }
            set
            {
                _trainingSetLongAbsoluteError = value;
                NotifyPropertyChanged("TrainingSetLongAbsoluteError");
            }
        }

        private string _testSetError = "";
        public string TestSetError
        {
            get { return _testSetError; }
            set
            {
                _testSetError = value;
                NotifyPropertyChanged("TestSetError");
            }
        }

        private string _testSetAbsoluteErrorAbsCg = "";
        public string TestSetAbsoluteError
        {
            get { return _testSetAbsoluteErrorAbsCg; }
            set
            {
                _testSetAbsoluteErrorAbsCg = value;
                NotifyPropertyChanged("TestSetAbsoluteError");
            }
        }


        private int state = 2;

        public void UpdateResultTable()
        {
            ModifableGrid.Children.Clear();
            ModifableGrid.Children.Add(GenerateMachineRepresentation(MachinePSO.BestMachine));
        }

        public Grid GenerateMachineRepresentation(Machine m)
        {
            var g = new Grid();
            for(int i=0; i<=m.StateCount; i++)
            {
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                if (i == 0) continue;
                var stateLabel = new Label() { Content = i, Margin = new Thickness(4), FontWeight=FontWeights.Bold };
                g.Children.Add(stateLabel);
                Grid.SetColumn(stateLabel, 0);
                Grid.SetRow(stateLabel, i);
            }
            for (int i = 0; i <= m.LetterCount; i++)
            {
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                if (i == 0) continue;
                var letterLabel = new Label() { Content = m.alphabet[i - 1], Margin = new Thickness(4), FontStyle = FontStyles.Italic };
                g.Children.Add(letterLabel);
                Grid.SetColumn(letterLabel, i);
                Grid.SetRow(letterLabel, 0);
            }
            for (int state = 0; state < m.StateCount; state++)
                for (int letter = 0; letter < m.LetterCount; letter++)
                {
                    var label = new Label() { Content = (int)m.stateFunction[state, letter] + 1.0, Margin = new Thickness(4) };
                    g.Children.Add(label);
                    Grid.SetColumn(label, letter + 1);
                    Grid.SetRow(label, state + 1);
                }
            return g;
        }

        bool ClosingRequested = false;
        Task<bool> iteration;
        private async void StartPSO()
        {
            MachinePSO.Stop = false;
            int state = State;
            int pCount = particleCount;

            DateTime StartTime = DateTime.Now;
            TrainingSetAbsoluteError = TestCount;
            MachinePSO.BestErrorChanged += MachinePSO_BestErrorChanged;
            for (iteration = MachinePSO.Iterate(state, pCount, ProgressCount, DeathChance);
                await iteration;
                iteration = MachinePSO.Iterate(state, pCount, ProgressCount, DeathChance))

            {
                State++;
                state = State;
                if (ClosingRequested)
                {
                    MachinePSO.BestErrorChanged -= MachinePSO_BestErrorChanged;
                    return;
                }
            }

            MachinePSO.BestErrorChanged -= MachinePSO_BestErrorChanged;
            DateTime EndTime = DateTime.Now;
            MessageBox.Show("Calculation finished " + (EndTime - StartTime).ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Information);

            LogVisible = Visibility.Visible;
            double per;
            int err = MachinePSO.PerformTest(set.TestSet, out per);
            TestSetError = per.ToString();
            TestSetAbsoluteError = err.ToString();

            double trainingSetShortError, trainingSetLongError;
            int trainingSetShortAbsoluteError, trainingSetLongAbsoluteError;
            MachinePSO.PerformTrainingTest(set.ShortWordMaxLength, set.TrainingSet, out trainingSetShortError, out trainingSetLongError,
                out trainingSetShortAbsoluteError, out trainingSetLongAbsoluteError);
            TrainingSetShortError = trainingSetShortError.ToString();
            TrainingSetLongError = trainingSetLongError.ToString();
            TrainingSetShortAbsoluteError = trainingSetShortAbsoluteError.ToString();
            TrainingSetLongAbsoluteError = trainingSetLongAbsoluteError.ToString();
        }

        private void MachinePSO_BestErrorChanged(object sender, EventArgs e)
        {
            TrainingSetAbsoluteError = MachinePSO.BestError;
            UpdateResultTable();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ClosingRequested = true;
            var msg = Task.Run(() => MessageBox.Show("Trwa zamykanie", "Message", MessageBoxButton.OK, MessageBoxImage.Information));
            var iter = Task.Run(() => { MachinePSO.Stop = true; if (iteration != null) iteration.Wait(); });
            Task.WaitAll(new Task[] { msg, iter });
        }

        private void CreateLog_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog of = new SaveFileDialog { Filter = "Pliki testów (*.txt) | *.txt" };
            var B = of.ShowDialog();
            if (B.HasValue && B.Value)
            {
                CreateLog(of.FileName);
            }
        }

        private void CreateLog(string fileName)
        {
            try
            {
                using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    var sb = new StringBuilder();
                    foreach (var keyVal in MachinePSO.Words)
                    {
                        var w1 = MachinePSO.BestMachine.alphabet.Translate(keyVal.Item1.ToList());
                        var w2 = MachinePSO.BestMachine.alphabet.Translate(keyVal.Item2.ToList());
                        var rel1 = MachinePSO.BestMachine.AreWordsInRelation(keyVal.Item1, keyVal.Item2);
                        var rel2 = MachinePSO.AreWordsInRelation(keyVal.Item1, keyVal.Item2);
                        sb.AppendFormat("{0}Obliczone {1}\n--{2}\n--{3}\n\n", rel1 == rel2 ? "" : ">", rel1 == rel2 ? "dobrze" : "źle", w1, w2);
                    }
                    //sb.AppendFormat("{0}\n--{1}\n--{2}\n\n");
                    writer.Write(sb.ToString());

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void CompareMachines_Click(object sender, RoutedEventArgs e)
        {
            if (machine1 == null)
            {
                var M = MachinePSO.BestMachine;
                var darray = M.stateFunction;
                darray = RemoveUnAccessible(darray);
                darray = ReorderAccordingTo(darray,Base.stateFunction);
                machine1 = GenerateMachineRepresentation(new Machine(Base.alphabet, darray));
            }
            MachineComparer mc = new MachineComparer(machine2,machine1);
            mc.ShowDialog();
        }
        private int GetMinEdge(int a, double[,] d)
        {
            int r = d.GetLength(0)+2;
            for (int i = 0; i < d.GetLength(1); i++) if (((int)d[a, i]) < r) r = (int)d[a, i];
            return r;
        }
        private int GetMaxEdge(int a, double[,] d)
        {
            int r = 0;
            for (int i = 0; i < d.GetLength(1); i++) if (((int)d[a, i]) > r) r=(int)d[a,i];
            return r;
        }
        private int GetSmallestRepetitions(int a, double[,] d)
        {
            int[] t = new int[d.GetLength(0)];
            for (int i = 0; i < d.GetLength(0); i++) t[i] = 0;
            for (int i = 0; i < d.GetLength(1); i++) t[(int)d[a, i]]++;
            return t.Where(s=>s!=0).Min();
        }
        private int GetSelfDeegree(int a, double[,] d)
        {
            int r = 0;
            for(int i=0;i<d.GetLength(1);i++) if (((int) d[a, i]) == a) r++;
            return r;
        }
        private int GetBiggestRepetitions(int a, double[,] d)
        {
            int[] t = new int[d.GetLength(0)];
            for (int i = 0; i < d.GetLength(0); i++) t[i] = 0;
            for (int i = 0; i < d.GetLength(1); i++) t[(int) d[a, i]]++;
            return t.Max();
        }
        private int GetDeegree(int a, double[,] d)
        {
            List<int> L = new List<int>();
            for (int i = 0; i < d.GetLength(1); i++)
            {
                int t = (int)d[a, i];
                if (!L.Contains(t)) L.Add(t);
            }
            return L.Count;
        }
        private int CompareForSorting(int a, int b, double[,] d)
        {
            List<System.Func<int,double[,],int>> FL = new List<Func<int, double[,], int>>();
            FL.AddRange(new Func<int,double[,],int> []{GetDeegree,GetSelfDeegree,GetBiggestRepetitions,GetSmallestRepetitions,GetMaxEdge,GetMinEdge});
            return FL.Select(F => F(a, d).CompareTo(F(b, d))).FirstOrDefault(k => k != 0);
        }

        private void Swap(ref double[,] d, int a, int b)
        {
            for (int i = 0; i < d.GetLength(1); i++)
            {
               var t = d[a, i];
                d[a, i] = d[b, i];
                d[b, i] = t;
            }
        }
        private double[,] ReorderAccordingTo(double[,] darray, double[,] p,int times=3)
        {
            for (int K = 0; K < times; K++)
            {
                int n = darray.GetLength(0);
                List<int> L = new List<int>();
                for (int i = 1; i < darray.GetLength(0); i++) L.Add(i);
                List<int> La = new List<int>();
                for (int i = 1; i < p.GetLength(0); i++) La.Add(i);

                La.Sort((s, t) => (CompareForSorting(s, t, p)));
                L.Sort((s, t) => (CompareForSorting(s, t, darray)));
                La.Insert(0, 0);
                L.Insert(0, 0);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < darray.GetLength(1); j++)
                    {
                        int t = (int) darray[i, j];
                        int k = L.IndexOf(t);
                        if (k < La.Count)
                        {
                            darray[i, j] = La[k];
                        }
                    }
                }
                for (int i = 1; i < Math.Min(La.Count,L.Count); i++)
                {
                    Swap(ref darray,L[i],La[i]);
                }
            }
            return darray;
        }

        private double[,] RemoveUnAccessible(double[,] darray)
        {
            List<int> Current = new List<int>();
            Current.Add(0);
            List<int> Accessible = new List<int>();
            Accessible.Add(0);
            while (Current.Count > 0)
            {
                var I = Current[0];
                Current.RemoveAt(0);
                for (int i = 0; i < Base.alphabet.Letters.Length; i++)
                {
                    var J = (int) darray[I, i];
                    if (!Accessible.Contains(J) && !Current.Contains(J))
                    {
                        Accessible.Add(J);
                        Current.Add(J);
                    }
                }
            }
            Accessible.Sort();
            double[,] result = new double[Accessible.Count,Base.alphabet.Letters.Length];
            {
                for (int i = 0; i < Accessible.Count; i++)
                {
                    for (int j = 0; j < Base.alphabet.Letters.Length;j++)
                    {
                        result[i, j] = Accessible.IndexOf((int)darray[Accessible[i], j]);
                    }
                }
            }
            return result;
        }
    }
}
