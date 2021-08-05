using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AscCutlistEditor.ViewModels.Cutlists;
using AscCutlistEditor.ViewModels.FlatParts;
using AscCutlistEditorTests.ViewModels.Cutlists;
using ExcelDataReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.ViewModels.FlatParts
{
    [TestClass]
    public class FlatPartRowsViewModelTests
    {
        [TestMethod]
        public async Task CreateRowsTest()
        {
            // Arrange
            string path = "../../../Assets/AndrewCutlist.CSV";
            IExcelDataReader reader = CutlistParseViewModelTests.OpenCsv(path);
            var cutlists =
                await CutlistParseViewModel.ParseCutlistCsvAsync(reader);

            FlatPartRowsViewModel model = new FlatPartRowsViewModel();

            // Act
            // Create an STA thread to run UI logic.
            Thread staThread = new Thread(async () =>
            {
                await model.CreateRowsAsync(cutlists);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            var row = model.PartRows[0];

            // Assert
            // Test count might break on changing part merge cutoff.
            Assert.AreEqual(model.PartRows.Count, 17);

            Assert.AreEqual(row.Parts.Count, 1);
        }

        [TestMethod]
        public async Task CreateRowsRefreshTest()
        {
            // Arrange
            string path1 = "../../../Assets/AndrewCutlist.CSV";
            IExcelDataReader reader1 = CutlistParseViewModelTests.OpenCsv(path1);
            var cutlists1 =
                await CutlistParseViewModel.ParseCutlistCsvAsync(reader1);

            string path2 = "../../../Assets/BryanCutlist.CSV";
            IExcelDataReader reader2 = CutlistParseViewModelTests.OpenCsv(path2);
            var cutlists2 =
                await CutlistParseViewModel.ParseCutlistCsvAsync(reader2);

            FlatPartRowsViewModel model = new FlatPartRowsViewModel();

            // Act
            // Create an STA thread to run UI logic.
            Thread staThread = new Thread(async () =>
            {
                await model.CreateRowsAsync(cutlists1);
                // Load in another cutlist.
                await model.CreateRowsAsync(cutlists2);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            var row = model.PartRows[0];

            // Assert
            // Test count might break on changing part merge cutoff.
            Assert.AreEqual(model.PartRows.Count, 10);

            Assert.AreEqual(row.Parts.Count, 1);
        }

        [TestMethod]
        public async Task CreateRowsAsyncTest()
        {
            // Arrange
            string path = "../../../Assets/AndrewCutlist.CSV";
            IExcelDataReader reader = CutlistParseViewModelTests.OpenCsv(path);
            var cutlists =
                await CutlistParseViewModel.ParseCutlistCsvAsync(reader);

            // Set model to not merge cutlists into single parts, and load async.
            FlatPartRowsViewModel model = new FlatPartRowsViewModel();

            // Act
            // Create an STA thread to run UI logic.
            Thread staThread = new Thread(async () =>
            {
                await model.CreateRowsAsync(cutlists);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            var row = model.PartRows[0];

            // Assert
            // Test count might break on changing part merge cutoff.
            Assert.AreEqual(model.PartRows.Count, 17);

            Assert.AreEqual(row.Parts.Count, 1);
        }
    }
}