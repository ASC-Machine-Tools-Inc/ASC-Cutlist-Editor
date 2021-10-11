using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Threading;
using AscCutlistEditor.Utility;
using AscCutlistEditor.Utility.MQTT;
using AscCutlistEditor.ViewModels.MQTT;
using MQTTnet.Client;
using Newtonsoft.Json;

namespace AscCutlistEditor.Models.MQTT
{
    internal class MockMachineClient
    {
        public int Id { get; set; }

        public IMqttClient Client { get; set; }

        public string Topic { get; set; }

        public DispatcherTimer MessageTimer { get; set; }

        private readonly DispatcherTimer _statusTimer;

        private string _lineStatus = "LINE STOPPED";

        public MockMachineClient(int id, IMqttClient client, string topic)
        {
            Id = id;
            Client = client;
            Topic = topic;

            // Initialize timer for randomizing machine status.
            _statusTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 15)
            };
            _statusTimer.Tick += StatusTimerTick;
            _statusTimer.Start();

            // Initialize timer for sending mock messages.
            MessageTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            MessageTimer.Tick += MockMessageTimerTick;
            MessageTimer.Start();
        }

        // End the timers and remove their ticks to prevent memory leaks.
        public void StopTimers()
        {
            _statusTimer.Stop();
            _statusTimer.Tick -= StatusTimerTick;
            MessageTimer.Stop();
            MessageTimer.Tick -= MockMessageTimerTick;
        }

        // Pick a random line running status.
        private void StatusTimerTick(object sender, EventArgs e)
        {
            var lineRunningStatuses = new List<string> { "LINE RUNNING", "LINE STOPPED" };
            _lineStatus = lineRunningStatuses[new Random().Next(lineRunningStatuses.Count)];
        }

        // Publish a fake message for this machine.
        private async void MockMessageTimerTick(object sender, EventArgs e)
        {
            MachineMessage message = new MachineMessage
            {
                connected = "true",
                tags = new Tags
                {
                    set1 = new Set1
                    {
                        MqttPub = new MqttPub
                        {
                            JobNumber = "JN_TEST",
                            LineRunning = _lineStatus
                        }
                    }
                },
                timestamp = DateTime.Now
            };
            await MachineMessageViewModel.PublishMessage(
                Client,
                Topic,
                JsonConvert.SerializeObject(message));
        }
    }
}