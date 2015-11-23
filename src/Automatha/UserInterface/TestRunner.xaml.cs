﻿using System;
using System.Collections.Generic;
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
using Microsoft.Win32;
using TestGenerator;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for TestRunner.xaml
    /// </summary>
    public partial class TestRunner : Window
    {
        public TestRunner(Machine Automaton)
        {
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

        private void Button_Click(object sender, RoutedEventArgs e)
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
            if (!flag) errbuf += " Losowe testy";
            bool flag1 = int.TryParse(ControlTestBox.Text, out M);
            if (!flag1) errbuf += " Dokładne testy";
            bool flag2 = int.TryParse(ThoroughTestTextBox.Text, out P);
            if (!flag2) errbuf += " Kontrolne testy";
            if (!(flag1 && flag2 && flag)) return false;
            random = N;
            control = M;
            thorough = P;

            return true;
        }

        public TestSets Set
        {
            get { return set; }
        }

        private void TextBoxHandler(object sender, EventArgs e)
        {
            ValidatedParameters = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog of = new SaveFileDialog();

            of.Filter = "Pliki testów (.tst) | .tst";
            var B = of.ShowDialog();
            if (B.HasValue && B.Value)
            {
                SaveSetToFile(of.FileName);
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Multiselect = false;
            of.Filter = "Pliki testów (.tst) | .tst";
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
                set = new TestSets(Automaton, thorough, random, control);
            }
            else
            {
                set = new TestSets(Automaton);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (!ValidatedParameters)
            {
                string sMessageBoxText = "Generowanie zostanie uruchomione z bazowymi parametrami - kontynuować?";
                string sCaption = "Ostrzeżenie";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox,
                    icnMessageBox);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:

                        break;

                    case MessageBoxResult.No:
                        return;



                }

                GenerateTests();
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}