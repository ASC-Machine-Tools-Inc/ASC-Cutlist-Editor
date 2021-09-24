using System.Globalization;
using AscCutlistEditor.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media;

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

        [TestMethod]
        [DataRow(0, 100)]
        [DataRow(3, 97)]
        [DataRow(47.0, 53.0)]
        [DataRow(47.986, 52.014)]
        [DataRow(100, 0)]
        [DataRow(103, -3)]
        public void InversePercentageConverterTest(double input, double output)
        {
            InversePercentageConverter converter = new InversePercentageConverter();

            object converted = converter.Convert(input, typeof(double), null, CultureInfo.DefaultThreadCurrentCulture);
            Assert.AreEqual(converted, output);

            object reverted = converter.ConvertBack(converted, typeof(double), null, CultureInfo.DefaultThreadCurrentCulture);
            Assert.AreEqual(reverted, input);
        }
    }
}