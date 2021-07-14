using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation.Metadata;

namespace ASC_Cutlist_Editor.Common
{
    internal class LengthParser
    {
        /* Takes in a string and converts it to inches.
         * Should be easily extensible to handle more formats.
         */

        public static double ParseString(string length)
        {
            // Denotes what index to cut off for the suffix.
            int suffixCutoff = Math.Max(0, length.Length - 2);

            string unconverted = length[..suffixCutoff];
            string suffix = length[suffixCutoff..];

            switch (suffix)
            {
                case "mm":
                    return double.Parse(unconverted) / 25.4;

                default:
                    return double.Parse(length);
            }
        }
    }
}