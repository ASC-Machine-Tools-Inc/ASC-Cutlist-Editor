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

namespace AscCutlistEditor.Utility
{
    /// <summary>
    /// Sends mock data to alphapub for simulating machine information.
    /// </summary>
    internal class MockMachineData
    {
        private readonly MqttFactory _mqttFactory;
        private readonly IMqttServer _server;  // Our mqtt broker.
        private readonly List<MockMachineClient> _clients;

        public int Port = 1883;

        public MockMachineData()
        {
            _mqttFactory = new MqttFactory();
            _server = _mqttFactory.CreateMqttServer();
            _clients = new List<MockMachineClient>();
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

        public async void StartServer()
        {
            if (!_server.IsStarted)
            {
                var options = new MqttServerOptionsBuilder()
                    .WithDefaultEndpointPort(Port)
                    .Build();
                await _server.StartAsync(options);
            }
        }

        public async void AddMockClient()
        {
            // Start the MQTTClient to listen for new messages.
            await StartClient();
        }

        /// <summary>
        /// Disconnect the last added mock client.
        /// </summary>
        public async void StopMockMessages()
        {
            await StopClient(_clients[^1]);
        }

        private async Task StartClient()
        {
            // Create MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("192.168.10.43", Port)
                .Build();
            var clientId = _clients.Count();
            var client = _mqttFactory.CreateMqttClient();
            var topic = "alphapub/mockdata" + clientId + "/";
            var mockClient = new MockMachineClient(clientId, client, topic);

            // Response to send on connection.
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