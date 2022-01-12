using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditor.ViewModels.MQTT.MachineMessage;
using AscCutlistEditorTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace AscCutlistEditorTests.ViewModels.MQTT
{
    [TestClass]
    public class MachineConnectionsViewModelTests
    {
        [TestMethod]
        public async Task MachineConnectionsViewModelTest()
        {
            // Arrange
            // Set up machine connections listener.
            SqlConnectionViewModel sqlModel = SetupSqlModel();

            MachineConnectionsViewModel connsModel =
                new MachineConnectionsViewModel(sqlModel)
                {
                    ListenerTopic = "testing/+"
                };
            MachineConnectionsViewModel.SubTopic = "testing";
            MachineConnectionsViewModel.PubTopic = "testing";
            await connsModel.Start(false);

            // Loop through test messages.
            for (int i = 0; i < TestMessagePairs.PubMessages.Count; i++)
            {
                MachineMessage pubMessage = TestMessagePairs.PubMessages[i];
                MachineMessage subMessage = TestMessagePairs.SubMessages[i];

                // Act
                MachineMessageViewModel.PublishMessage(
                    connsModel.Listener,
                    "testing/test_topic",
                    JsonConvert.SerializeObject(pubMessage));

                // Wait for connsModel to detect test_topic & respond accordingly.
                if (!Debugger.IsAttached)
                {
                    // Quit once we have our connection for fast testing.
                    int waitTime = 5000;
                    const int timeIncrement = 10;

                    // Wait while we don't have a machine connection or our
                    // response message hasn't arrived.
                    while (connsModel.MachineConnections.FirstOrDefault() == null ||
                           connsModel.MachineConnections.First().MachineConnection
                               .MachineMessagePubCollection.Count != i + 1)
                    {
                        if (waitTime <= 0)
                        {
                            // Kill if no successful response in time.
                            Assert.Fail("Current message count: " +
                                        $"{connsModel.MachineConnections.First().MachineConnection.MachineMessagePubCollection.Count}");
                        }

                        waitTime -= timeIncrement;
                        await Task.Delay(timeIncrement);
                    }
                }
                else
                {
                    // Default delay while debugging to ensure connection.
                    await Task.Delay(500);
                }

                MachineMessageViewModel msgModel = connsModel.MachineConnections.First();
                MachineMessage response = msgModel.MachineConnection.MachineMessagePubCollection.Last();

                // Assert we have a connection and receive the right response.
                Assert.AreEqual(1, connsModel.MachineConnections.Count);

                Assert.AreEqual(
                    JsonConvert.SerializeObject(subMessage),
                    JsonConvert.SerializeObject(response));
            }
        }

        [TestMethod]
        public void MachineConnectionsRefreshTest()
        {
            // Arrange
            var sqlModel = SetupSqlModel();
            var connsModel = new MachineConnectionsViewModel(sqlModel);
            var connsModelEmpty = new MachineConnectionsViewModel(sqlModel);

            // Act
            connsModel.MachineConnections.Add(
                new MachineMessageViewModel("test", sqlModel));
            connsModel.Refresh();

            // Assert
            Assert.AreEqual(
                connsModel.MachineConnections.Count,
                connsModelEmpty.MachineConnections.Count);
        }

        private SqlConnectionViewModel SetupSqlModel()
        {
            // Set up sql model.
            Mocks.MockSettings settings = new Mocks.MockSettings();
            settings.InitializeWithDefaults();

            SqlConnectionViewModel sqlModel = new SqlConnectionViewModel(
                settings,
                new Mocks.MockDialog());
            sqlModel.UpdateConnectionString(Strings.ConnectionString);

            return sqlModel;
        }
    }
}