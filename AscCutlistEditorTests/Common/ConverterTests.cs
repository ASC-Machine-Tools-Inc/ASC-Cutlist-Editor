using System;
using System.Globalization;
using System.Windows.Media;
using AscCutlistEditor.Utility.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.Common
{
    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void BoolRadioConverterTest()
        {
            // Arrange
            BoolRadioConverter converter = new BoolRadioConverter
            {
                Inverse = true
            };

            // Assert
            Assert.IsFalse(BoolRadioConvertHelper(converter, true));
            Assert.IsTrue(BoolRadioConvertHelper(converter, false));

            Assert.IsFalse((bool)BoolRadioConvertBackHelper(converter, true));
            // We expect null, since we only pay attention to selected radio buttons.
            Assert.IsNull(BoolRadioConvertBackHelper(converter, false));

            converter.Inverse = false;

            Assert.IsTrue(BoolRadioConvertHelper(converter, true));
            Assert.IsFalse(BoolRadioConvertHelper(converter, false));
        }

        private bool BoolRadioConvertHelper(BoolRadioConverter converter, bool value)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (bool)converter.Convert(value, typeof(bool), null, CultureInfo.CurrentCulture);
        }

        private object BoolRadioConvertBackHelper(BoolRadioConverter converter, bool value)
        {
            // ReSharper disable once PossibleNullReferenceException
            return converter.ConvertBack(value, typeof(bool), null, CultureInfo.CurrentCulture);
        }

        [TestMethod]
        public void LineRunningConverterTest()
        {
            // Arrange
            LineRunningColorConverter converter = new LineRunningColorConverter();

            // Assert
            Assert.AreEqual(LineRunningConvertHelper(converter, "LINE RUNNING"), Brushes.Green);
            Assert.AreEqual(LineRunningConvertHelper(converter, "LINE STOPPED"), Brushes.Red);
            Assert.AreEqual(LineRunningConvertHelper(converter, "INVALID VALUE"), Brushes.Transparent);
        }

        private Brush LineRunningConvertHelper(LineRunningColorConverter converter, string value)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (Brush)converter.Convert(value, typeof(Brush), null, CultureInfo.CurrentCulture);
        }
    }
}