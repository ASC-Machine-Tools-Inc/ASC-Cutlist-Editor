using AscCutlistEditor.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media;
using AscCutlistEditor.Utility.Helpers;

namespace AscCutlistEditorTests.Utility
{
    [TestClass]
    public class HelpersTests
    {
        [TestMethod]
        public void LengthToColorTest()
        {
            // Arrange
            double length1 = 600.504;
            double length2 = 600.504;
            double length3 = 123.456;

            // Act
            Brush brush1 = Helpers.LengthToColor(length1);
            Brush brush2 = Helpers.LengthToColor(length2);
            Brush brush3 = Helpers.LengthToColor(length3);

            // Assert
            Assert.AreEqual(brush1, brush2);
            Assert.AreNotEqual(brush1, brush3);
            Assert.AreNotEqual(brush2, brush3);
        }
    }
}