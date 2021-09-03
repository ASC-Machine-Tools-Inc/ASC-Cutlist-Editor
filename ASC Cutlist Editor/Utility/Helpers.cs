﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;

namespace AscCutlistEditor.Utility
{
    public class Helpers
    {
        /// <summary>
        /// Convert the cutlist length into a corresponding brush color.
        /// </summary>
        /// <param name="length">The length to convert into a color.</param>
        /// <returns>A brush (all brushes from the same length will have the same color).</returns>
        public static Brush LengthToColor(double length)
        {
            PropertyInfo[] properties = typeof(Brushes).GetProperties();
            int randomIndex = Convert.ToInt32(length) % properties.Length;
            Brush brush = (SolidColorBrush)properties[randomIndex]
                .GetValue(null, null);
            return brush;
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class InversePercentageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return 100 - (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion IValueConverter Members
    }
}