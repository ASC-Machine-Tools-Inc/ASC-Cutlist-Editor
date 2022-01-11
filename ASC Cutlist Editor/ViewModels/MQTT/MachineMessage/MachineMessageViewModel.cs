using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.Utility.MQTT;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace AscCutlistEditor.ViewModels.MQTT.MachineMessage
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
        /// Represents the various plots to display for this connection.
        /// </summary>
        public MachineMessagePlotsViewModel Plots { get; set; }

        /// <summary>
        /// Last machine message the client listener received.
        /// </summary>
        public Models.MQTT.MachineMessage LatestMachineMessage
        {
            get => _latestMachineMessage;
            set
            {
                _latestMachineMessage = value;
                RaisePropertyChangedEvent("LatestMachineMessage");
            }
        }

        private Models.MQTT.MachineMessage _latestMachineMessage;

        private SqlConnectionViewModel _sqlConn;

        public MachineMessageViewModel(
            string topic,
            SqlConnectionViewModel connModel)
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

            // Create the plots for the UI.
            Plots = new MachineMessagePlotsViewModel();

            StartClient();  // Listen for continued messages.
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
            MachineConnection.Client.UseApplicationMessageReceivedHandler(async e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                try
                {
                    // Attempt payload conversion and logging.
                    Models.MQTT.MachineMessage machineMessage =
                        JsonConvert.DeserializeObject<Models.MQTT.MachineMessage>(payload);

                    int rowsAdded = await ProcessResponseMessage(machineMessage);
                    if (rowsAdded > 0) { Debug.WriteLine("Rows added: " + rowsAdded); }
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
        /// Handle returning the response message and performing any tasks
        /// based off the flags set in the machine's message.
        /// </summary>
        /// <returns>
        /// The number of rows added for usage.
        /// </returns>
        private async Task<int> ProcessResponseMessage(Models.MQTT.MachineMessage message)
        {
            int rowsAdded = 0;

            // To avoid confusion - the pub here refers to the machine's
            // published message, and sub refers to the response the machine
            // expects. The client publishes the return message to PubTopic.
            MqttPub pub = message.tags?.set1?.MqttPub;
            if (pub == null)
            {
                return rowsAdded;
            }

            Models.MQTT.MachineMessage returnMessage = new Models.MQTT.MachineMessage
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
                return rowsAdded;
            }

            // Update the UI.
            UpdateMachineTab(message);

            try
            {
                // Retrieve the list of table/col name settings.
                ISettings settings = _sqlConn.UserSqlSettings;

                // Handle the order data requested flag (getting the orders and bundles).
                await Orders.OrderDatReqFlagHandler(settings, message, returnMessage);

                // Handle the coil data requested flag (running a specific coil and order).
                await Coils.CoilDatReqFlagHandler(settings, message, returnMessage);

                // Handle the coil list requested flag (all non-depleted coils).
                await Coils.CoilStoreReqFlagHandler(settings, message, returnMessage);

                // Handle the coil usage sending requested flag (write to database).
                rowsAdded = await Usage.CoilUsageSendFlagHandler(settings, message, returnMessage);
            }
            catch (Exception ex)
            {
                // Query error. Close the connection.
                // TODO: set status to sql failed

                Debug.WriteLine("Query error." + ex);
                throw;
            }

            // Finally, write the response message back out for the HMI.
            PublishMessage(
                MachineConnection.Client,
                MachineConnection.PubTopic,
                JsonConvert.SerializeObject(returnMessage));
            MachineConnection.MachineMessagePubCollection.Add(returnMessage);
            return rowsAdded;
        }

        /// <summary>
        /// Update the UI for the corresponding machine from its message data.
        /// </summary>
        internal void UpdateMachineTab(Models.MQTT.MachineMessage message)
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

                // Add new message data to plots.
                Plots.UpdatePlots(message);
            });
        }
    }
}