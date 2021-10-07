using System;
using System.Collections.Generic;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Views.MQTT;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using Newtonsoft.Json;

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

        private readonly SqlConnectionViewModel _sqlConnectionViewModel;

        public static int Port = 1883;
        public static string Ip = "192.168.0.119";
        public static string MainTopic = "alphapub";

        public MachineConnectionsViewModel(
            SqlConnectionViewModel connModel)
        {
            var mqttFactory = new MqttFactory();
            Server = mqttFactory.CreateMqttServer();
            _listener = mqttFactory.CreateMqttClient();
            _listenerTopic = MainTopic + "/+/+";

            _knownTopics = new HashSet<string>();
            _machineConnectionTabs = new ObservableCollection<TabItem>();

            _sqlConnectionViewModel = connModel;
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
                !await _sqlConnectionViewModel.TestConnection())
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
            MachineDataViewModel model =
                new MachineDataViewModel(topic, _sqlConnectionViewModel);
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
                string topic = e.ApplicationMessage.Topic.Substring(MainTopic.Length);
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
    }
}