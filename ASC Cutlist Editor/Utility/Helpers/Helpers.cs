using System;
using System.Reflection;
using System.Windows.Media;

namespace AscCutlistEditor.Utility.Helpers
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

        /// <summary>
        /// Takes in a string and converts it to inches.
        /// </summary>
        /// <param name="length">The inches string to convert.</param>
        /// <returns>The number of inches from the string as a double.</returns>
        public static double ParseString(string length)
        {
            // Try regular parsing first.
            if (double.TryParse(length, out double result))
            {
                return result;
            }

            // Denotes what index to cut off for the suffix.
            int suffixCutoff = Math.Max(0, length.Length - 2);

            string unconverted = length[..suffixCutoff];
            string suffix = length[suffixCutoff..];

            return suffix switch
            {
                "mm" => double.Parse(unconverted) / 25.4,
                _ => throw new FormatException()
            };
        }
    }
}