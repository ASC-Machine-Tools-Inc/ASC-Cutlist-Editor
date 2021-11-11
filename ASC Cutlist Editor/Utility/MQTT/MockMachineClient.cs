using System;
using System.Collections.Generic;
using System.Windows.Threading;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT;
using ModernWpf.Controls;
using MQTTnet.Client;
using Newtonsoft.Json;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class MockMachineClient
    {
        public int Id { get; set; }

        public IMqttClient Client { get; set; }

        public string Topic { get; set; }

        public DispatcherTimer MessageTimer { get; set; }

        private readonly DispatcherTimer _statusTimer;

        private string _lineStatus;
        private double _totalMessages;
        private double _totalRunningMessages;

        public MockMachineClient(int id, IMqttClient client, string topic)
        {
            Id = id;
            Client = client;
            Topic = topic;

            _totalMessages = _totalRunningMessages = 0;

            // Pick initial line status.
            PickRandomLineStatus();

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
        private void PickRandomLineStatus()
        {
            var lineRunningStatuses = new List<string> { "LINE RUNNING", "LINE STOPPED" };
            _lineStatus = lineRunningStatuses[new Random().Next(lineRunningStatuses.Count)];
        }

        private void StatusTimerTick(object sender, EventArgs e)
        {
            PickRandomLineStatus();
        }

        // Publish a fake message for this machine.
        private void MockMessageTimerTick(object sender, EventArgs e)
        {
            _totalMessages++;
            if (_lineStatus == "LINE RUNNING") _totalRunningMessages++;

            double uptimePercentage = _totalRunningMessages / _totalMessages * 100;
            double downtimePercentage = 100 - uptimePercentage;

            MachineMessage message = new MachineMessage
            {
                connected = "true",
                tags = new Tags
                {
                    set1 = new Set1
                    {
                        MqttPub = new MqttPub
                        {
                            JobNumber = $"JN_MOCK_AAAAAAAAAAAAAAAAAAAAAA{Id}",
                            LineRunning = _lineStatus,
                            OrderDatReq = "FALSE"
                        },
                        PlantData = new PlantData
                        {
                            KPI = new KPI
                            {
                                CoilChangePct = 0,
                                BundlePct = 1.2,
                                MaintPct = 14.5,
                                EmergencyPct = 2,
                                IdlePct = 67.38,
                                ShiftChangePct = 4.15,
                                BreakPct = 10.77,
                                UptimePct = uptimePercentage,
                                DowntimePct = downtimePercentage,
                                PrimeFootagePct = 96.82,
                                ScrapFootagePct = 3.16,
                                TotalHours = 824.5
                            }
                        },
                        MachineStatistics = new MachineStatistics
                        {
                            UserPrime = 412.34,
                            UserScrap = 13.45,
                            UserUsage = 425.79
                        }
                    }
                },
                timestamp = DateTime.Now
            };

            MachineMessageViewModel.PublishMessage(
                Client,
                Topic,
                JsonConvert.SerializeObject(message));
        }
    }
}