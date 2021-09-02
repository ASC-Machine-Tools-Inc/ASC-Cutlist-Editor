using AscCutlistEditor.Utility;
using AscCutlistEditor.ViewModels.Cutlists;
using ExcelDataReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AscCutlistEditorTests.ViewModels.Cutlists
{
    [TestClass]
    public class CutlistParseViewModelTests
    {
        [TestMethod]
        public async Task ParseCutlistCsvAsyncAndrewTest()
        {
            // Arrange
            String path = "../../../Assets/AndrewCutlist.CSV";
            IExcelDataReader reader = OpenCsv(path);

            // Act
            var cutlists =
                await CutlistParseViewModel.ParseCutlistCsvAsync(reader);
            var cutlist = cutlists[0];

            // Assert
            Assert.AreEqual(cutlists.Count, 9);

            Assert.AreEqual(cutlist.ID, 1);
            Assert.AreEqual(cutlist.Length, 222);
            Assert.AreEqual(cutlist.Quantity, 2);
            Assert.AreEqual(cutlist.Made, 2);
            Assert.AreEqual(cutlist.Left, 0);
            Assert.AreEqual(cutlist.Bundle, 1);
            Assert.AreEqual(cutlist.Color, Helpers.LengthToColor(cutlist.Length));
        }

        [TestMethod]
        public async Task ParseCutlistCsvAsyncBryanTest()
        {
            // Arrange
            String path = "../../../Assets/BryanCutlist.CSV";
            IExcelDataReader reader = OpenCsv(path);

            // Act
            var cutlists =
                await CutlistParseViewModel.ParseCutlistCsvAsync(reader);
            var cutlist = cutlists[0];

            // Assert
            Assert.AreEqual(cutlists.Count, 3);

            Assert.AreEqual(cutlist.ID, 1);
            Assert.AreEqual(cutlist.Name, "Ken Pendergrass- l stone");
            Assert.AreEqual(cutlist.Length, 300.0);
            Assert.AreEqual(cutlist.Quantity, 4);
            Assert.AreEqual(cutlist.Made, 0);
            Assert.AreEqual(cutlist.Left, 4);
            Assert.AreEqual(cutlist.Bundle, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public async Task ParseCutlistCsvAsyncBadFileTest()
        {
            // Arrange
            // Not actually a cutlist!
            String path = "../../../Assets/BadCutlist.CSV";
            IExcelDataReader reader = OpenCsv(path);

            // Act
            await CutlistParseViewModel.ParseCutlistCsvAsync(reader);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public async Task ParseCutlistCsvAsyncMalformedFileTest()
        {
            // Arrange
            // Looks like one of Bryan's cutlists, but let's say it got malformed
            // somewhere in the export process.
            String path = "../../../Assets/MalformedBryanCutlist.CSV";
            IExcelDataReader reader = OpenCsv(path);

            // Act
            await CutlistParseViewModel.ParseCutlistCsvAsync(reader);
        }

        public static IExcelDataReader OpenCsv(string path)
        {
            // Needed for .NET core to fix this exception:
            // "No data is available for encoding 1252".
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            FileStream stream = File.Open(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite);
            return ExcelReaderFactory.CreateCsvReader(stream);
        }
    }
}