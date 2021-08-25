using AscCutlistEditor.ViewModels.Cutlists;
using AscCutlistEditor.ViewModels.FlatParts;
using AscCutlistEditorTests.ViewModels.Cutlists;
using ExcelDataReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

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
            Thread staThread = new Thread(() =>
            {
                model.CreateRows(cutlists);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            var row1 = model.PartRows[0];
            var row2 = model.PartRows[1];

            // Assert
            // Test count might break on changing part merge cutoff.
            Assert.AreEqual(model.PartRows.Count, 17);

            Assert.AreEqual(row1.Parts.Count, 1);
            Assert.AreEqual(row1.LeftOffset.Left, 0);
            Assert.AreEqual(row2.LeftOffset.Left, model.LeftOffsetPx);
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
            Thread staThread = new Thread(() =>
            {
                model.CreateRows(cutlists1);
                // Load in another cutlist.
                model.CreateRows(cutlists2);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            var row1 = model.PartRows[0];
            var row2 = model.PartRows[1];

            // Assert
            // Test count might break on changing part merge cutoff.
            Assert.AreEqual(model.PartRows.Count, 10);

            Assert.AreEqual(row1.Parts.Count, 1);
            Assert.AreEqual(row1.LeftOffset.Left, 0);
            Assert.AreEqual(row2.LeftOffset.Left, model.LeftOffsetPx);
        }

        [TestMethod]
        public async Task CreateRowsSameCutlistTest()
        {
            // Arrange
            string path1 = "../../../Assets/AndrewCutlist.CSV";
            IExcelDataReader reader1 = CutlistParseViewModelTests.OpenCsv(path1);
            var cutlists1 =
                await CutlistParseViewModel.ParseCutlistCsvAsync(reader1);

            string path2 = "../../../Assets/AndrewCutlist.CSV";
            IExcelDataReader reader2 = CutlistParseViewModelTests.OpenCsv(path2);
            var cutlists2 =
                await CutlistParseViewModel.ParseCutlistCsvAsync(reader2);

            FlatPartRowsViewModel model = new FlatPartRowsViewModel();

            // Act
            // Create an STA thread to run UI logic.
            Thread staThread = new Thread(() =>
            {
                model.CreateRows(cutlists1);
                // Load in the same cutlist.
                model.CreateRows(cutlists2);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            var row1 = model.PartRows[0];
            var row2 = model.PartRows[1];

            // Assert
            // Test count might break on changing part merge cutoff.
            Assert.AreEqual(model.PartRows.Count, 17);

            Assert.AreEqual(row1.Parts.Count, 1);
            Assert.AreEqual(row1.LeftOffset.Left, 0);
            Assert.AreEqual(row2.LeftOffset.Left, model.LeftOffsetPx);
        }

        /* Can't unit test creating the rows async due to its nature - the UI
           updates, and we can't block the STA thread. Manual testing should
           work well enough for this.
        [TestMethod]
        public async Task CreateRowsAsyncTest()
        {
        } */
    }
}