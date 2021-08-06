using AscCutlistEditor.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AscCutlistEditorTests.Common
{
    [TestClass]
    public class LengthParserTests
    {
        [TestMethod]
        public void ParseValidInchTest()
        {
            Assert.AreEqual(LengthParser.ParseString("42"), 42);
            Assert.AreEqual(LengthParser.ParseString("42.0"), 42);
            Assert.AreEqual(LengthParser.ParseString("42.25785"), 42.25785);
            Assert.AreEqual(LengthParser.ParseString("-42.3"), -42.3);
        }

        [TestMethod]
        public void ParseInvalidInchTest()
        {
            Assert.AreNotEqual(LengthParser.ParseString("42"), 42.5);
            Assert.AreNotEqual(LengthParser.ParseString("39.999"), 40);
            Assert.AreNotEqual(LengthParser.ParseString("-1"), 1);
        }

        [TestMethod]
        public void ParseValidMillimeterTest()
        {
            Assert.AreEqual(Math.Round(LengthParser.ParseString("333mm"), 2), 13.11);
            Assert.AreEqual(LengthParser.ParseString("10160mm"), 400);
        }

        [TestMethod]
        public void ParseInvalidMillimeterTest()
        {
            Assert.AreNotEqual(LengthParser.ParseString("333mm"), 13.11);
            Assert.AreNotEqual(LengthParser.ParseString("10160mm"), 10160);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ParseBadFormatTest()
        {
            LengthParser.ParseString("333nn");
        }
    }
}