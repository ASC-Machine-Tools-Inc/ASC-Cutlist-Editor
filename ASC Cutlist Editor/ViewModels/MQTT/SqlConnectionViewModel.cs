using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Properties;
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
        public SqlConnectionStringBuilder Builder;

        private readonly IMqttClient _client;
        private readonly string _subTopic = "alphapub";
        private readonly string _pubTopic = "alphasub";

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
        /// <returns></returns>
        public async Task<bool> TestConnection()
        {
            if (Builder?.ConnectionString == null)
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
        /// Creates a new connection string from the SQL Server connection parameters.
        /// </summary>
        /// <param name="dataSource">
        /// The name or network address of the instance of SQL Server to connect to.
        /// </param>
        /// <param name="databaseName">
        /// The name of the database to connect to.
        /// </param>
        /// <param name="username">
        /// The user ID to use when connecting.
        /// </param>
        /// <param name="password">
        /// The password for the SQL Server account.
        /// </param>
        /// <returns>
        /// True if a connection string was created, false otherwise.
        /// </returns>
        public bool CreateConnectionString(
            string dataSource,
            string databaseName,
            string username,
            string password)
        {
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

            this["DataSource"] = dataSource;
            this["DatabaseName"] = databaseName;
            this["Username"] = username;
            this["Password"] = password;

            return true;
        }

        /// <summary>
        /// Begin listening for and handling MQTT payloads.
        /// </summary>
        public async void StartClient()
        {
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
        /// Get the coil data for a specific coil.
        /// </summary>
        /// <param name="coilId">The coil's corresponding id to get data for.</param>
        /// <returns>The data for that coil.</returns>
        public async Task<DataTable> GetCoilData(string coilId)
        {
            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr =
                "SELECT startlength, lengthused, material " +
                "FROM amscoil " +
                "WHERE coilnumber LIKE @coilID";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "@coilID",
                "%" + coilId + "%");

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get all non-depleted coils.
        /// </summary>
        /// <returns>The list of non-depleted coils.</returns>
        public async Task<DataTable> GetNonDepletedCoils()
        {
            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr = "SELECT coilnumber, description, material, " +
                              "startlength, lengthused " +
                              "FROM amscoil " +
                              "WHERE dateout IS NULL";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the order numbers, material type, and total run length of the
        /// orders for a specific order number.
        /// </summary>
        /// <param name="orderNum">
        /// The order number to retrieve orders for.
        /// </param>
        /// <returns>A list of the orders for a given order number.</returns>
        public async Task<DataTable> GetOrdersById(string orderNum)
        {
            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr =
                "SELECT orderno, material, " +
                "SUM(length * CONVERT(DECIMAL(10,2),quantity)) AS orderlen " +
                "FROM amsorder " +
                "WHERE orderno LIKE @orderNum " +
                "GROUP BY orderno, material";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "@orderNum",
                "%" + orderNum + "%");

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the order numbers, material type, part number, and total run
        /// length of the orders for a specific machine.
        /// </summary>
        /// <param name="machineId">
        /// The machine's corresponding id to retrieve orders for.
        /// </param>
        /// <returns>A list of the orders for a given machine.</returns>
        public async Task<DataTable> GetOrdersByMachineNum(string machineId)
        {
            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr =
                "SELECT orderno, material, partno, " +
                "SUM(length * CONVERT(DECIMAL(10,2),quantity)) AS orderlen " +
                "FROM amsorder " +
                "WHERE orderno IS NOT NULL AND machinenum LIKE @machineID " +
                "GROUP BY orderno, material, partno, machinenum";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "@machineID",
                "%" + machineId + "%");

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the list of orders for the given order number on the given machine.
        /// </summary>
        /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
        /// <param name="orderNum">The orders' corresponding id to retrieve orders for.</param>
        /// <returns>A list of orders for the given machine and order number.</returns>
        public async Task<DataTable> GetOrdersByMachineAndId(string machineId, string orderNum)
        {
            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr =
                "SELECT item_id, length, quantity, bundle " +
                "FROM amsorder " +
                "WHERE machinenum LIKE @machineId AND orderno LIKE @orderNum";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "@machineId",
                "%" + machineId + "%");
            cmd.Parameters.AddWithValue(
                "@orderNum",
                "%" + orderNum + "%");

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the bundle data for a given order number.
        /// </summary>
        /// <param name="orderNum">The order number for the corresponding bundle.</param>
        /// <returns>A list of distinct bundle data for the order number.</returns>
        public async Task<DataTable> GetBundle(string orderNum)
        {
            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr =
                "SELECT DISTINCT material, prodcode, user1, user2, user3, user4, " +
                "custname, custaddr1, custaddr2, custcity, custstate, custzip " +
                "FROM amsbundle " +
                "WHERE orderno LIKE @orderNum";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "@orderNum",
                "%" + orderNum + "%");

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Handles running a SQL command and returning the results.
        /// </summary>
        /// <param name="conn">The connection to execute the command on.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <returns>The results of the select statement.</returns>
        private async Task<DataTable> SelectHelper(
            SqlConnection conn,
            SqlCommand cmd)
        {
            DataTable result = new DataTable();

            try
            {
                conn.Open();
                await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                result.Load(reader);
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    $"SQL exception for query: {cmd.CommandText}.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return result;
        }

        /// <summary>
        /// Update amsproduct by inserting the contents of usageData.
        /// </summary>
        /// <param name="usageData">The data to add to amsproduct.</param>
        /// <returns>The number of rows added to amsproduct.</returns>
        private async Task<int> SetUsageData(DataTable usageData)
        {
            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr =
                "INSERT INTO amsproduct " +
                "(orderno, material, itemid, totallength, adddate) " +
                "VALUES ";

            // Append the fields to add from our DataTable to our SqlCommand text.
            foreach (DataRow row in usageData.Rows)
            {
                string orderNum = row[0].ToString();
                string material = row[2].ToString();
                string itemId = row[3].ToString();
                string totalLength = row[4].ToString();
                string addDate = row[8].ToString();

                queryStr += $"('{orderNum}', '{material}', '{itemId}', " +
                            $"'{totalLength}', '{addDate}')";
            }

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.CommandType = CommandType.Text;

            try
            {
                conn.Open();
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    $"SQL exception for query: {cmd.CommandText}.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            // Query failed, no rows added.
            return 0;
        }
    }
}