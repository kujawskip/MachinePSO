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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LanguageProcessor;
using Microsoft.Win32;
using TestGenerator;
using System.IO;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {
        private bool _inputLoaded;

        public bool ParamethersValidated
        {
            get { return _paramethersValidated; }
            private set { _paramethersValidated = value; NotifyPropertyChanged("ParamethersValidated"); NotifyPropertyChanged("ReadyToStart"); }
        }

        private int ParticleCount;
        private double VelocityWeight, CognitiveWeight, GlobalWeight;
        private Machine Automaton;
        private TestSets Set;
        private bool _testsCreated;
        private bool _paramethersValidated;

        public bool InputLoaded
        {
            get { return _inputLoaded; }
            set { _inputLoaded = value;
                TestsCreated = false;
                NotifyPropertyChanged("InputLoaded");
            }
        }

        public bool TestsCreated
        {
            get { return _testsCreated; }
            private set { _testsCreated = value; NotifyPropertyChanged("TestsCreated"); NotifyPropertyChanged("ReadyToStart"); }
        }

        public bool ReadyToStart
        {
            get { return ParamethersValidated && TestsCreated; }
        }
        public MainWindow()
        {
            _inputLoaded = false;
            ParamethersValidated = false;
            TestsCreated = false;
            InitializeComponent();

            DataContext = this;
        }

        private void TextBoxHandler(object sender, EventArgs e)
        {
            if (ParamethersValidated) ParamethersValidated = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string errbuf;
            ParamethersValidated = ValidateParamethers(out errbuf);
            if (!ParamethersValidated)
            {
                MessageBox.Show("Błąd poczas walidowania parametrów " + errbuf, "Error");
            }
        }

        private bool ValidateParamethers(out string errbuf)
        {
            int N;
            double Weight1, Weight2, Weight3;
            bool flag = int.TryParse(ParticleCountTextBox.Text, out N);
            errbuf = "";
            if (!flag) errbuf += " Liczba cząsteczek";

            bool flag1 = double.TryParse(VelocityTextBox.Text, out Weight1);
            if (!flag1) errbuf += " Waga Prędkości";

           bool flag2 = double.TryParse(LocalTextBox.Text, out Weight2);
            if (!flag2) errbuf += " Lokalna Waga";
          bool  flag3 = double.TryParse(GlobalTextBox.Text, out Weight3);
            if (!flag3) errbuf += " Globalna Waga";
            if (!(flag1 && flag2 && flag3 && flag)) return false;
            ParticleCount = N;
            VelocityWeight = Weight1;
            CognitiveWeight = Weight2;
            GlobalWeight = Weight3;
            return true;
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InputLoaded = LoadAutomaton();
        }

        private bool LoadAutomaton()
        {

         //   throw new NotImplementedException();
            Automaton = Machine.GenerateRandomMachine(4, new Alphabet(new[] {'0', '1', '2'}));
            return true;
            OpenFileDialog of = new OpenFileDialog();
            of.Multiselect = false;
            var B = of.ShowDialog();
            if (B.HasValue && B.Value)
            {
                char[] alphabetSymbols;
                double[,] states;
                var ret = LoadAutomatonFromFile(of.FileName, out states, out alphabetSymbols);
                Automaton = new Machine(new Alphabet(alphabetSymbols), states);
                return ret;
            }
            return false;
        }

        private bool LoadAutomatonFromFile(string path, out double[,] stateFunction, out char[] alphabetSymbols)
        {
            stateFunction = null;
            alphabetSymbols = null;
            string[] linesInFile = null;
            try
            {
                linesInFile = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                return false;
            }

            int numberOfStates = int.Parse(linesInFile[0].Split(',')[0]);
            int numberOfAlphabetLetters = int.Parse(linesInFile[0].Split(',')[1]);

            alphabetSymbols = new char[numberOfAlphabetLetters];

            for (int i = 0; i < numberOfAlphabetLetters; i++)
                alphabetSymbols[i] = (char)(i + 48);


            stateFunction = new double[numberOfStates, numberOfAlphabetLetters];

            //var alphabet = new Alphabet(alphabetSymbols);
            for (int i = 1; i < linesInFile.Length; i++)
            {
                var line = linesInFile[i].Split(',');
                var state = int.Parse(line[0]);
                var input = int.Parse(line[1]);
                var outstate = int.Parse(line[2]);
                stateFunction[state, input] = outstate;
            }

            //var line = linesInFile[i].Split(',');
            //for (int i = 0; i < numberOfStates; i++)
            //{
            //    for (int j = 0; j < numberOfAlphabetLetters; j++)
            //    {
            //        stateFunction[i, j] = (double) line[i*numberOfAlphabetLetters + j  + 2][0] - 48;
            //    }
            //}


            return true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            TestsCreated = CreateTests();
        }

        private bool CreateTests()
        {
            var D = new TestRunner(Automaton);
            var b = D.ShowDialog();
            if (b.HasValue && b.Value)
            {
                Set = D.Set;
                return true;
            }
            return false;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            StartPSO();
        }

        private void StartPSO()
        {
            PSO pso = new PSO(VelocityWeight, CognitiveWeight, GlobalWeight, ParticleCount, Set, Automaton);
            pso.ShowDialog();
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
