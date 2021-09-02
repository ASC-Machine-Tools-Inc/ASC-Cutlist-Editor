using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using AscCutlistEditor.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;

namespace AscCutlistEditor.Utility
{
    /// <summary>
    /// Sends mock data to alphapub for simulating machine information.
    /// </summary>
    internal class MockMachineData
    {
        private DispatcherTimer _mockMessageTimer;
        private readonly IMqttServer _server;
        private readonly IMqttClient _client;
        private readonly string _topic;

        public int Port = 33333;

        public MockMachineData()
        {
            var mqttFactory = new MqttFactory();
            _server = mqttFactory.CreateMqttServer();
            _client = mqttFactory.CreateMqttClient();
            _topic = "alphapub/mockdata" + Port + "/";
        }

        public async Task PublishMessage(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();
            await _client.PublishAsync(message);
        }

        public async void Start()
        {
            if (!_server.IsStarted)
            {
                var options = new MqttServerOptionsBuilder()
                    .WithDefaultEndpointPort(Port)
                    .Build();
                await _server.StartAsync(options);

                // Start the MQTTClient to listen for new messages.
                await StartClient();
            }
        }

        public async Task StartClient()
        {
            // Create MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("192.168.10.43", Port)
                .Build();
            Port++;

            // Response to send on connection.
            _client.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### MOCK CONNECTED WITH SERVER ###");

                // Test publishing a message.
                await PublishMessage("self/success", "Mock connection successful!");
            });

            try
            {
                await _client.ConnectAsync(options);
            }
            catch
            {
                Debug.WriteLine("Connection unsuccessful.");
            }
        }

        public async Task StopClient()
        {
            _client.UseDisconnectedHandler(e =>
            {
                Debug.WriteLine("### MOCK DISCONNECTED ###");
            });

            try
            {
                await _client.DisconnectAsync();
            }
            catch
            {
                Debug.WriteLine("Disconnection unsuccessful.");
            }
        }

        public void StartMockMessages()
        {
            Start();

            _mockMessageTimer = new DispatcherTimer();
            _mockMessageTimer.Tick += MockMessageTimerTick;
            _mockMessageTimer.Interval = new TimeSpan(0, 0, 5);
            _mockMessageTimer.Start();
        }

        public async void StopMockMessages()
        {
            await StopClient();

            _mockMessageTimer.Stop();
        }

        private async void MockMessageTimerTick(object sender, EventArgs e)
        {
            await PublishMessage(_topic, "test");
        }
    }
}