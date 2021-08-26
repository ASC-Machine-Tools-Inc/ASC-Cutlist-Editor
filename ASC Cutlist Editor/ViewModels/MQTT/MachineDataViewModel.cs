using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AscCutlistEditor.ViewModels.MQTT
{
    internal class MachineDataViewModel : ObservableObject
    {
        private readonly IMqttServer _server;
        private readonly IMqttClient _client;
        private readonly string _topic;

        private ObservableCollection<MachineData> _machineDataCollection = new ObservableCollection<MachineData>();
        private MachineData _latestMachineData = null;

        public MachineDataViewModel()
        {
            var mqttFactory = new MqttFactory();
            _server = mqttFactory.CreateMqttServer();
            _client = mqttFactory.CreateMqttClient();
            _topic = "alphapub/+/+";

            MachineDataCollection = new ObservableCollection<MachineData>();
            LatestMachineData = null;
        }

        public ObservableCollection<MachineData> MachineDataCollection
        {
            get => _machineDataCollection;
            set
            {
                _machineDataCollection = value;
                RaisePropertyChangedEvent("MachineDataCollection");
            }
        }

        public MachineData LatestMachineData
        {
            get => _latestMachineData;
            set
            {
                _latestMachineData = value;
                RaisePropertyChangedEvent("LatestMachineData");
            }
        }

        public async void Start()
        {
            if (!_server.IsStarted)
            {
                await _server.StartAsync(new MqttServerOptions());

                // Start the MQTTClient to listen for new messages.
                await StartClient();
            }
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

        public async Task StartClient()
        {
            // Create MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("192.168.10.43", 1883)
                //.WithTcpServer("192.168.0.164", 1883)
                .Build();

            // Response to send on connection.
            _client.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### CONNECTED WITH SERVER ###");

                // Test publishing a message.
                await PublishMessage("self/success", "Connection successful!");

                // Subscribe to a topic.
                await _client.SubscribeAsync(_topic);

                Debug.WriteLine("### SUBSCRIBED ###");
            });

            // Response to send on receiving a message.
            _client.UseApplicationMessageReceivedHandler(e =>
            {
                // Acknowledge application message and print some relevant data.
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                Debug.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Debug.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Debug.Write($"+ Payload = {payload}");
                Debug.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Debug.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}\n");

                try
                {
                    // Attempt payload conversion and logging.
                    dynamic machineData = JsonConvert.DeserializeObject(payload);
                    if (machineData != null)
                    {
                        AddMachineData(machineData);

                        Debug.WriteLine($"Connection Status: {machineData.SelectToken("connected")}");
                        Debug.WriteLine($"Current Job Number: {machineData.SelectToken("tags.set1.MqttPub.JobNumber")}");
                        Debug.WriteLine($"Line Status: {machineData.SelectToken("tags.set1.MqttPub.LineRunning")}");
                        Debug.WriteLine($"Time Received: {machineData.SelectToken("timestamp")}");

                        // Attempt an acknowledgement response.
                        Task.Run(() => PublishMessage("self",
                            $"Successful packet for job number: {machineData.SelectToken("tags.set1.MqttPub.JobNumber")}"));
                    }
                }
                catch (JsonReaderException)
                {
                    Debug.WriteLine("Error: invalid JSON value.");
                }
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

        private void AddMachineData(dynamic data)
        {
            MachineData machineData = new MachineData
            {
                ConnectionStatus = (string)data.SelectToken("connected"), // TODO: bool?
                JobNumber = (string)data.SelectToken("tags.set1.MqttPub.JobNumber"),
                LineStatus = (string)data.SelectToken("tags.set1.MqttPub.LineRunning"),
                TimeStamp = (DateTime)data.SelectToken("timestamp")
            };

            MachineDataCollection.Add(machineData);

            // Update the latest if our data is newer.
            /*
            if (LatestMachineData == null ||
                data.TimeStamp > MachineDataCollection.Max(d => d.TimeStamp))
            {
                LatestMachineData = data;
            } */
        }
    }
}