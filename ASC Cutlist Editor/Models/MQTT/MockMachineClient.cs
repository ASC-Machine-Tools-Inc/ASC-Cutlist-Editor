using System;
using System.Windows.Threading;
using AscCutlistEditor.Utility;
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
            MessageTimer.Interval = new TimeSpan(0, 0, 5);
            MessageTimer.Start();
        }

        private async void MockMessageTimerTick(object sender, EventArgs e)
        {
            await MockMachineData.PublishMessage(Client, Topic, "test");
        }
    }
}