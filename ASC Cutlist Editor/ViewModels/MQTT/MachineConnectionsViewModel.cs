using AscCutlistEditor.Frameworks;
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
using System.Windows.Threading;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT.MachineMessage;
using Newtonsoft.Json;

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

        /// <summary>
        /// Collection that tracks the visibility of the connection status.
        /// Visibility order: start listening, no connections, connections (1+)
        /// </summary>
        public ObservableCollection<bool> ConnectionVisibility { get; set; } =
            new ObservableCollection<bool>(new[] { true, false, false });

        public MachineConnectionsViewModel(
            SqlConnectionViewModel connModel)
        {
            var mqttFactory = new MqttFactory();
            Server = mqttFactory.CreateMqttServer();
            Listener = mqttFactory.CreateMqttClient();
            ListenerTopic = SubTopic + "/+/+";

            _knownTopics = new HashSet<string>();
            MachineConnections = new ObservableCollection<MachineMessageViewModel>();

            _sqlConnection = connModel;
        }

        public ObservableCollection<MachineMessageViewModel> MachineConnections
        {
            get;
            set;
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
        public void AddTab(string topic)
        {
            // Create a new model for listening to this topic.
            MachineMessageViewModel model =
                new MachineMessageViewModel(topic, _sqlConnection);

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
            if (MachineConnections.Count > 0)
            {
                _knownTopics = new HashSet<string>();
                MachineConnections = new ObservableCollection<MachineMessageViewModel>();

                // Update UI.
                RaisePropertyChangedEvent("MachineConnections");
                ConnectionVisibility[1] = true;
                ConnectionVisibility[2] = false;
            }
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
                // Toggle connection tabs visibility.
                ConnectionVisibility[1] = false;
                ConnectionVisibility[2] = true;

                // Create a new tab if we haven't seen this topic before.
                string topic = e.ApplicationMessage.Topic.Substring(SubTopic.Length);
                if (!_knownTopics.Contains(topic))
                {
                    Debug.WriteLine($"NEW TOPIC SPOTTED: {topic}");
                    _knownTopics.Add(topic);

                    AddTab(topic);
                }
            });

            try
            {
                await Listener.ConnectAsync(options);
                ConnectionVisibility[0] = false;
                ConnectionVisibility[1] = true;
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