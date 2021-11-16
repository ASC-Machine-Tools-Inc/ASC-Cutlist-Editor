using System;
using System.Windows.Threading;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT.MachineMessage;
using MQTTnet.Client;
using Newtonsoft.Json;

namespace AscCutlistEditor.Utility.Helpers
{
    internal class MockMachineClient
    {
        public int Id { get; set; }

        public IMqttClient Client { get; set; }

        public string Topic { get; set; }

        public DispatcherTimer MessageTimer { get; set; }

        // After this many seconds, the machine's status is randomly picked
        // (according to the running probability).
        private const int TimePeriod = 15;

        // Probability for good material (not scrap).
        private const double GoodProbability = 0.98;

        private readonly Random _random;

        private readonly DispatcherTimer _statusTimer;

        // Status for if the line is running or stopped.
        private string _lineStatus;

        // Number of sent machine messages.
        private double _totalMessages;

        // Number of sent machine messages while running.
        private double _totalRunningMessages;

        // Value from 0.0 - 1.0 representing probability machine will be running
        // for each time period.
        private readonly double _runningProbability;

        // The total amount of material ran through the machine. If the machine
        // is running, it will run a random amount between 0.0 and 1.0.
        private double _totalMaterial;

        // The total amount of material used for parts (leftover is scrap).
        private double _totalGood;

        // Time the machine was started. Used to calculate uptime hours.
        private readonly DateTime _startTime;

        public MockMachineClient(int id, IMqttClient client, string topic)
        {
            Id = id;
            Client = client;
            Topic = topic;

            _random = new Random();
            _totalMessages = _totalRunningMessages = 0;
            _startTime = DateTime.Now;
            _runningProbability = _random.NextDouble();

            // Pick initial line status.
            PickRandomLineStatus();

            // Initialize timer for randomizing machine status.
            _statusTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, TimePeriod)
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
            double chance = _random.NextDouble();
            _lineStatus = chance < _runningProbability ? "LINE RUNNING" : "LINE STOPPED";
        }

        private void StatusTimerTick(object sender, EventArgs e)
        {
            PickRandomLineStatus();
        }

        // Publish a fake message for this machine.
        private void MockMessageTimerTick(object sender, EventArgs e)
        {
            _totalMessages++;

            // Update fields if machine is running.
            if (_lineStatus == "LINE RUNNING")
            {
                _totalRunningMessages++;

                double material = _random.NextDouble();
                _totalMaterial += material;

                if (_random.NextDouble() < GoodProbability)
                {
                    _totalGood += material;
                }
            }

            double uptime = (DateTime.Now - _startTime).TotalHours;

            double uptimePercentage = _totalRunningMessages / _totalMessages;

            double uptimeHours = uptime * uptimePercentage;
            double downtimeHours = uptime - uptimeHours;

            // Adjust for display.
            uptimePercentage *= 100;
            double downtimePercentage = 100 - uptimePercentage;

            double primeFootagePercentage = _totalMaterial == 0 ? 0 : _totalGood / _totalMaterial * 100;
            double scrapFootagePercentage = 100 - primeFootagePercentage;

            MachineMessage message = new MachineMessage
            {
                connected = "true",
                tags = new Tags
                {
                    set1 = new Set1
                    {
                        MqttPub = new MqttPub
                        {
                            JobNumber = $"JN_MOCK_{Id}",
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
                                UptimeHrs = uptimeHours,
                                DowntimePct = downtimePercentage,
                                DowntimeHrs = downtimeHours,
                                PrimeFootagePct = primeFootagePercentage,
                                ScrapFootagePct = scrapFootagePercentage,
                                TotalHours = uptime
                            }
                        },
                        MachineStatistics = new MachineStatistics
                        {
                            UserPrime = _totalGood,
                            UserScrap = _totalMaterial - _totalGood,
                            UserUsage = _totalMaterial
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