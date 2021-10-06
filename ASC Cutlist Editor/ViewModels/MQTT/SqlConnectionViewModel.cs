using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.UI.Notifications;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Properties;
using AscCutlistEditor.Utility.MQTT;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// This handles Bryan's code - connecting to the database, querying,
    /// and updating the ams data.
    /// </summary>
    internal class SqlConnectionViewModel : ObservableObject
    {
        public static SqlConnectionStringBuilder Builder;

        private readonly IMqttClient _client;
        private readonly string _subTopic = "alphapub";
        private readonly string _pubTopic = "alphasub";

        public SqlConnectionViewModel()
        {
            _client = new MqttFactory().CreateMqttClient();
        }

        public object this[string settingsName]
        {
            get => Settings.Default[settingsName];
            set
            {
                Settings.Default[settingsName] = value;
                RaisePropertyChangedEvent(settingsName);
            }
        }

        // Save the user settings and encrypt them.
        public static void Save()
        {
            string sectionName = "userSettings/AscCutlistEditor.Properties.Settings";
            string protectionProvider = "DataProtectionConfigurationProvider";

            Configuration config = ConfigurationManager.OpenExeConfiguration(
                ConfigurationUserLevel.PerUserRoamingAndLocal);
            ConfigurationSection userSettings = config.GetSection(sectionName);
            userSettings.SectionInformation.ProtectSection(protectionProvider);

            config.Save();
            Settings.Default.Save();
        }

        /// <summary>
        /// Attempt opening a connection to the current connection string in Builder.
        /// </summary>
        /// <returns>Returns true if the connection was successful, false otherwise.</returns>
        public async Task<bool> TestConnection()
        {
            // Check that we can create a connection string.
            if (!CreateConnectionString())
            {
                return false;
            }

            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);
            try
            {
                // Set the loading cursor while connecting.
                Mouse.OverrideCursor = Cursors.Wait;

                await conn.OpenAsync();

                // Show if connection successful.
                MessageBox.Show(
                    "Connection successful!",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Error connecting to the server.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            finally
            {
                // Reset to default.
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Begin listening for and handling MQTT payloads.
        /// </summary>
        public async void StartClient()
        {
            // Check that we can create a connection string.
            if (!CreateConnectionString())
            {
                return;
            }

            // TODO: message received handler
            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);
            try
            {
                await conn.OpenAsync();

                // Start listener for MQTT messages.
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(
                        MachineConnectionsViewModel.Ip,
                        MachineConnectionsViewModel.Port)
                    .Build();

                // Response to send on connection.
                _client.UseConnectedHandler(async e =>
                {
                    Debug.WriteLine("### ALPHA CUTLIST - CONNECTED WITH SERVER ###");

                    // Test publishing a message.
                    await MachineDataViewModel.PublishMessage(
                        _client,
                        "self/success",
                        "Alpha cutlist generator connection successful!");

                    // Subscribe to a topic.
                    await _client.SubscribeAsync(_subTopic);

                    Debug.WriteLine("### SUBSCRIBED TO " + _subTopic + "###");
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
                            // Attempt an acknowledgement response.
                            Task.Run(() => MachineDataViewModel.PublishMessage(
                                _client,
                                _pubTopic,
                                $"Successful packet for job number: {machineData.SelectToken("tags.set1.MqttPub.JobNumber")}"));
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
            catch (SqlException)
            {
                MessageBox.Show(
                    "Error connecting to the server.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Creates a new connection string from the SQL Server connection parameters.
        /// </summary>
        /// <returns>
        /// True if a connection string was created, false otherwise.
        /// </returns>
        private bool CreateConnectionString()
        {
            string dataSource = (string)this["DataSource"];
            string databaseName = (string)this["DatabaseName"];
            string username = (string)this["Username"];
            string password = (string)this["Password"];

            if (dataSource == null || databaseName == null ||
                username == null || password == null)
            {
                MessageBox.Show(
                    "Please fill out all the fields.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            Builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = databaseName,
                UserID = username,
                Password = password
            };

            return true;
        }
    }
}