using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditor.Views.MQTT;
using AscCutlistEditorTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.ViewModels.MQTT
{
    [TestClass]
    public class SqlConnectionViewModelTests
    {
        [TestMethod]
        public async Task TestGoodConnectionTest()
        {
            // Arrange
            SqlConnectionViewModel model = new SqlConnectionViewModel(
                new Mocks.MockSettings(),
                new Mocks.MockDialog());

            // Act
            bool connResult = await model.TestConnection(
                Strings.ConnectionString,
                false);

            // Assert
            Assert.IsTrue(connResult);
        }

        [TestMethod]
        public async Task TestBadConnectionTest()
        {
            // Arrange
            Mocks.MockSettings settings = new Mocks.MockSettings
            {
                DataSource = "test",
                DatabaseName = "test",
                Username = "test",
                Password = "test"
            };
            SqlConnectionViewModel model = new SqlConnectionViewModel(
                settings,
                new Mocks.MockDialog());

            // Act
            bool connResult = await model.TestConnection(toggleCursor: false);

            // Assert
            Assert.IsFalse(connResult);
        }

        // TODO: Might want to set up a remote test db later to test a working remote connection.

        [TestMethod]
        public async Task TestCreatingMissingFieldConnectionString()
        {
            // Arrange
            Mocks.MockSettings settings = new Mocks.MockSettings
            {
                DataSource = "test",
                DatabaseName = "test",
                Username = "test"
            };
            SqlConnectionViewModel model = new SqlConnectionViewModel(
                settings,
                new Mocks.MockDialog());

            // Act
            bool connResult = await model.TestConnection(toggleCursor: false);

            // Assert
            Assert.IsFalse(connResult);
        }
    }
}