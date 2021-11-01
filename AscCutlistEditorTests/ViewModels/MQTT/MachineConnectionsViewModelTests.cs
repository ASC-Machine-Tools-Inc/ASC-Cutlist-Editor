using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT;
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
            SqlConnectionViewModel sqlModel = new SqlConnectionViewModel(
                new Mocks.MockSettings(),
                new Mocks.MockDialog());
            sqlModel.UpdateConnectionString(Strings.ConnectionString);

            MachineConnectionsViewModel connsModel =
                new MachineConnectionsViewModel(sqlModel)
                {
                    ListenerTopic = "testing/+"
                };
            MachineConnectionsViewModel.SubTopic = "testing";
            MachineConnectionsViewModel.PubTopic = "testing";

            await connsModel.Start(false);

            // TODO: write helper method and add fields
            // TODO: test response message?
            MachineMessage message = new MachineMessage
            {
                connected = "true",
                tags = new Tags
                {
                    set1 = new Set1
                    {
                        MachineStatistics = new MachineStatistics
                        {
                            UserPrime = 0,
                            UserScrap = 0,
                            UserUsage = 0
                        },
                        MqttPub = new MqttPub
                        {
                            CoilDatReq = "",
                            CoilStoreReq = "",
                            CoilUsageDat = "",
                            CoilUsageSend = "",
                            EmergencyStopped = "",
                            JobNumber = "test",
                            LineRunning = "FALSE",
                            OrderDatReq = "",
                            OrderNo = "",
                            ScanCoilID = ""
                        },
                        PlantData = new PlantData
                        {
                            COIL = new COIL
                            {
                                COIL_CALCS = new COILCALCS
                                {
                                    ActiveData = new ActiveData()
                                },
                                DESCRIPTION = "",
                                LOG_COMMENT = ""
                            },
                            KPI = new KPI(),
                            WORKORDER = new WORKORDER()
                        }
                    }
                },
                timestamp = DateTime.Now
            };

            // Act
            MachineMessageViewModel.PublishMessage(
                connsModel.Listener,
                "testing/test_topic",
                JsonConvert.SerializeObject(message));

            // Wait for connsModel to detect test_topic & respond accordingly.
            int waitTime = 1000;
            const int timeIncrement = 10;
            while (connsModel.MachineConnections.Count == 0)
            {
                if (waitTime <= 0)
                {
                    Assert.Fail();  // Kill if no successful response in time.
                }

                waitTime -= timeIncrement;
                await Task.Delay(timeIncrement);
            }

            MachineMessageViewModel msgModel = connsModel.MachineConnections.First();
            MachineMessage response = msgModel.MachineConnection.MachineMessageSubCollection.First();

            // Assert
            Assert.AreEqual(connsModel.MachineConnections.Count, 1);

            // Check sent message.
        }
    }
}