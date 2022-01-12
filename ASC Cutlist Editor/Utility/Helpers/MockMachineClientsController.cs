using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditor.ViewModels.MQTT.MachineMessage;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace AscCutlistEditor.Utility.Helpers
{
    /// <summary>
    /// Sends mock data to alphapub for simulating machine information.
    /// </summary>
    internal class MockMachineClientsController
    {
        private readonly List<MockMachineClient> _clients;
        private readonly MachineConnectionsViewModel _machineConnectionsViewModel;
        private int _currentId;

        public MockMachineClientsController(MachineConnectionsViewModel model)
        {
            _clients = new List<MockMachineClient>();
            _machineConnectionsViewModel = model;
        }

        public async void AddMockClient()
        {
            // Start the MQTTClient to listen for new messages.
            if (_machineConnectionsViewModel.Server.IsStarted)
            {
                await StartClient();
            }
        }

        private async Task StartClient()
        {
            // Create a new MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    MachineConnectionsViewModel.Ip,
                    MachineConnectionsViewModel.Port)
                .Build();
            var clientId = _currentId++;
            var client = new MqttFactory().CreateMqttClient();
            var topic = MachineConnectionsViewModel.SubTopic +
                        "/mockdata" + clientId + "/";
            var mockClient = new MockMachineClient(clientId, client, topic);

            // Set the response to send on connection.
            client.UseConnectedHandler(e =>
            {
                Debug.WriteLine("### MOCK " + mockClient.Id + " CONNECTED ###");

                // Test publishing a message.
                MachineMessageViewModel.PublishMessage(
                    client,
                    "self/success",
                    "Mock connection successful!");
            });

            try
            {
                await client.ConnectAsync(options);
            }
            catch
            {
                Debug.WriteLine("Connection unsuccessful.");
            }

            _clients.Add(mockClient);
        }

        private async Task StopClient(MockMachineClient mockClient)
        {
            mockClient.Client.UseDisconnectedHandler(e =>
            {
                Debug.WriteLine("### MOCK CLIENT " + mockClient.Id + " DISCONNECTED ###");
            });

            try
            {
                await mockClient.Client.DisconnectAsync();
            }
            catch
            {
                Debug.WriteLine("Disconnection unsuccessful.");
            }

            // Stop timer to prevent memory leak - might be unnecessary?
            mockClient.StopTimers();
            _clients.Remove(mockClient);
        }
    }
}