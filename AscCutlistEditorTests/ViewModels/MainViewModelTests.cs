using AscCutlistEditor.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AscCutlistEditor.Models;
using AscCutlistEditor.ViewModels.Cutlists;
using AscCutlistEditor.ViewModels.FlatParts;

namespace AscCutlistEditorTests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        [TestMethod]
        public void MainViewModelVisibilityTest()
        {
            // Arrange
            MainViewModel model = new MainViewModel();
            List<bool> visibilityList = new List<bool> { false, false, false };
            ObservableCollection<bool> visibility =
                new ObservableCollection<bool>(visibilityList);

            // Act
            // Test UI toggling
            model.ToggleCutlistCommand.Execute(null);
            model.ToggleFlatViewCommand.Execute(null);
            model.Toggle3DCommand.Execute(null);

            // Assert
            Assert.IsTrue(model.UiVisibility.SequenceEqual(visibility));
        }

        [TestMethod]
        public void MainViewModelClearCutlistTest()
        {
            // Arrange
            MainViewModel model = new MainViewModel();

            // Simulating opening a cutlist.
            CutlistImportViewModel cutModel = model.CutlistViewModel;
            cutModel.Filename = "Imaginary.CSV";
            cutModel.ImportVisibility = false;
            cutModel.CloseButtonVisibility = true;
            cutModel.Cutlists.Add(new Cutlist());

            // Simulating drawing a part row.
            FlatPartRowsViewModel rowModel = model.FlatPartRowsViewModel;
            rowModel.FlatPartButtonRowVisibility = true;
            rowModel.PartRows.Add(new PartRow());

            // Act
            model.ClearCutlistCommand.Execute(null);

            // Assert
            Assert.AreEqual(cutModel.Filename, null);
            Assert.AreEqual(cutModel.ImportVisibility, true);
            Assert.AreEqual(cutModel.CloseButtonVisibility, false);
            Assert.AreEqual(cutModel.Cutlists.Count, 0);

            Assert.AreEqual(rowModel.FlatPartButtonRowVisibility, false);
            Assert.AreEqual(rowModel.PartRows.Count, 0);
        }
    }
}