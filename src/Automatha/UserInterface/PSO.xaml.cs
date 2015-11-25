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
            this.ParticleCount = ParticleCount;
            MachinePSO.Initialize(set.TestSet.Keys.ToList(),(w1,w2)=>(set.TestSet[new Tuple<int[],int[]>(w1,w2)]),MaxStates,Base.alphabet);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Collapsed;
            StartButton.IsEnabled = false;
            StartPSO();
        }

        private int State = 2;
        private void StartPSO()
        {
            while (MachinePSO.Iterate(State, ParticleCount))
            {
                State++;

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
