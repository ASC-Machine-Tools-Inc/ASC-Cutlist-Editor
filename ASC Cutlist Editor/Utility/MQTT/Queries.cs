using AscCutlistEditor.ViewModels.MQTT;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using AscCutlistEditor.Frameworks;

// ReSharper disable All

namespace AscCutlistEditor.Utility.MQTT
{
    // Collection of queries that handle data for machine messages.
    public class Queries
    {
        private readonly ISettings _settings;

        public Queries(ISettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Get the starting length, used length, and material for a specific coil.
        /// </summary>
        /// <param name="coilId">The coil's corresponding id to get data for.</param>
        /// <returns>The data for that coil.</returns>
        public async Task<DataTable> GetCoilData(string coilId)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string startlength = builder.QuoteIdentifier(_settings.CoilStartLengthName);
            string lengthused = builder.QuoteIdentifier(_settings.CoilLengthUsedName);
            string material = builder.QuoteIdentifier(_settings.CoilMaterialName);

            // Table name.
            string coilTableName = builder.QuoteIdentifier(_settings.CoilTableName);

            // Filter columns.
            string coilnumber = builder.QuoteIdentifier(_settings.CoilNumberName);

            string queryStr =
                $"SELECT {startlength}, {lengthused}, {material} " +
                $"FROM {coilTableName} " +
                $"WHERE {coilnumber} LIKE @coilID";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;

            // Filter parameters.
            cmd.Parameters.AddWithValue("@coilID", "%" + coilId + "%");

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the coil number, description, material, starting length, and
        /// length used for all non-depleted coils.
        /// </summary>
        /// <returns>The list of non-depleted coils.</returns>
        public async Task<DataTable> GetNonDepletedCoils()
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string coilnumber = builder.QuoteIdentifier(_settings.CoilNumberName);
            string description = builder.QuoteIdentifier(_settings.CoilDescriptionName);
            string material = builder.QuoteIdentifier(_settings.CoilMaterialName);
            string startlength = builder.QuoteIdentifier(_settings.CoilStartLengthName);
            string lengthused = builder.QuoteIdentifier(_settings.CoilLengthUsedName);

            // Table name.
            string coilTableName = builder.QuoteIdentifier(_settings.CoilTableName);

            // Filter columns.
            string dateout = builder.QuoteIdentifier(_settings.CoilDateName);

            string queryStr = $"SELECT {coilnumber}, {description}, {material}, " +
                              $"(CONVERT(DECIMAL(10,2),{startlength}) - " +
                              $"CONVERT(DECIMAL(10,2),{lengthused})) AS currlength " +
                              $"FROM {coilTableName} " +
                              $"WHERE {dateout} IS NULL";
            Debug.WriteLine(queryStr);

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
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string orderno = builder.QuoteIdentifier(_settings.OrderNumName);
            string material = builder.QuoteIdentifier(_settings.OrderMaterialName);
            string length = builder.QuoteIdentifier(_settings.OrderLengthName);
            string quantity = builder.QuoteIdentifier(_settings.OrderQuantityName);

            // Table name.
            string orderTableName = builder.QuoteIdentifier(_settings.OrderTableName);

            string queryStr =
                $"SELECT {orderno}, {material}, " +
                $"SUM({length} * CONVERT(DECIMAL(10,2),{quantity})) AS orderlen " +
                $"FROM {orderTableName} " +
                $"WHERE {orderno} LIKE @orderNum " +
                $"GROUP BY {orderno}, {material}";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;

            // Filter parameters.
            cmd.Parameters.AddWithValue("@orderNum", "%" + orderNum + "%");

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
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string orderno = builder.QuoteIdentifier(_settings.OrderNumName);
            string material = builder.QuoteIdentifier(_settings.OrderMaterialName);
            string length = builder.QuoteIdentifier(_settings.OrderLengthName);
            string quantity = builder.QuoteIdentifier(_settings.OrderQuantityName);
            string partno = builder.QuoteIdentifier(_settings.OrderPartNumName);

            // Table name.
            string orderTableName = builder.QuoteIdentifier(_settings.OrderTableName);

            // Filter columns.
            string machinenum = builder.QuoteIdentifier(_settings.OrderMachineNumName);

            string queryStr =
                $"SELECT {orderno}, {material}, " +
                $"SUM({length} * CONVERT(DECIMAL(10,2),{quantity})) AS orderlen, " +
                $"{partno} " +
                $"FROM {orderTableName} " +
                $"WHERE {orderno} IS NOT NULL AND {machinenum} LIKE @machineID " +
                $"GROUP BY {orderno}, {material}, {partno}, {machinenum}";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;

            // Filter parameters.
            cmd.Parameters.AddWithValue("@machineID", "%" + machineId + "%");

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the list of orders for the given order number on the given machine.
        /// </summary>
        /// <param name="orderNum">The orders' corresponding id to retrieve orders for.</param>
        /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
        /// <returns>A list of orders for the given machine and order number.</returns>
        public async Task<DataTable> GetOrdersByIdAndMachine(string orderNum, string machineId)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string itemId = builder.QuoteIdentifier(_settings.OrderItemIdName);
            string length = builder.QuoteIdentifier(_settings.OrderLengthName);
            string quantity = builder.QuoteIdentifier(_settings.OrderQuantityName);
            string bundle = builder.QuoteIdentifier(_settings.OrderBundleName);

            // Table name.
            string orderTableName = builder.QuoteIdentifier(_settings.OrderTableName);

            // Filter columns.
            string machinenum = builder.QuoteIdentifier(_settings.OrderMachineNumName);
            string orderno = builder.QuoteIdentifier(_settings.OrderNumName);

            string queryStr =
                $"SELECT {itemId}, {length}, {quantity}, {bundle} " +
                $"FROM {orderTableName} " +
                $"WHERE {machinenum} LIKE @machineId AND {orderno} LIKE @orderNum";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;

            // Filter parameters.
            cmd.Parameters.AddWithValue("@machineID", "%" + machineId + "%");
            cmd.Parameters.AddWithValue("@orderNum", "%" + orderNum + "%");

            return await SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the bundle data for a given order number.
        /// </summary>
        /// <param name="orderNum">The order number for the corresponding bundle.</param>
        /// <returns>A list of distinct bundle data for the order number.</returns>
        public async Task<DataTable> GetBundle(string orderNum)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string[] bundlesCols = _settings.BundleColumns.Split(",");

            // Table name.
            string bundleTableName = builder.QuoteIdentifier(_settings.BundleTableName);

            // Filter columns.
            string orderno = builder.QuoteIdentifier(_settings.BundleOrderNumName);

            string queryStr =
                $"SELECT DISTINCT ";

            // Add columns.
            queryStr += string.Join(", ", bundlesCols);

            queryStr += $" FROM {bundleTableName} " +
                $"WHERE {orderno} LIKE @orderNum";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.CommandType = CommandType.Text;

            // Filter parameters.
            cmd.Parameters.AddWithValue("@orderNum", "%" + orderNum + "%");

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

            conn.Open();
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            result.Load(reader);

            return result;
        }

