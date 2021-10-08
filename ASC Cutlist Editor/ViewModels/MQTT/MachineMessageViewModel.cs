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

        private MachineMessage _latestMachineMessage;
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

        public MachineMessage LatestMachineMessage
        {
            get => _latestMachineMessage;
            set
            {
                _latestMachineMessage = value;
                RaisePropertyChangedEvent("LatestMachineMessage");
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

            LatestMachineMessage = null;

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
            _machineConnection.Client.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### CONNECTED WITH SERVER ###");

                // Test publishing a message.
                await PublishMessage(
                    _machineConnection.Client,
                    "self/success",
                    "Connection successful!");

                // Subscribe to a topic.
                await _machineConnection.Client.SubscribeAsync(_machineConnection.SubTopic);

                Debug.WriteLine("### SUBSCRIBED TO " + _machineConnection.SubTopic + "###");
            });

            // Response to send on receiving a message.
            _machineConnection.Client.UseApplicationMessageReceivedHandler(e =>
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
                    MachineMessage machineMessage = (MachineMessage)JsonConvert.DeserializeObject(payload);
                    if (machineMessage == null)
                    {
                        return;
                    }

                    // Add the data to our graph.
                    AddMachineMessage(machineMessage);

                    // Bryan's code.
                    // BryanCode(machineMessage);

                    // Attempt an acknowledgement response.
                    Task.Run(() => PublishMessage(
                        _machineConnection.Client,
                        "self",
                        $"Successful packet for job number: " +
                        $"{machineMessage.tags.set1.MqttPub.JobNumber}"));
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

        private void AddMachineMessage(MachineMessage message)
        {
            // Run on UI thread.
            // Select the correct dispatcher: if Application.Current is null,
            // we're running unit tests and should use CurrentDispatcher instead.
            Dispatcher dispatcher = Application.Current != null ?
                Application.Current.Dispatcher :
                Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                MachineMessageCollection.Add(message);

                // Calculate percentage for line running bars.
                bool running = false;
                if (message.tags.set1.MqttPub.LineRunning.Equals("LINE RUNNING"))
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

                Debug.WriteLine($"Connection Status: {message.connected}");
                Debug.WriteLine($"Current Job Number: {message.tags.set1.MqttPub.JobNumber}");
                Debug.WriteLine($"Line Status: {message.tags.set1.MqttPub.LineRunning}");
                Debug.WriteLine($"Time Received: {message.timestamp}");
            });

            // Update the latest if our message is newer.
            /*
            if (LatestMachineMessage == null ||
                message.TimeStamp > MachineMessageCollection.Max(d => d.TimeStamp))
            {
                LatestMachineMessage = message;
            } */
        }

        /// <summary>
        /// Putting all the code for the message handler here temporarily.
        /// </summary>
        //TODO: change to task?
        private async void BryanCode(MachineMessage message)
        {
            MachineMessage returnMessage = new MachineMessage();

            // 1. Check job number
            string jobNum = message.tags.set1.MqttPub.JobNumber;
            if (string.IsNullOrEmpty(jobNum))
            {
                return;
            }

            // TODO: break by step into own methods

            // 2. Check order dat req false
            string orderDataRequested = message.tags.set1.MqttPub.OrderDatReq;
            if (orderDataRequested.Equals("FALSE"))
            {
                // Check JN exists

                // Update KPI displays

                // Query SQL.
                DataTable orderNumTable = await Queries.GetOrdersByMachineNum(jobNum);

                // Add contents of orderNumTable into return message.
                string orderData = "";
                foreach (DataRow row in orderNumTable.Rows)
                {
                    string orderNum = row[0].ToString()?.Trim();
                    string material = row[1].ToString()?.Trim();
                    string partNum = row[2].ToString()?.Trim();
                    string orderLen = row[3].ToString()?.Trim();

                    orderData += $"{orderNum},{material},{orderLen},{partNum}|";
                }

                returnMessage.tags.set2.MqttSub.MqttDest = message.tags.set1.MqttPub.JobNumber;
                returnMessage.tags.set2.MqttSub.MqttString = orderData;
            }
            else
            {
                if (!string.IsNullOrEmpty(message.tags.set1.MqttPub.OrderNo))
                {
                    // 3. Else if true and orderno not blank
                    DataTable fullOrderTable = await Queries.GetOrdersByIdAndMachine(
                        message.tags.set1.MqttPub.OrderNo,
                        jobNum);

                    string fullOrderData = "";
                    foreach (DataRow row in fullOrderTable.Rows)
                    {
                        string itemId = row[0].ToString()?.Trim();
                        string length = row[1].ToString()?.Trim();
                        string quantity = row[2].ToString()?.Trim();
                        string bundle = row[3].ToString()?.Trim();

                        fullOrderData += $"{itemId},{length},{quantity},{bundle}|";
                    }

                    returnMessage.tags.set2.MqttSub.OrderDatRecv = fullOrderData;

                    // Query for bundles.
                    DataTable bundlesTable = await Queries.GetBundle(message.tags.set1.MqttPub.OrderNo);

                    string bundleData = "";
                    foreach (DataRow row in bundlesTable.Rows)
                    {
                        for (int i = 0; i < row.ItemArray.Length; i++)
                        {
                            if (i > 0)
                            {
                                bundleData += ",";
                            }
                            bundleData += row[i].ToString()?.Trim();
                        }

                        bundleData += "|";
                    }
                    // TODO: might just turn this into a helper method, since we do it so much.
                    // Might need to reset the order to Bryan's standard?
                }
                else
                {
                    // 4. Else if true and orderno blank
                    // Set flag to clear button.
                    returnMessage.tags.set2.MqttSub.OrderDatAck = "TRUE";
                }
            }

            // 5. Check coil dat req true and scan coil id not empty and order no not empty and order no not no order selected

            // 6. Else if coil dat req false

            // 7. Check if coil store req true and coil usage dat not empty

            // 8. if coil usage send true and coil usage dat

            // 9. write data back to hmi

            // Notes: don't check for null, either empty string or not
        }
    }
}