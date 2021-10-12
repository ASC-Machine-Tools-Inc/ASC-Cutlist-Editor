﻿using AscCutlistEditor.Frameworks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.Utility.MQTT;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// Handles the data for a single machine connection.
    /// </summary>
    internal class MachineMessageViewModel : ObservableObject
    {
        // Model for storing the mqtt connection data.
        private MachineConnection _machineConnection;

        private LineSeries _uptimeSeries;
        private LineSeries _uptimePercentageSeries;
        private int _uptimeRunningCount;
        private double _uptimeRunningPercentage;

        public PlotModel UptimePlot { get; set; }

        public PlotModel UptimePercentagePlot { get; set; }

        public ObservableCollection<MachineMessage> MachineMessageCollection
        {
            get => _machineConnection.MachineMessageCollection;
            set
            {
                _machineConnection.MachineMessageCollection = value;
                RaisePropertyChangedEvent("MachineMessageCollection");
            }
        }

        public double UptimeRunningPercentage
        {
            get => _uptimeRunningPercentage;
            set
            {
                _uptimeRunningPercentage = value;
                RaisePropertyChangedEvent("UptimeRunningPercentage");
            }
        }

        public MachineMessageViewModel(string topic, SqlConnectionViewModel connModel)
        {
            var mqttFactory = new MqttFactory();
            IMqttClient client = mqttFactory.CreateMqttClient();
            string subTopic = MachineConnectionsViewModel.MainTopic + topic;
            string pubTopic = "alphasub";

            _machineConnection = new MachineConnection(
                client,
                subTopic,
                pubTopic,
                connModel
            );

            CreateUptimeModel();
        }

        public async Task StartClient()
        {
            // Create MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    MachineConnectionsViewModel.Ip,
                    MachineConnectionsViewModel.Port)
                .Build();

            // Response to send on connection.
            _machineConnection.Client.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### SUBSCRIBED TO " + _machineConnection.SubTopic + " ###");

                PublishMessage(
                    _machineConnection.Client,
                    "self/success",
                    "Connection successful!");

                await _machineConnection.Client.SubscribeAsync(_machineConnection.SubTopic);
            });

            // Response to send on receiving a message.
            _machineConnection.Client.UseApplicationMessageReceivedHandler(e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                try
                {
                    // Attempt payload conversion and logging.
                    MachineMessage machineMessage = JsonConvert.DeserializeObject<MachineMessage>(payload);
                    if (machineMessage == null)
                    {
                        return;
                    }

                    // Bryan's code for handling messages TODO rename.
                    BryanCode(machineMessage);
                }
                catch (JsonReaderException)
                {
                    Debug.WriteLine("Error: invalid JSON value.");
                }
            });

            try
            {
                await _machineConnection.Client.ConnectAsync(options);
            }
            catch
            {
                Debug.WriteLine("Connection unsuccessful.");
            }
        }

        /// <summary>
        /// Publish an MQTT message.
        /// </summary>
        /// <param name="client">The IMqttClient to publish the message from.</param>
        /// <param name="topic">The topic to publish the message under.</param>
        /// <param name="payload">The contents of the message to publish.</param>
        public static async void PublishMessage(IMqttClient client, string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();
            await client.PublishAsync(message);
        }

        /// <summary>
        /// Create the OxyPlot graphs for our view.
        /// </summary>
        private void CreateUptimeModel()
        {
            var uptimePlot = new PlotModel { Title = "Uptime", Subtitle = "Current" };
            var uptimePercentagePlot = new PlotModel { Title = "Uptime", Subtitle = "Percentage" };

            // Create line series (markers are hidden by default).
            var currentSeries = new LineSeries { Title = "Current Status", MarkerType = MarkerType.Square };
            var percentSeries = new LineSeries { Title = "Uptime Percentage", MarkerType = MarkerType.Square };

            // Add the series to the plot models.
            uptimePlot.Series.Add(currentSeries);
            uptimePlot.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "h:mm:ss",
            });
            uptimePlot.Axes.Add(new LinearAxis
            {
                Maximum = 105,
                Minimum = 0,
                AbsoluteMaximum = 105,
                AbsoluteMinimum = 0
            });

            uptimePercentagePlot.Series.Add(percentSeries);
            uptimePercentagePlot.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "h:mm:ss",
            });
            uptimePercentagePlot.Axes.Add(new LinearAxis
            {
                Maximum = 105,
                Minimum = 0,
                AbsoluteMaximum = 105,
                AbsoluteMinimum = 0
            });

            _uptimeSeries = currentSeries;
            _uptimePercentageSeries = percentSeries;

            UptimePlot = uptimePlot;
            UptimePercentagePlot = uptimePercentagePlot;
        }

        /// <summary>
        /// Putting all the code for the message handler here temporarily.
        /// </summary>
        //TODO: change to task?
        private async void BryanCode(MachineMessage message)
        {
            MachineMessage returnMessage = new MachineMessage
            {
                tags = new Tags
                {
                    set2 = new Set2
                    {
                        MqttSub = new MqttSub()
                    }
                }
            };

            MqttPub pub = message.tags.set1.MqttPub;
            MqttSub sub = returnMessage.tags.set2.MqttSub;

            // Check that a job number was included in the message.
            if (string.IsNullOrEmpty(pub.JobNumber))
            {
                return;
            }

            // Update the UI.
            UpdateMachineTab(message);

            // Handle the order data requested flag.
            await MessageFlagHandlers.OrderDatReqFlagHandler(message, returnMessage);

            // Handle the coil data requested flag.
            await MessageFlagHandlers.CoilDatReqFlagHandler(message, returnMessage);

            // 7. Check if coil store req true and coil usage dat not empty

            // 8. if coil usage send true and coil usage dat

            // 9. write data back to hmi

            // Notes: don't check for null, either empty string or not
        }

        /// <summary>
        /// Update the UI for the corresponding machine from its message data.
        /// </summary>
        internal void UpdateMachineTab(MachineMessage message)
        {
            // Run on UI thread.
            // Select the correct dispatcher: if Application.Current is null,
            // we're running unit tests and should use CurrentDispatcher instead.
            Dispatcher dispatcher = Application.Current != null
                ? Application.Current.Dispatcher
                : Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                MachineMessageCollection.Add(message);

                MqttPub mqttPub = message.tags.set1.MqttPub;

                // Calculate percentage for line running bars.
                bool running = false;
                if (mqttPub.LineRunning.Equals("LINE RUNNING"))
                {
                    _uptimeRunningCount++;
                    running = true;
                }

                UptimeRunningPercentage = (double)_uptimeRunningCount / MachineMessageCollection.Count * 100;

                _uptimeSeries.Points.Add(
                    new DataPoint(
                        DateTimeAxis.ToDouble(message.timestamp),
                        running ? 100 : 0));
                UptimePlot.InvalidatePlot(true); // Refresh plot with new message.

                _uptimePercentageSeries.Points.Add(
                    new DataPoint(
                        DateTimeAxis.ToDouble(message.timestamp),
                        UptimeRunningPercentage));
                UptimePercentagePlot.InvalidatePlot(true); // Refresh plot with new message.
            });
        }
    }
}