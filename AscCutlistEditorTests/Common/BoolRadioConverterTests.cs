using System;
using System.Globalization;
using AscCutlistEditor.Utility.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.Common
{
    [TestClass]
    public class BoolRadioConverterTests
    {
        [TestMethod]
        public void ConvertTest()
        {
            // Arrange
            BoolRadioConverter converter = new BoolRadioConverter();
            converter.Inverse = true;

            // Assert
            Assert.IsFalse(ConvertHelper(converter, true));
            Assert.IsTrue(ConvertHelper(converter, false));

            Assert.IsFalse((bool)ConvertBackHelper(converter, true));
            // We expect null, since we only pay attention to selected radio buttons.
            Assert.IsNull(ConvertBackHelper(converter, false));

            converter.Inverse = false;

            Assert.IsTrue(ConvertHelper(converter, true));
            Assert.IsFalse(ConvertHelper(converter, false));
        }

        private bool ConvertHelper(BoolRadioConverter converter, bool value)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (bool)converter.Convert(value, typeof(bool), null, CultureInfo.CurrentCulture);
        }

        private object ConvertBackHelper(BoolRadioConverter converter, bool value)
        {
            // ReSharper disable once PossibleNullReferenceException
            return converter.ConvertBack(value, typeof(bool), null, CultureInfo.CurrentCulture);
        }
    }
}