﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AscCutlistEditor.Utility.Helpers
{
    public class BoolRadioConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return Inverse ? !boolValue : boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            if (!boolValue)
            {
                // We only care when the user clicks a radio button to select it.
                return null;
            }

            return !Inverse;
        }
    }

    public class LineRunningColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string lineStatus = value?.ToString();

            return lineStatus switch
            {
                "LINE RUNNING" => Brushes.LightGreen,
                "LINE STOPPED" => Brushes.LightCoral,
                _ => Brushes.Transparent
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}