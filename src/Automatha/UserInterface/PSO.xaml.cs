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

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for PSO.xaml
    /// </summary>
    public partial class PSO : Window, INotifyPropertyChanged
    {
        private int ParticleCount;
        public PSO(double Omega,double OmegaLocal,double OmegaGlobal,int ParticleCount,TestSets set,Machine Base,int MaxStates=15)
        {
            InitializeComponent();
            DataContext = this;
            this.ParticleCount = ParticleCount;
            MachinePSO.Initialize(set.TestSet.Keys.ToList(),(w1,w2)=>(set.TestSet[new Tuple<int[],int[]>(w1,w2)]),MaxStates,Base.alphabet);
            MachinePSO.InputParameters(Omega,OmegaLocal,OmegaGlobal);
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

        public int BestError
        {
            get { return MachinePSO.BestError; }
        }
        private int state = 2;
        private void StartPSO()
        {
            



                while (MachinePSO.Iterate(state, ParticleCount))
                {
                    State++;
                    NotifyPropertyChanged("BestError");
                    
                    //TO DO: Wyswietlenie nowego automatu
                }
       
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
