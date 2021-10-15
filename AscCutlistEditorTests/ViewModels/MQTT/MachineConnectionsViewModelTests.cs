using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.Media.Audio;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditorTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.ViewModels.MQTT
{
    [TestClass]
    public class MachineConnectionsViewModelTests
    {
        [TestMethod]
        public async Task MachineConnectionsViewModelTest()
        {
            // Arrange
            SqlConnectionViewModel sqlModel = new SqlConnectionViewModel(
                new Mocks.MockSettings(),
                new Mocks.MockDialog());
            sqlModel.UpdateConnectionString(Strings.ConnectionString);

            MachineConnectionsViewModel connsModel =
                new MachineConnectionsViewModel(sqlModel)
                {
                    ListenerTopic = "testing/+"
                };
            await connsModel.Start(false);

            // Act
            // Testing adding a tab when a message is received is finicky due
            // to delays, so we'll pretend like we heard one and add it
            // manually instead.
            // Create an STA thread to run UI logic.
            Thread staThread = new Thread(() =>
            {
                connsModel.AddTab("testing/test_topic", "payload!");
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            MachineMessageViewModel messageModel = (MachineMessageViewModel)
                connsModel.MachineConnectionTabs[0].DataContext;

            // Assert
            Assert.AreEqual(messageModel.MachineMessageCollection.Count, 1);
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