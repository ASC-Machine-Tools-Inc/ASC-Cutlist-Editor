using System;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditorTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.ViewModels.MQTT
{
    [TestClass]
    public class MachineConnectionsViewModelTests
    {
        [TestMethod]
        public void MachineConnectionsViewModelTest()
        {
            // Arrange
            SqlConnectionViewModel sqlModel = new SqlConnectionViewModel(
                new Mocks.MockSettings(),
                new Mocks.MockDialog());
            sqlModel.CreateConnectionString(Strings.ConnectionString);

            MachineConnectionsViewModel connsModel =
                new MachineConnectionsViewModel(sqlModel);

            // Act

            // Assert
        }

        [TestMethod]
        public void StartTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void AddTabTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void RefreshTest()
        {
            throw new NotImplementedException();
        }
    }
}