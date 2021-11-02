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
            if (!Debugger.IsAttached)
            {
                // Quit once we have our connection for fast testing.
                int waitTime = 1000;
                const int timeIncrement = 10;

                // Wait while we don't have a machine connection or our
                // connection has no sent messages yet.
                while (connsModel.MachineConnections.FirstOrDefault() == null ||
                       connsModel.MachineConnections.First().MachineConnection.MachineMessagePubCollection.Count == 0)
                {
                    if (waitTime <= 0)
                    {
                        Assert.Fail();  // Kill if no successful response in time.
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
            MachineMessage response = msgModel.MachineConnection.MachineMessagePubCollection.First();

            // Assert
            Assert.AreEqual(connsModel.MachineConnections.Count, 1);

            Assert.AreEqual(
                JsonConvert.SerializeObject(response),
                JsonConvert.SerializeObject(subMessage));
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
                            UserPrime = 3292.49,
                            UserScrap = 16.53,
                            UserUsage = 3309.01
                        },
                        MqttPub = new MqttPub
                        {
                            CoilDatReq = "TRUE",
                            CoilStoreReq = "TRUE",
                            CoilUsageDat = "TEST-ORDERNO1,TEST-COILID,TEST-MAT,TEST-ITEMID,5|TEST-ORDERNO2,TEST-COILID,TEST-MAT,TEST-ITEMID,10",
                            CoilUsageSend = "TRUE",
                            EmergencyStopped = "LINE IS ACTIVE",
                            JobNumber = "JN28174",
                            LineRunning = "LINE STOPPED",
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
            return new MachineMessage
            {
                tags = new Tags
                {
                    set2 = new Set2
                    {
                        MqttSub = new MqttSub
                        {
                            MqttDest = "JN28174",
                            MqttString = null,
                            OrderDatAck = "TRUE",
                            OrderDatRecv = Strings.ExpectedOrderData,
                            BundleDatRecv = Strings.ExpectedBundleData,
                            OrderNo = "10152",
                            CoilDatAck = "TRUE",
                            CoilDatRecv = Strings.ExpectedCoilData,
                            CoilDatValidAck = "FALSE",
                            CoilUsageRecvAck = "TRUE",
                            CoilStoreAck = "TRUE",
                            CoilStoreRecv = Strings.ExpectedCoilStore
                        }
                    }
                }
            };
        }
    }
}