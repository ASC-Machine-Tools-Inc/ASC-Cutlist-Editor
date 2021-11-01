using System;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.Utility.MQTT;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// Handles the data for a single machine connection.
    /// </summary>
    public class MachineMessageViewModel : ObservableObject
    {
        /// <summary>
        /// Model for storing the mqtt connection data.
        /// </summary>
        public MachineConnection MachineConnection { get; set; }

        /// <summary>
        /// Last machine message the client listener received.
        /// </summary>
        public MachineMessage LatestMachineMessage
        {
            get => _latestMachineMessage;
            set
            {
                _latestMachineMessage = value;
                RaisePropertyChangedEvent("LatestMachineMessage");
            }
        }

        public PlotModel UptimePlot { get; set; }

        public PlotModel DowntimeStatsPlot { get; set; }

        private MachineMessage _latestMachineMessage;

        private LineSeries _uptimeSeries;
        private BarSeries _downtimeStatsSeries;

        private SqlConnectionViewModel _sqlConn;

        public MachineMessageViewModel(
            string topic,
            SqlConnectionViewModel connModel,
            MachineMessage message = null)
        {
            var mqttFactory = new MqttFactory();
            IMqttClient client = mqttFactory.CreateMqttClient();
            string subTopic = MachineConnectionsViewModel.SubTopic + topic;
            string pubTopic = MachineConnectionsViewModel.PubTopic + topic;

            // Strip slashes to just display the subscribed topic.
            string displayTopic = topic.Replace("/", "");

            MachineConnection = new MachineConnection(
                client,
                subTopic,
                pubTopic,
                displayTopic,
                connModel
            );

            _sqlConn = connModel;

            CreateUptimeModel();  // Create the plots for the UI.
            StartClient();  // Listen for continued messages.
            ProcessResponseMessage(message);  // Process first message (if exists).
        }

        /// <summary>
        /// Fire & forget for initializing client.
        /// </summary>
        public async void StartClient()
        {
            await StartClientAsync();
        }

        private async Task StartClientAsync()
        {
            // Create MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    MachineConnectionsViewModel.Ip,
                    MachineConnectionsViewModel.Port)
                .Build();

            // Response to send on connection.
            MachineConnection.Client.UseConnectedHandler(async e =>
            {
                PublishMessage(
                    MachineConnection.Client,
                    "self/success",
                    "Connection successful!");

                await MachineConnection.Client.SubscribeAsync(MachineConnection.SubTopic);
            });

            // Response to send on receiving a message.
            MachineConnection.Client.UseApplicationMessageReceivedHandler(e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                try
                {
                    // Attempt payload conversion and logging.
                    MachineMessage machineMessage =
                        JsonConvert.DeserializeObject<MachineMessage>(payload);

                    // Bryan's code for handling messages and their flags.
                    ProcessResponseMessage(machineMessage);
                }
                catch (JsonReaderException)
                {
                    Debug.WriteLine("Error: invalid JSON value.");
                }
            });

            try
            {
                await MachineConnection.Client.ConnectAsync(options);
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
            // Create the plot models.
            var uptimePlot = new PlotModel
            {
                Title = "Status Over Time",
            };
            var downtimeStatsPlot = new PlotModel
            {
                Title = "Downtime Statistics"
            };

            // Create the line series.
            var currentSeries = new LineSeries
            {
                Title = "Current Status",
                MarkerType = MarkerType.Square
            };
            var downtimeStatsSeries = new BarSeries
            {
                ItemsSource = new List<BarItem>(7),
                LabelFormatString = "{0:.00}%"
            };

            // Add the series to the plot models.
            uptimePlot.Series.Add(currentSeries);
            uptimePlot.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "h:mm:ss",
            });
            // Y axis with labels for current line status.
            uptimePlot.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                // A little buffer room is added to make sure "Running" is
                // shown on the y-axis for the maximum.
                Maximum = 1.05,
                Minimum = 0,
                IsTickCentered = true,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                Labels = { "Stopped", "Running" }
            });

            downtimeStatsPlot.Series.Add(downtimeStatsSeries);
            downtimeStatsPlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Maximum = 105,
                Minimum = 0,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });
            downtimeStatsPlot.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                IsTickCentered = true,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                ItemsSource = new[]
                {
                    "Material Change",
                    "Bundle Unloading",
                    "Maintenance",
                    "Emergency",
                    "Idle",
                    "Shift Change",
                    "Break"
                }
            });

            _uptimeSeries = currentSeries;
            _downtimeStatsSeries = downtimeStatsSeries;

            UptimePlot = uptimePlot;
            DowntimeStatsPlot = downtimeStatsPlot;
        }

        /// <summary>
        /// Handle returning the response message and performing any tasks
        /// based off the flags set in the machine's message.
        /// </summary>
        private async void ProcessResponseMessage(MachineMessage message)
        {
            if (message == null)
            {
                return;
            }

            // To avoid confusion - the pub here refers to the machine's
            // published message, and sub refers to the response the machine
            // expects. The client publishes the return message to PubTopic.
            MqttPub pub = message.tags.set1.MqttPub;
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

            // Check that a job number was included in the message.
            if (string.IsNullOrEmpty(pub.JobNumber))
            {
                return;
            }

            // Update the UI.
            UpdateMachineTab(message);

            MessageFlagHandlers handlers = new MessageFlagHandlers(_sqlConn);

            try
            {
                // Handle the order data requested flag (getting the orders and bundles).
                await handlers.OrderDatReqFlagHandler(message, returnMessage);

                // Handle the coil data requested flag (running a specific coil and order).
                await handlers.CoilDatReqFlagHandler(message, returnMessage);

                // Handle the coil list requested flag (all non-depleted coils).
                await handlers.CoilStoreReqFlagHandler(message, returnMessage);

                // Handle the coil usage sending requested flag (write to database).
                int rowsAdded = await handlers.CoilUsageSendFlagHandler(
                    message,
                    returnMessage);
            }
            catch (Exception ex)
            {
                // Query error. Close the connection.
                // TODO: set status to sql failed
                Debug.WriteLine("Query error." + ex);
            }

            // Finally, write the response message back out for the HMI.
            PublishMessage(
                MachineConnection.Client,
                MachineConnection.PubTopic,
                JsonConvert.SerializeObject(returnMessage));
            MachineConnection.MachineMessagePubCollection.Add(returnMessage);
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
                MachineConnection.MachineMessageSubCollection.Add(message);
                LatestMachineMessage = message;

                MqttPub pub = message.tags.set1.MqttPub;
                KPI kpi = message.tags.set1.PlantData.KPI;

                // Add new data point for line status.
                _uptimeSeries.Points.Add(
                    new DataPoint(
                        DateTimeAxis.ToDouble(message.timestamp),
                        pub.LineRunning.Equals("LINE RUNNING") ? 1 : 0));
                UptimePlot.InvalidatePlot(true);

                // Recalculate percentage for downtime bars.
                _downtimeStatsSeries.ItemsSource = new List<BarItem>
                {
                    new BarItem { Value = kpi.CoilChangePct },
                    new BarItem { Value = kpi.BundlePct },
                    new BarItem { Value = kpi.MaintPct },
                    new BarItem { Value = kpi.EmergencyPct },
                    new BarItem { Value = kpi.IdlePct },
                    new BarItem { Value = kpi.ShiftChangePct },
                    new BarItem { Value = kpi.BreakPct }
                };
                DowntimeStatsPlot.InvalidatePlot(true);
            });
        }
    }
}