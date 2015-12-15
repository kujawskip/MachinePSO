using Microsoft.VisualStudio.TestTools.UnitTesting;
using LanguageProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageProcessorTests
{
    [TestClass()]
    public class MachineTests
    {
        [TestMethod()]
        public void GettingFinishedStateFromString()
        {
            char[] set = new[] { 'a', 'b' };
            Alphabet alphabet = new Alphabet(set);
            var sf = new double[,]
            {
                { 0, 2}, 
                { 2, 0}, 
                { 1, 2}
            } ;
            var machine = new Machine(alphabet, sf);
            var ret = machine.GetFinishedState("baba");
            Assert.AreEqual(0, ret);
        }

        [TestMethod()]
        public void GettingFinishedStateFromIntList()
        {
            char[] set = new[] { 'a', 'b' };
            Alphabet alphabet = new Alphabet(set);
            var sf = new double[,]
            {
                { 0, 2},
                { 2, 0},
                { 1, 2}
            };
            var machine = new Machine(alphabet, sf);
            var list = new List<int>() {1, 0, 1, 0};
            var ret = machine.GetFinishedState(list);
            Assert.AreEqual(0, ret);
        }
        
        [TestMethod()]
        public void GetMachinesWithMoreStatesTest()
        {
            var machineCount = 3;
            char[] set = new[] { 'a', 'b' };
            Alphabet alphabet = new Alphabet(set);
            var sf = new double[,]
            {
                { 0, 2},
                { 2, 0},
                { 1, 2}
            };
            var machine = new Machine(alphabet, sf);
            var ret = machine.GetMachinesWithMoreStates(machineCount);
            Assert.AreEqual(3, ret.Count);
        }

        [TestMethod()]
        public void AreWordsInRelationArrayTest()
        {
            char[] set = new[] { 'a', 'b' };
            Alphabet alphabet = new Alphabet(set);
            var sf = new double[,]
            {
                { 0, 2},
                { 2, 0},
                { 1, 2}
            };
            var machine = new Machine(alphabet, sf);

            Assert.AreEqual(true, machine.AreWordsInRelation
                (
                new int[] { 1, 0, 1, 0, 1 },
                new int[] { 1, 0, 0 })
                );
            Assert.AreEqual(false, machine.AreWordsInRelation
                (
                new int[] {1,0,1,0,0,0,0}, 
                new int[] {1,0,1,0,1,1,1}
                )
                );
        }

        [TestMethod()]
        public void AreWordsInRelationListTest()
        {
            char[] set = new[] { 'a', 'b' };
            Alphabet alphabet = new Alphabet(set);
            var sf = new double[,]
            {
                { 0, 2},
                { 2, 0},
                { 1, 2}
            };
            var machine = new Machine(alphabet, sf);

            Assert.AreEqual(true, machine.AreWordsInRelation
                (
                new List<int>() { 1, 0, 1, 0, 1 },
                new List<int>() { 1, 0, 0 })
                );
            Assert.AreEqual(false, machine.AreWordsInRelation
                (
                new List<int>() { 1, 0, 1, 0, 0, 0, 0 },
                new List<int>() { 1, 0, 1, 0, 1, 1, 1 }
                )
                );
        }

        [TestMethod()]
        public void AreWordsInRelationStringTest()
        {
            char[] set = new[] { 'a', 'b' };
            Alphabet alphabet = new Alphabet(set);
            var sf = new double[,]
            {
                { 0, 2},
                { 2, 0},
                { 1, 2}
            };
            var machine = new Machine(alphabet, sf);

            Assert.AreEqual(true, machine.AreWordsInRelation("babab", "baa"));
            Assert.AreEqual(false, machine.AreWordsInRelation("babaaaa", "bababbb"));

        }
    }
}