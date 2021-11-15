using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AscCutlistEditor.Utility;
using AscCutlistEditor.Utility.Helpers;

namespace AscCutlistEditorTests.Common
{
    [TestClass]
    public class LengthParserTests
    {
        [TestMethod]
        public void ParseValidInchTest()
        {
            Assert.AreEqual(Helpers.ParseString("42"), 42);
            Assert.AreEqual(Helpers.ParseString("42.0"), 42);
            Assert.AreEqual(Helpers.ParseString("42.25785"), 42.25785);
            Assert.AreEqual(Helpers.ParseString("-42.3"), -42.3);
        }

        [TestMethod]
        public void ParseInvalidInchTest()
        {
            Assert.AreNotEqual(Helpers.ParseString("42"), 42.5);
            Assert.AreNotEqual(Helpers.ParseString("39.999"), 40);
            Assert.AreNotEqual(Helpers.ParseString("-1"), 1);
        }

        [TestMethod]
        public void ParseValidMillimeterTest()
        {
            Assert.AreEqual(Math.Round(Helpers.ParseString("333mm"), 2), 13.11);
            Assert.AreEqual(Helpers.ParseString("10160mm"), 400);
        }

        [TestMethod]
        public void ParseInvalidMillimeterTest()
        {
            Assert.AreNotEqual(Helpers.ParseString("333mm"), 13.11);
            Assert.AreNotEqual(Helpers.ParseString("10160mm"), 10160);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ParseBadFormatTest()
        {
            Helpers.ParseString("333nn");
        }
    }
}