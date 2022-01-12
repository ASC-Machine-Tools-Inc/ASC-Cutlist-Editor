using AscCutlistEditor.Frameworks;
using ExcelDataReader;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using AscCutlistEditor.Models.Cutlists;
using AscCutlistEditor.Utility.Helpers;

namespace AscCutlistEditor.ViewModels.Cutlists
{
    /// <summary>
    /// Class <c>CutlistParseViewModel</c> handles creating and managing a
    /// collection of cutlists.
    /// </summary>
    internal class CutlistParseViewModel : ObservableObject
    {
        /// <summary>
        ///   Process the given file, checking that it's in a valid cutlist format
        ///   and if so, copy its contents into the list of cutlists.
        ///   <br/>
        ///   See BryanCutlist.csv and AndrewCutlist.csv under Assets for
        ///   cutlist formats.
        /// </summary>
        /// <param name="reader">
        ///    The IExcelDataReader to read data in from after choosing a file.
        /// </param>
        public static async Task<ObservableCollection<Cutlist>> ParseCutlistCsvAsync(IExcelDataReader reader)
        {
            // Reset the cutlists.
            ObservableCollection<Cutlist> cutlists =
                new ObservableCollection<Cutlist>();

            // Advance reader to first row.
            reader.Read();

            // Skip some rows at the start based off the given format.
            string header = reader.GetString(0);
            int rowsToSkip = header switch
            {
                "HEADER 1:" => // Bryan's format.
                    17,
                "CUTLIST" => // Andrew's format.
                    1,
                _ => throw new FileFormatException()
            };

            for (int i = 0; i < rowsToSkip; i++)
            {
                reader.Read();
            }

            while (reader.Read())
            {
                // Asynchronously load in the cutlists from the file.
                Cutlist cutlist = await Task.Run<Cutlist>(() =>
                    ParseCutlistCsvHelper(reader, header));
                if (cutlist != null)
                {
                    cutlists.Add(cutlist);
                }
            }

            return cutlists;
        }

        /// Do the dirty work of parsing the file into neat Cutlists.
        private static Cutlist ParseCutlistCsvHelper(IExcelDataReader reader, string header)
        {
            Cutlist cutlist = null;
            double length;

            switch (header)
            {
                case "HEADER 1:":  // Bryan's format.
                    // Skip empty rows in the file.
                    int qty = int.Parse(reader.GetString(3));
                    if (qty == 0)
                    {
                        break;
                    }

                    length = Math.Round(Helpers.ParseString(reader.GetString(2)), 2);
                    cutlist = new Cutlist
                    {
                        ID = int.Parse(reader.GetString(0)),
                        Name = reader.GetString(1),
                        Length = length,
                        Quantity = qty,
                        Made = int.Parse(reader.GetString(4)),
                        Left = int.Parse(reader.GetString(5)),
                        Bundle = int.Parse(reader.GetString(6)),
                        Color = Helpers.LengthToColor(length)
                    };
                    break;

                case "CUTLIST":  // Andrew's format.
                    length = double.Parse(reader.GetString(5));
                    cutlist = new Cutlist
                    {
                        ID = int.Parse(reader.GetString(0)),
                        Length = length,
                        Quantity = int.Parse(reader.GetString(6)),
                        Made = int.Parse(reader.GetString(7)),
                        Bundle = int.Parse(reader.GetString(8)),
                        Color = Helpers.LengthToColor(length)
                    };

                    // Skip feed info.
                    for (int i = 0; i < 11; i++)
                    {
                        reader.Read();
                    }
                    break;
            }

            return cutlist;
        }
    }
}