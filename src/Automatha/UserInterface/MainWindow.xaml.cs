﻿using System;
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
using TestGenerator;

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
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
