using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Threading;
using AscCutlistEditor.Utility;
using AscCutlistEditor.Utility.MQTT;
using AscCutlistEditor.ViewModels.MQTT;
using MQTTnet.Client;

namespace AscCutlistEditor.Models.MQTT
{
    internal class MockMachineClient
    {
        public int Id { get; set; }

        public IMqttClient Client { get; set; }

        public string Topic { get; set; }

        public DispatcherTimer MessageTimer { get; set; }

        public MockMachineClient(int id, IMqttClient client, string topic)
        {
            Id = id;
            Client = client;
            Topic = topic;

            // Message timer is always initialized to send messages every 5 seconds.
            MessageTimer = new DispatcherTimer();
            MessageTimer.Tick += MockMessageTimerTick;
            MessageTimer.Interval = new TimeSpan(0, 0, 1);
            MessageTimer.Start();
        }

        private async void MockMessageTimerTick(object sender, EventArgs e)
        {
            // TODO: update based on line stopped, custom job number, custom connected
            // Pick a random line running status.
            var lineRunningStatuses = new List<string> { "LINE RUNNING", "LINE STOPPED" };
            string lineRunning = lineRunningStatuses[new Random().Next(lineRunningStatuses.Count)];

            string payload =
                "{\"connected\":\"true\"," +
                "\"tags\":" +
                    "{\"set1\":" +
                        "{\"MqttPub\":" +
                            "{\"JobNumber\":\"JN12345\"," +
                            "\"LineRunning\":\"" + lineRunning + "\"}}}," +
                "\"timestamp\":\"" + DateTime.Now + "\"}";
            await MachineDataViewModel.PublishMessage(Client, Topic, payload);
        }
    }
}