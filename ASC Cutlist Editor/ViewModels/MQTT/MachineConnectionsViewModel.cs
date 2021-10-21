using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Views.MQTT;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AscCutlistEditor.Models.MQTT;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// Handles listening for new machines on alphapub and creating uptime
    /// status tabs for them.
    /// </summary>
    public class MachineConnectionsViewModel : ObservableObject
    {
        internal readonly IMqttServer Server;
        internal readonly IMqttClient Listener;
        internal string ListenerTopic;

        private HashSet<string> _knownTopics;
        private ObservableCollection<MachineMessageViewModel> _machineConnections;

        private readonly SqlConnectionViewModel _sqlConnection;

        public static int Port = 1883;
        public static string Ip = GetLocalIpAddress();

        /// <summary>
        /// The topic to listen for machine messages on.
        /// </summary>
        public static string SubTopic = "alphapub";

        /// <summary>
        /// The topic to publish our message responses to.
        /// </summary>
        public static string PubTopic = "alphasub";

        public MachineConnectionsViewModel(
            SqlConnectionViewModel connModel)
        {
            var mqttFactory = new MqttFactory();
            Server = mqttFactory.CreateMqttServer();
            Listener = mqttFactory.CreateMqttClient();
            ListenerTopic = SubTopic + "/+/+";

            _knownTopics = new HashSet<string>();
            _machineConnections = new ObservableCollection<MachineMessageViewModel>();

            _sqlConnection = connModel;
        }

        public ObservableCollection<MachineMessageViewModel> MachineConnections
        {
            get => _machineConnections;
            set
            {
                _machineConnections = value;
                RaisePropertyChangedEvent("MachineConnections");
            }
        }

        /// <summary>
        /// Start the server and client for listening for new connections.
        /// </summary>
        /// <param name="checkConnection">
        /// Skip checking the SQL connection for testing/situations where you
        /// don't want to connect to the database.
        /// </param>
        public async Task Start(bool checkConnection = true)
        {
            // Check that the server isn't already running, and that we have
            // a valid connection string to connect to.
            if (Server.IsStarted ||
                checkConnection && !await _sqlConnection.TestConnection())
            {
                return;
            }

            var options = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(Port)
                .Build();
            await Server.StartAsync(options);

            // Start the MQTTClient to listen for new connections.
            await StartListener();
        }

        /// <summary>
        /// Add a new TabItem to track the KPI data for that connection.
        /// </summary>
        /// <param name="topic">The topic this payload came through on.</param>
        /// <param name="payload">The first message from the machine.</param>
        public void AddTab(string topic, string payload)
        {
            // Create a new model for listening to this topic.
            MachineMessageViewModel model =
                new MachineMessageViewModel(topic, _sqlConnection, payload);

            // Run on UI thread.
            // Select the correct dispatcher: if Application.Current is null,
            // we're running unit tests and should use CurrentDispatcher instead.
            Dispatcher dispatcher = Application.Current != null ?
                Application.Current.Dispatcher :
                Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                MachineConnections.Add(model);
            });
        }

        /// <summary>
        /// Clear all connections and the UI. The client is still listening,
        /// so all active connections will reappear while inactive ones won't.
        /// </summary>
        public void Refresh()
        {
            _knownTopics = new HashSet<string>();
            MachineConnections = new ObservableCollection<MachineMessageViewModel>();
        }

        private async Task StartListener()
        {
            // Create MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(Ip, Port)
                .Build();

            // Response to send on connection.
            Listener.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### LISTENER CONNECTED ###");

                // Subscribe to a topic.
                await Listener.SubscribeAsync(ListenerTopic);
            });

            // Response to send on receiving a message.
            Listener.UseApplicationMessageReceivedHandler(e =>
            {
                // Create a new tab if we haven't seen this topic before.
                string topic = e.ApplicationMessage.Topic.Substring(SubTopic.Length);
                if (!_knownTopics.Contains(topic))
                {
                    Debug.WriteLine($"NEW TOPIC SPOTTED: {topic}");
                    _knownTopics.Add(topic);
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    AddTab(topic, payload);

                    Debug.WriteLine("### ADDING TOPIC " + topic + " ###");
                }
            });

            try
            {
                await Listener.ConnectAsync(options);
            }
            catch
            {
                Debug.WriteLine("Connection unsuccessful.");
            }
        }

        // Get the local IP address for use in our MQTT broker.
        private static string GetLocalIpAddress()
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address.ToString();
        }
    }
}