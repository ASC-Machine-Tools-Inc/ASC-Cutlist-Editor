using AscCutlistEditor.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
    }
}