using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// Handles the data for a single machine connection.
    /// </summary>
    internal class MachineDataViewModel : ObservableObject
    {
        private readonly IMqttClient _client;

        // The topic to read from.
        private readonly string _subTopic;

        // The topic to publish responses to.
        private readonly string _pubTopic = "alphasub";

        private ObservableCollection<MachineData> _machineDataCollection =
            new ObservableCollection<MachineData>();

        private MachineData _latestMachineData;
        private LineSeries _uptimeSeries;
        private LineSeries _uptimePercentageSeries;
        private string _connectionStatus;
        private int _uptimeRunningCount;
        private double _uptimeRunningPercentage;

        public PlotModel UptimePlot { get; set; }

        public PlotModel UptimePercentagePlot { get; set; }

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

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                _connectionStatus = value;
                RaisePropertyChangedEvent("ConnectionStatus");
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

        // TODO: assign connmodel
        public MachineDataViewModel(string topic, SqlConnectionViewModel connModel)
        {
            var mqttFactory = new MqttFactory();
            _client = mqttFactory.CreateMqttClient();
            _subTopic = MachineConnectionsViewModel.MainTopic + topic;

            MachineDataCollection = new ObservableCollection<MachineData>();
            LatestMachineData = null;
            ConnectionStatus = "disconnected";

            CreateUptimeModel();
        }

        // TODO: use connmodel to fire off our queries
        public async Task StartClient()
        {
            // Create MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    MachineConnectionsViewModel.Ip,
                    MachineConnectionsViewModel.Port)
                .Build();

            // Response to send on connection.
            _client.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### CONNECTED WITH SERVER ###");

                // Test publishing a message.
                await PublishMessage(
                    _client,
                    "self/success",
                    "Connection successful!");

                // Subscribe to a topic.
                await _client.SubscribeAsync(_subTopic);

                Debug.WriteLine("### SUBSCRIBED TO " + _subTopic + "###");

                ConnectionStatus = "connected";
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
                        // Add the data to our graph.
                        AddMachineData(machineData);

                        // Attempt an acknowledgement response.
                        Task.Run(() => PublishMessage(
                            _client,
                            "self",
                            $"Successful packet for job number: " +
                            $"{machineData.SelectToken("tags.set1.MqttPub.JobNumber")}"));
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

        public static async Task PublishMessage(IMqttClient client, string topic, string payload)
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
                Minimum = 0
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
                Minimum = 0
            });

            _uptimeSeries = currentSeries;
            _uptimePercentageSeries = percentSeries;

            UptimePlot = uptimePlot;
            UptimePercentagePlot = uptimePercentagePlot;
        }

        private void AddMachineData(dynamic data)
        {
            // Run on UI thread.
            // Select the correct dispatcher: if Application.Current is null,
            // we're running unit tests and should use CurrentDispatcher instead.
            Dispatcher dispatcher = Application.Current != null ?
                Application.Current.Dispatcher :
                Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                string lineRunning = data.SelectToken("tags.set1.MqttPub.LineRunning");
                DateTime timeStamp = (DateTime)data.SelectToken("timestamp");

                MachineData machineData = new MachineData
                {
                    ConnectionStatus = (string)data.SelectToken("connected"), // TODO: bool?
                    JobNumber = (string)data.SelectToken("tags.set1.MqttPub.JobNumber"),
                    LineStatus = lineRunning,
                    TimeStamp = timeStamp
                };

                MachineDataCollection.Add(machineData);

                // Calculate percentage for line running bars.
                bool running = false;
                if (lineRunning.Equals("LINE RUNNING"))
                {
                    _uptimeRunningCount++;
                    running = true;
                }
                UptimeRunningPercentage = (double)_uptimeRunningCount / MachineDataCollection.Count * 100;

                _uptimeSeries.Points.Add(
                    new DataPoint(
                        DateTimeAxis.ToDouble(timeStamp),
                        running ? 100 : 0));
                UptimePlot.InvalidatePlot(true); // Refresh plot with new data.

                _uptimePercentageSeries.Points.Add(
            new DataPoint(
                DateTimeAxis.ToDouble(timeStamp),
                UptimeRunningPercentage));
                UptimePercentagePlot.InvalidatePlot(true); // Refresh plot with new data.

                Debug.WriteLine($"Connection Status: {data.SelectToken("connected")}");
                Debug.WriteLine($"Current Job Number: {lineRunning}");
                Debug.WriteLine($"Line Status: {data.SelectToken("tags.set1.MqttPub.LineRunning")}");
                Debug.WriteLine($"Time Received: {timeStamp}");
            });

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