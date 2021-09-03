using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility
{
    /// <summary>
    /// Sends mock data to alphapub for simulating machine information.
    /// </summary>
    internal class MockMachineData
    {
        private readonly List<MockMachineClient> _clients;
        private readonly MachineConnectionsViewModel _machineConnectionsViewModel;

        public MockMachineData(MachineConnectionsViewModel model)
        {
            _clients = new List<MockMachineClient>();
            _machineConnectionsViewModel = model;
        }

        public static async Task PublishMessage(IMqttClient client, string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();
            await client.PublishAsync(message);
        }

        public async void AddMockClient()
        {
            // Start the MQTTClient to listen for new messages.
            if (_machineConnectionsViewModel.Server.IsStarted)
            {
                await StartClient();
            }
        }

        /// <summary>
        /// Disconnect the last added mock client.
        /// </summary>
        public async void RemoveMockClient()
        {
            await StopClient(_clients[^1]);
        }

        private async Task StartClient()
        {
            // Create a new MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    MachineConnectionsViewModel.Ip,
                    MachineConnectionsViewModel.Port)
                .Build();
            var clientId = _clients.Count();
            var client = new MqttFactory().CreateMqttClient();
            var topic = MachineConnectionsViewModel.MainTopic + "/mockdata" + clientId + "/";
            var mockClient = new MockMachineClient(clientId, client, topic);

            // Set the response to send on connection.
            client.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### MOCK " + mockClient.Id + " CONNECTED ###");

                // Test publishing a message.
                await PublishMessage(client, "self/success", "Mock connection successful!");
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
            mockClient.MessageTimer.Stop();
            _clients.Remove(mockClient);
        }
    }
}