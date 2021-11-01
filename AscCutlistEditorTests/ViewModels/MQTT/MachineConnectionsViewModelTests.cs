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
            // Set up sql model.
            Mocks.MockSettings settings = new Mocks.MockSettings();
            settings.InitializeWithDefaults();

            SqlConnectionViewModel sqlModel = new SqlConnectionViewModel(
                settings,
                new Mocks.MockDialog());
            sqlModel.UpdateConnectionString(Strings.ConnectionString);

            // Set up machine connections listener.
            MachineConnectionsViewModel connsModel =
                new MachineConnectionsViewModel(sqlModel)
                {
                    ListenerTopic = "testing/+"
                };
            MachineConnectionsViewModel.SubTopic = "testing";
            MachineConnectionsViewModel.PubTopic = "testing";
            await connsModel.Start(false);

            MachineMessage pubMessage = GetPubMessage();
            MachineMessage subMessage = GetSubMessage();

            // Act
            MachineMessageViewModel.PublishMessage(
                connsModel.Listener,
                "testing/test_topic",
                JsonConvert.SerializeObject(pubMessage));

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

            //Assert.AreEqual(
            //    JsonConvert.SerializeObject(response),
            //    JsonConvert.SerializeObject(subMessage));
        }

        private MachineMessage GetPubMessage()
        {
            return new MachineMessage
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
                            CoilDatReq = "TRUE",
                            CoilStoreReq = "TRUE",
                            CoilUsageDat = "",
                            CoilUsageSend = "",
                            EmergencyStopped = "LINE IS ACTIVE",
                            JobNumber = "JN28174",
                            LineRunning = "ELINE STOPPED",
                            OrderDatReq = "TRUE",
                            OrderNo = "10152",
                            ScanCoilID = "H2215"
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
                    },
                    set3 = new Set3()
                },
                timestamp = DateTime.Now
            };
        }

        private MachineMessage GetSubMessage()
        {
            return new MachineMessage();
        }
    }
}