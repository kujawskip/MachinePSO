using System;
using System.ComponentModel;
using System.Windows;
using LanguageProcessor;
using Microsoft.Win32;
using TestGenerator;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for TestRunner.xaml
    /// </summary>
    public partial class TestRunner : Window, INotifyPropertyChanged
    {
        public TestRunner(Machine Automaton)
        {
            DataContext = this;
            InitializeComponent();
            this.Automaton = Automaton;
        }

        private void LoadSetFromFile(string path)
        {
            bool flag = TestSets.LoadSetFromFile(path, Automaton, out set);

            if (!flag)
            {
                MessageBox.Show("Nie udało się wczytać danych z pliku", "Error");
            }
            else NotifyPropertyChanged("TestsReady");
        }

        private void SaveSetToFile(string path)
        {
            bool flag = Set.SaveSetToFile(path);
            if (!flag)
            {
                MessageBox.Show("Nie udało się zapisać danych do pliku", "Error");
            }
        }

        private int random, thorough, control;
        private Machine Automaton;

        public bool TestsReady
        {
            get { return Set != null; }
        }

        

    public bool ValidatedParameters { get; private set; }
        private TestSets set;

        private void Button_AcceptParams(object sender, RoutedEventArgs e)
        {
            string errbuf;
            ValidatedParameters = ValidateParamethers(out errbuf);
            if (!ValidatedParameters)
            {
                MessageBox.Show("Błąd poczas walidowania parametrów " + errbuf, "Error");
            }
        }

        private bool ValidateParamethers(out string errbuf)
        {
            int N, M, P;

            errbuf = "";
            bool flag = int.TryParse(RandomTestTextBox.Text, out N);
            if (!flag) errbuf += " Liczba długich słów";
            bool flag1 = int.TryParse(ControlTestBox.Text, out M);
            if (!flag1) errbuf += " Rozmiar zbioru testowego";
            bool flag2 = int.TryParse(ThoroughTestTextBox.Text, out P);
            if (!flag2) errbuf += " Maksymalna długość krótkiego słowa";
            if (!(flag1 && flag2 && flag)) return false;
            random = N;
            control = M;
            thorough = P;

            return true;
        }

        public TestSets Set
        {
            get { return set; }
            set
            {
                set = value;
                NotifyPropertyChanged("TestsReady");
            }
        }

        private void TextBoxHandler(object sender, EventArgs e)
        {
            ValidatedParameters = false;
        }

        private void Button_SaveParams(object sender, RoutedEventArgs e)
        {
            SaveFileDialog of = new SaveFileDialog {Filter = "Pliki testów (*.tst) | *.tst"};

            var B = of.ShowDialog();
            if (B.HasValue && B.Value)
            {
                SaveSetToFile(of.FileName);
            }
        }


        private void Button_LoadParams(object sender, RoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Pliki testów (*.tst) | *.tst"
            };
            var B = of.ShowDialog();
            if (B.HasValue && B.Value)
            {
                LoadSetFromFile(of.FileName);
            }
        }

        private void GenerateTests()
        {
            if (ValidatedParameters)
            {
                Set = new TestSets(Automaton, thorough, random, control);
            }
            else
            {
                Set = new TestSets(Automaton);
            }
        }

        private void Button_Generate(object sender, RoutedEventArgs e)
        {
            if (!ValidatedParameters)
            {
                string sMessageBoxText = "Generowanie zostanie uruchomione z bazowymi parametrami - kontynuować?";
                string sCaption = "Ostrzeżenie";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        break;
                    case MessageBoxResult.No:
                        return;
                }
            }
            GenerateTests();
        }

        private void Button_AcceptGenerated(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            GC.Collect();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
