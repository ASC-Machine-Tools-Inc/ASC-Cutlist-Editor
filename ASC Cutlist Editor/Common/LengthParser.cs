using System;

namespace AscCutlistEditor.Common
{
    internal class LengthParser
    {
        /* Takes in a string and converts it to inches.
         * Should be easily extensible to handle more formats.
         */

        public static double ParseString(string length)
        {
            // Attempt regular parsing first.
            try
            {
                return double.Parse(length);
            }
            catch (FormatException)
            {
                // Denotes what index to cut off for the suffix.
                int suffixCutoff = Math.Max(0, length.Length - 2);

                string unconverted = length[..suffixCutoff];
                string suffix = length[suffixCutoff..];

                return suffix switch
                {
                    "mm" => double.Parse(unconverted) / 25.4
                };
            }
        }
    }
}