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
            get { return MachinePSO.BestError==int.MaxValue? 100 : 100*(float)MachinePSO.BestError/(float)TestCount; }
        }
        private int state = 2;

        bool ClosingRequested = false;
        private async void StartPSO()
        {
            int state = State;
            int pCount = ParticleCount;
            //bool inProgress = true;
            //Task update = Task.Factory.StartNew(async () =>
            //{
            //    while (inProgress)
            //    {
            //        await Task.Delay(TimeSpan.FromSeconds(1));
            //        NotifyPropertyChanged("BestError");
            //    }
            //    NotifyPropertyChanged("BestError");
            //});
            while (await Task.Run(async () => await MachinePSO.Iterate(state, pCount)))
            {
                NotifyPropertyChanged("BestError");
                State++;
                state = State;
                if (ClosingRequested)
                    return;
            }
            //inProgress = false;
            //await update;
            MessageBox.Show("Calculation finished", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ClosingRequested = true;
        }
    }
}
