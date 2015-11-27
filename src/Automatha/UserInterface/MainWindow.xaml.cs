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

        private int ParticleCount,ProgressCount;
        private double VelocityWeight, CognitiveWeight, GlobalWeight;
        private Machine Automaton;
        private TestSets Set;
        private double DeathChance;
        private bool _testsCreated;
        private bool _paramethersValidated;
        private int MaxStates;
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
            double deathCount;
            int progress, maxstates;
            bool flag = int.TryParse(ParticleCountTextBox.Text, out N);
            errbuf = "";
            if (!flag) errbuf += " Liczba cząsteczek";

            bool flag1 = double.TryParse(VelocityTextBox.Text, out Weight1);
            if (!flag1) errbuf += " Waga Prędkości";

           bool flag2 = double.TryParse(LocalTextBox.Text, out Weight2);
            if (!flag2) errbuf += " Lokalna Waga";
          bool  flag3 = double.TryParse(GlobalTextBox.Text, out Weight3);
            if (!flag3) errbuf += " Globalna Waga";
            bool flag4 = double.TryParse(DeathTextBox.Text, out deathCount);
            if (!flag4) errbuf += " Szansa na śmierć cząsteczki";
            bool flag5 = int.TryParse(ProgressionTextBox.Text, out progress);
            if (!flag5) errbuf += " Cząstki przekazane do następnej iteracji";
            bool flag6 = int.TryParse(MaxStateTextBox.Text, out maxstates);
            if (!flag6) errbuf += " Cząstki przekazane do następnej iteracji";
            if (!(flag1 && flag2 && flag3 && flag&&flag5&&flag6&&flag4)) return false;
            ParticleCount = N;
            VelocityWeight = Weight1;
            CognitiveWeight = Weight2;
            GlobalWeight = Weight3;
            ProgressCount = progress;
            MaxStates = maxstates;
            DeathChance = deathCount;
            
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
            linesInFile = linesInFile.Select(x => x.Replace(" ", "")).ToArray();
            int numberOfStates = int.Parse(linesInFile[0].Split(',')[0]);
            int numberOfAlphabetLetters = int.Parse(linesInFile[0].Split(',')[1]);

            alphabetSymbols = new char[numberOfAlphabetLetters];

            for (int i = 0; i < numberOfAlphabetLetters; i++)
                alphabetSymbols[i] = (char)(i + 48);


            stateFunction = new double[numberOfStates, numberOfAlphabetLetters];
            List<string> Numerals = new List<string>();
            for (int i = 1; i < linesInFile.Length; i++)
            {
                Numerals.AddRange(linesInFile[i].Split(','));
            }
            //var alphabet = new Alphabet(alphabetSymbols);
            for (int i = 0; i < Numerals.Count; i+=3)
            {
                
                var state = int.Parse(Numerals[i]) - 1;
                var input = int.Parse(Numerals[i+1]) - 1;
                var outstate = int.Parse(Numerals[i+2]) - 1;
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
            PSO pso = new PSO(VelocityWeight, CognitiveWeight, GlobalWeight, ParticleCount, Set, Automaton,MaxStates,DeathChance,ProgressCount);
            pso.ShowDialog();
            //pso.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void DeathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
