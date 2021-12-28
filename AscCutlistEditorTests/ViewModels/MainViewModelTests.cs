using AscCutlistEditor.Models;
using AscCutlistEditor.ViewModels;
using AscCutlistEditor.ViewModels.Cutlists;
using AscCutlistEditor.ViewModels.Parts;
using AscCutlistEditor.Views.Bundles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AscCutlistEditor.Models.Cutlists;
using AscCutlistEditor.Models.Parts;

namespace AscCutlistEditorTests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private readonly MainViewModel _model = new MainViewModel();

        [TestMethod]
        public void MainViewModelVisibilityTest()
        {
            // Arrange
            List<bool> visibilityList = new List<bool> { false, false, false };
            ObservableCollection<bool> visibility =
                new ObservableCollection<bool>(visibilityList);

            // Act
            // Test UI toggling
            _model.ToggleCutlistCommand.Execute(null);
            _model.ToggleFlatViewCommand.Execute(null);
            _model.Toggle3DCommand.Execute(null);

            // Assert
            Assert.IsTrue(_model.UiVisibility.SequenceEqual(visibility));
        }

        [TestMethod]
        public void MainViewModelClearCutlistTest()
        {
            // Arrange
            // Simulating opening a cutlist.
            CutlistImportViewModel cutModel = _model.CutlistViewModel;
            cutModel.Filename = "Imaginary.CSV";
            cutModel.ImportVisibility = false;
            cutModel.CloseButtonVisibility = true;
            cutModel.Cutlists.Add(new Cutlist());

            // Simulating drawing a part row.
            PartCollectionViewModel rowModel = _model.PartCollectionViewModel;
            rowModel.FlatPartButtonRowVisibility = true;
            rowModel.PartRows.Add(new PartRow());

            // Simulating drawing a bundle.
            rowModel.Bundles.Add(new SingleBundleControl());

            // Act
            _model.ClearCutlistCommand.Execute(null);

            // Assert
            Assert.AreEqual(cutModel.Filename, null);
            Assert.AreEqual(cutModel.ImportVisibility, true);
            Assert.AreEqual(cutModel.CloseButtonVisibility, false);
            Assert.AreEqual(cutModel.Cutlists.Count, 0);

            Assert.AreEqual(rowModel.FlatPartButtonRowVisibility, false);
            Assert.AreEqual(rowModel.PartRows.Count, 0);

            Assert.AreEqual(rowModel.Bundles.Count, 0);
        }
    }
}