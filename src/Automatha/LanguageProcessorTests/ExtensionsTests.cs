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
    public class ExtensionsTests
    {
        [TestMethod()]
        public void RoundingDoubleArray()
        {
            var tab = new double[,]
            {
                {1.2, 3.4},
                {5.6, 7.8}
            };
            var ret = tab.Round();
            Assert.AreEqual(1, ret[0, 0]);
            Assert.AreEqual(3, ret[0, 1]);
            Assert.AreEqual(5, ret[1, 0]);
            Assert.AreEqual(7, ret[1, 1]);
        }

        [TestMethod()]
        public void RoundingFloatArray()
        {
            var tab = new float[,]
            {
                {1.2f, 3.4f},
                {5.6f, 7.8f}
            };
            var ret = tab.Round();
            Assert.AreEqual(1, ret[0, 0]);
            Assert.AreEqual(3, ret[0, 1]);
            Assert.AreEqual(5, ret[1, 0]);
            Assert.AreEqual(7, ret[1, 1]);
        }
    }
}