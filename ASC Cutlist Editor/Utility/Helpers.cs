using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace AscCutlistEditor.Utility
{
    public class Helpers
    {
        /// <summary>
        /// Convert the cutlist length into a corresponding brush color.
        /// </summary>
        /// <param name="length">The length to convert into a color.</param>
        /// <returns>A brush (all brushes from the same length will have the same color.</returns>
        public static Brush LengthToColor(double length)
        {
            PropertyInfo[] properties = typeof(Brushes).GetProperties();
            int randomIndex = Convert.ToInt32(length) % properties.Length;
            Brush brush = (SolidColorBrush)properties[randomIndex]
                .GetValue(null, null);
            return brush;
        }
    }
}