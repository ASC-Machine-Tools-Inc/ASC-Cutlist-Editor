using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.Security.Cryptography.Core;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Properties;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// This handles Bryan's code - connecting to the database, querying,
    /// and updating the ams data.
    /// </summary>
    internal class SqlConnectionViewModel : ObservableObject
    {
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
        public void Save()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(
                ConfigurationUserLevel.PerUserRoamingAndLocal);
            ConfigurationSection userSettings = config.GetSection("userSettings/AscCutlistEditor.Properties.Settings");
            userSettings.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
            config.Save(ConfigurationSaveMode.Full);
        }

        public SqlConnectionStringBuilder Builder;

        public async void CreateConnectionString(
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
                return;
            }

            Builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = databaseName,
                UserID = username,
                Password = password
            };
            Debug.WriteLine(Builder.ConnectionString);

            // SqlConnectionViewModel["DataSource"] = dataSource;

            Settings.Default.DataSource = dataSource;
            Settings.Default.DatabaseName = databaseName;
            Settings.Default.Username = username;
            Settings.Default.Password = password;
            Save();

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

                // Test query.
                DataTable table = await GetCoil("21");

                Debug.WriteLine("Writing GetOrders rows.");
                foreach (DataRow row in table.Rows)
                {
                    Debug.WriteLine(string.Join(", ", row.ItemArray));
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
            finally
            {
                // Reset to default.
                Mouse.OverrideCursor = null;
            }
        }

        // TODO: message received handler

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
            DataTable result = new DataTable();

            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr = "SELECT orderno, material, partno, " +
                              "SUM(length * CONVERT(DECIMAL(10,2),quantity)) AS orderlen " +
                              "FROM amsorder " +
                              "WHERE orderno IS NOT NULL AND " +
                              "machinenum LIKE @machineID " +
                              "GROUP BY orderno, material, partno, machinenum";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "@machineID",
                "%" + machineId + "%");

            try
            {
                conn.Open();
                await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                result.Load(reader);
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    $"SQL exception for GetOrders({machineId}).",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return result;
        }

        /// <summary>
        /// Get the order numbers, material type, and total run length of the
        /// orders for a specific order number.
        /// </summary>
        /// <param name="orderId">
        /// The order number to retrieve orders for.
        /// </param>
        /// <returns>A list of the orders for a given order number.</returns>
        public async Task<DataTable> GetOrdersByOrderNo(string orderId)
        {
            DataTable result = new DataTable();

            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr = "SELECT orderno, material, " +
                              "SUM(length * CONVERT(DECIMAL(10,2),quantity)) AS orderlen " +
                              "FROM amsorder " +
                              "WHERE orderno LIKE @orderId " +
                              "GROUP BY orderno, material";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "orderId",
                "%" + orderId + "%");

            try
            {
                conn.Open();
                await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                result.Load(reader);
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    $"SQL exception for GetOrdersLen({orderId}).",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return result;
        }

        /// <summary>
        /// Get the coil data for a specific coil.
        /// </summary>
        /// <param name="coilId">The coil's corresponding id to get data for.</param>
        /// <returns>The data for that coil.</returns>
        public async Task<DataTable> GetCoil(string coilId)
        {
            DataTable result = new DataTable();

            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr = "SELECT startlength as startlen, lengthused as lenused, " +
                              "material as matl from amscoil where coilnumber like @coilID";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "@coilID",
                "%" + coilId + "%");

            try
            {
                conn.Open();
                await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                result.Load(reader);
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    $"SQL exception for GetCoil({coilId}).",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return result;
        }

        /*
        /// <summary>
        /// Get the coil data for a specific coil.
        /// </summary>
        /// <returns>The data for that coil</returns>
        public async Task<DataTable> GetCoil()
        {
            DataTable result = new DataTable();

            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);

            string queryStr = "SELECT startlength as startlen, lengthused as lenused, " +
                              "material as matl from amscoil where coilnumber like @coilID";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue(
                "@coilID",
                "%" + coilId + "%");

            try
            {
                conn.Open();
                await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                result.Load(reader);
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    $"SQL exception for GetCoil({coilId}).",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return result;
        }

        /*
        /// <summary>
        /// Get the list of coils that aren't currently depleted for displaying on an HMI.
        /// </summary>
        /// <returns>A list of non-depleted coils.</returns>
        public IQueryable<dynamic> GetCoils()
        {
            return amscoil
                .Select(x => new
                {
                    x.coilnumber,
                    x.description,
                    x.material,
                    x.startlength,
                    x.lengthused
                })
                .Where(x => x.dateout == null);
        }

        /// <summary>
        /// Get the list of orders for the given order number on the given machine.
        /// </summary>
        /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
        /// <param name="orderNum">The orders' corresponding id to retrieve orders for.</param>
        /// <returns>A list of orders for the given machine and order number.</returns>
        public IQueryable<dynamic> GetCoils(string machineId, string orderNum)
        {
            return amsorder
                .Select(x => new
                {
                    x.item_id,
                    x.length,
                    x.quantity,
                    x.bundle
                })
                .Where(x => x.machinenum.equals(machineId) && x.orderno.equals(orderNum));
        }

        /// <summary>
        /// Get the bundle data for a given order number.
        /// </summary>
        /// <param name="orderNum">The order number for the corresponding bundle.</param>
        /// <returns>A list of distinct bundle data for the order number.</returns>
        public IQueryable<dynamic> GetBundle(string orderNum)
        {
            return amsbundle
                .Select(x => new
                {
                    x.material,
                    x.prodcode,
                    x.user1,
                    x.user2,
                    x.user3,
                    x.user4,
                    x.custname,
                    x.custaddr1,
                    x.custaddr2,
                    x.custcity,
                    x.custstate,
                    x.custzip
                })
                .Distinct()
                .Where(x => x.orderno.equals(orderNum));
        }

        /*
/// <summary>
/// Update amsproduct by inserting the contents of usageData.
/// </summary>
/// <param name="usageData">The data to add to amsproduct.</param>
private async void SetUsageData(IQueryable<dynamic> usageData)
{
    foreach (var product in usageData)
    {
        // Something along these lines - need model for product, set up context
        //_context.TimeLog.Add(TimeLog);
        //await _context.SaveChangesAsync();
    }
}

*/
    }
}