        /// <summary>
        /// Update amsproduct by inserting the contents of usageData.
        /// </summary>
        /// <param name="usageData">The data to add to amsproduct.</param>
        /// <returns>The number of rows added to amsproduct.</returns>
        public async Task<int> SetUsageData(DataTable usageData)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string ordernoCol = builder.QuoteIdentifier(_settings.UsageOrderNumName);
            string materialCol = builder.QuoteIdentifier(_settings.OrderMaterialName);
            string itemIdCol = builder.QuoteIdentifier(_settings.UsageItemIdName);
            string totallengthCol = builder.QuoteIdentifier(_settings.UsageLengthName);
            string adddateCol = builder.QuoteIdentifier(_settings.UsageDateName);

            // Table name.
            string usageTableName = builder.QuoteIdentifier(_settings.UsageTableName);

            string queryStr =
                $"INSERT INTO {usageTableName} " +
                $"({ordernoCol}, {materialCol}, {itemIdCol}, {totallengthCol}, {adddateCol}) " +
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

            conn.Open();
            return await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Converts a data table into a string where fields are separated with
        /// commas and rows are separated with vertical bars for the HMI to parse.
        /// Example result: row1-field1,row1-field2|row2-field1,row2-field2
        /// </summary>
        /// <param name="table">The table to convert.</param>
        /// <returns>The string representation of the table for HMIs.</returns>
        public string DataTableToString(DataTable table)
        {
            string result = "";

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                if (i > 0) result += "|";

                for (int j = 0; j < row.ItemArray.Length; j++)
                {
                    if (j > 0) result += ",";
                    result += row[j].ToString()?.Trim();
                }
            }

            return result;
        }
    }
}