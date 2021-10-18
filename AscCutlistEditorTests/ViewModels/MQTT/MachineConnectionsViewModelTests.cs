using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.ViewModels.MQTT
{
    [TestClass]
    public class MachineConnectionsViewModelTests
    {
        /* TODO: update MachineConnections to be more MVVM-friendly
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

            MachineMessage message = new MachineMessage
            {
                connected = "true",
                tags = new Tags
                {
                    set1 = new Set1
                    {
                        MqttPub = new MqttPub()
                    }
                },
                timestamp = DateTime.Now
            };

            // Act
            // Testing adding a tab when a message is received is finicky due
            // to delays, so we'll pretend like we heard one and add it
            // manually instead.
            // Create an STA thread to run UI logic.
            Thread staThread = new Thread(async () =>
            {
                await connsModel.AddTab(
                    "testing/test_topic",
                    JsonConvert.SerializeObject(message));
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
        } */
    }
}