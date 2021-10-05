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
using System.Diagnostics;
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
    ///
    internal class MachineDataViewModel : ObservableObject
    {
        private readonly IMqttClient _client;
        private readonly string _topic;

        private ObservableCollection<MachineData> _machineDataCollection = new ObservableCollection<MachineData>();
        private MachineData _latestMachineData;
        private readonly LineSeries _uptimeSeries;
        private string _connectionStatus;
        private int _uptimeRunningCount;
        private double _uptimeRunningPercentage;

        public PlotModel PlotModel { get; set; }

        public MachineDataViewModel(string topic)
        {
            var mqttFactory = new MqttFactory();
            _client = mqttFactory.CreateMqttClient();
            _topic = MachineConnectionsViewModel.MainTopic + topic;

            MachineDataCollection = new ObservableCollection<MachineData>();
            LatestMachineData = null;
            ConnectionStatus = "disconnected";

            // Test plot model.
            // Create the plot model.
            var tmp = new PlotModel { Title = "Uptime", Subtitle = "TESTING" };

            // Create two line series (markers are hidden by default).
            var series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Square };

            DateTimeAxis xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "h:mm:ss",
            };
            LinearAxis yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Maximum = 105,
                Minimum = 0
            }; // TODO: fix axis, maybe not linear???

            var series2 = new LineSeries { Title = "Series 2", MarkerType = MarkerType.Square };
            series2.Points.Add(new DataPoint(0, 4));
            series2.Points.Add(new DataPoint(10, 12));
            series2.Points.Add(new DataPoint(20, 16));
            series2.Points.Add(new DataPoint(30, 25));
            series2.Points.Add(new DataPoint(40, 5));

            // Add the series to the plot model.
            tmp.Series.Add(series1);
            tmp.Axes.Add(xAxis);
            tmp.Axes.Add(yAxis);
            _uptimeSeries = series1;
            //tmp.Series.Add(series2);

            PlotModel = tmp;
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
                await _client.SubscribeAsync(_topic);

                Debug.WriteLine("### SUBSCRIBED TO " + _topic + "###");

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
                if (lineRunning.Equals("LINE RUNNING"))
                {
                    _uptimeRunningCount++;
                }
                UptimeRunningPercentage = (double)_uptimeRunningCount / MachineDataCollection.Count * 100;

                _uptimeSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(timeStamp), UptimeRunningPercentage));
                PlotModel.InvalidatePlot(true); // Refresh plot with new data.

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