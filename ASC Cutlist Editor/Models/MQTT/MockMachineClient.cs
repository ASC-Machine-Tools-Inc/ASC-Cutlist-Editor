using System;
using System.Windows.Threading;
using AscCutlistEditor.Utility;
using AscCutlistEditor.Utility.MQTT;
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
            // Pick a random line running status (weighted towards running).
            string[] lineRunningStatuses = { "LINE RUNNING", "LINE RUNNING", "LINE STOPPED" };
            string lineRunning = lineRunningStatuses[new Random().Next(0, lineRunningStatuses.Length)];

            string payload =
                "{\"connected\":\"true\"," +
                "\"tags\":" +
                    "{\"set1\":" +
                        "{\"MqttPub\":" +
                            "{\"JobNumber\":\"JN12345\"," +
                            "\"LineRunning\":\"" + lineRunning + "\"}}}," +
                "\"timestamp\":\"" + DateTime.Now + "\"}";
            await MockMachineData.PublishMessage(Client, Topic, payload);
        }
    }
}