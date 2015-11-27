using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LanguageProcessor;
using PSO;
using TestGenerator;
using System.Threading;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for PSO.xaml
    /// </summary>
    public partial class PSO : Window, INotifyPropertyChanged
    {
        private int ParticleCount;
        int TestCount;
        public PSO(double Omega,double OmegaLocal,double OmegaGlobal,int ParticleCount,TestSets set,Machine Base,int MaxStates=15)
        {
            InitializeComponent();
            TestCount = set.TestSet.Count;
            DataContext = this;
            this.ParticleCount = ParticleCount;
            MachinePSO.Initialize(set.TestSet.Keys.ToList(), (w1, w2) => (set.TestSet[new Tuple<int[], int[]>(w1, w2)]), MaxStates, Base.alphabet);
            MachinePSO.InputParameters(Omega, OmegaLocal, OmegaGlobal);
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Collapsed;
            StartButton.IsEnabled = false;
            StartPSO();
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

        public float BestError
        {
            get { return MachinePSO.BestError == int.MaxValue ? 100 : 100f * (float)MachinePSO.BestError / TestCount; }
        }
        public float BestErrorAbsolute
        {
            get { return MachinePSO.BestError; }
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
                var letterLabel = new Label() { Content = m.alphabet[i - 1], Margin=new Thickness(4), FontStyle=FontStyles.Italic };
                g.Children.Add(letterLabel);
                Grid.SetColumn(letterLabel, i);
                Grid.SetRow(letterLabel, 0);
            }
            for (int state = 0; state < m.StateCount; state++)
                for (int letter = 0; letter < m.LetterCount; letter++)
                {
                    var label = new Label() { Content = m.stateFunction[state, letter] + 1.0, Margin = new Thickness(4) };
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
            int state = State;
            int pCount = ParticleCount;
            PropertyChanged += PSO_PropertyChanged;
            bool stillWorking = true;
            var t = Task.Factory.StartNew(async () =>
            {
                var lastError = BestError;
                while (stillWorking)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    if (BestError != lastError)
                    {
                        NotifyPropertyChanged("BestError");
                        NotifyPropertyChanged("BestErrorAbsolute");
                    }
                }
            });
            for (iteration = Task.Run(async () => await MachinePSO.Iterate(state, pCount));
                await iteration; 
                iteration = Task.Run(async () => await MachinePSO.Iterate(state, pCount)))
            {
                NotifyPropertyChanged("BestError");
                NotifyPropertyChanged("BestErrorAbsolute");
                State++;
                state = State;
                if (ClosingRequested)
                    return;
            }
            PropertyChanged -= PSO_PropertyChanged;
            stillWorking = false;
            await t;
            MessageBox.Show("Calculation finished", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PSO_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName=="BestError")
            {
                UpdateResultTable();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var msg = Task.Run(() => MessageBox.Show("Trwa zamykanie", "Message", MessageBoxButton.OK, MessageBoxImage.Information));
            var iter = Task.Run(() => { if (iteration != null) iteration.Wait(); });
            Task.WaitAll(new Task[] { msg, iter });
            ClosingRequested = true;
        }
    }
}
