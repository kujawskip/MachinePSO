using System;
using System.Collections.Generic;
using LanguageProcessor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LanguageProcessorTests
{
    [TestClass]
    public class AlphabetTests
    {
        [TestMethod]
        public void AlphabetTranslatingCharsToInts()
        {
            char[] set = new[] { 'a', 'b', 'c', 'd' };
            Alphabet alphabet = new Alphabet(set);

            var list = alphabet.Translate("baca");
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(0, list[1]);
            Assert.AreEqual(2, list[2]);
            Assert.AreEqual(0, list[3]);
        }

        [TestMethod]
        public void AlphabetTranslatingIntsToChars()
        {
            char[] set = new[] { 'a', 'b', 'c', 'd' };
            Alphabet alphabet = new Alphabet(set);

           
            List<int> list = new List<int>() {0, 2, 1, 0, 2};
            var s = alphabet.Translate(list);

            Assert.AreEqual("acbac", s);
        }
    }
}
