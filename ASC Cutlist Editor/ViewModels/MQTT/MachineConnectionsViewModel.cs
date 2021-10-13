using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Views.MQTT;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// Handles listening for new machines on alphapub and creating uptime
    /// status tabs for them.
    /// </summary>
    internal class MachineConnectionsViewModel : ObservableObject
    {
        internal readonly IMqttServer Server;
        private readonly IMqttClient _listener;
        private readonly string _listenerTopic;

        private HashSet<string> _knownTopics;
        private ObservableCollection<TabItem> _machineConnectionTabs;

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
            _listener = mqttFactory.CreateMqttClient();
            _listenerTopic = SubTopic + "/+/+";

            _knownTopics = new HashSet<string>();
            _machineConnectionTabs = new ObservableCollection<TabItem>();

            _sqlConnection = connModel;
        }

        public ObservableCollection<TabItem> MachineConnectionTabs
        {
            get => _machineConnectionTabs;
            set
            {
                _machineConnectionTabs = value;
                RaisePropertyChangedEvent("MachineConnectionTabs");
            }
        }

        /// <summary>
        /// Start the server and client for listening for new connections.
        /// </summary>
        public async void Start()
        {
            // Check that the server isn't already running, and that we have
            // a valid connection string to connect to.
            if (Server.IsStarted ||
                !await _sqlConnection.TestConnection())
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
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task AddTab(string topic)
        {
            // Create a new model for listening to this topic.
            MachineMessageViewModel model =
                new MachineMessageViewModel(topic, _sqlConnection);
            await model.StartClient();

            Dispatcher dispatcher = Application.Current != null ?
                Application.Current.Dispatcher :
                Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                MachineConnectionTabs.Add(new TabItem
                {
                    Header = topic.Replace("/", ""),
                    Content = new UptimeControl(),
                    DataContext = model
                });
            });
        }

        /// <summary>
        /// Clear all connections and the UI. The client is still listening,
        /// so all active connections will reappear while inactive ones won't.
        /// </summary>
        public void Refresh()
        {
            _knownTopics = new HashSet<string>();
            MachineConnectionTabs = new ObservableCollection<TabItem>();
        }

        private async Task StartListener()
        {
            // Create MQTT client.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(Ip, Port)
                .Build();

            // Response to send on connection.
            _listener.UseConnectedHandler(async e =>
            {
                Debug.WriteLine("### LISTENER CONNECTED ###");

                // Subscribe to a topic.
                await _listener.SubscribeAsync(_listenerTopic);
            });

            // Response to send on receiving a message.
            _listener.UseApplicationMessageReceivedHandler(async e =>
            {
                // Create a new tab if we haven't seen this topic before.
                string topic = e.ApplicationMessage.Topic.Substring(SubTopic.Length);
                if (!_knownTopics.Contains(topic))
                {
                    _knownTopics.Add(topic);
                    await AddTab(topic);

                    Debug.WriteLine("### ADDING TOPIC " + topic + " ###");
                }
            });

            try
            {
                await _listener.ConnectAsync(options);
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