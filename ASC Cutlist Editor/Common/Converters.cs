using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace AscCutlistEditor.Common
{
    public class BoolInverterConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            return value;
        }

        #endregion IValueConverter Members
    }
}