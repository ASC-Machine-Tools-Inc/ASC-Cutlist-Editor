using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media.Streaming.Adaptive;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class Queries
    {
        /// <summary>
        /// Get the coil data for a specific coil.
        /// </summary>
        /// <param name="coilId">The coil's corresponding id to get data for.</param>
        /// <returns>The data for that coil.</returns>
        public static async Task<DataTable> GetCoilData(string coilId)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

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
        public static async Task<DataTable> GetNonDepletedCoils()
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

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
        public static async Task<DataTable> GetOrdersById(string orderNum)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

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
        public static async Task<DataTable> GetOrdersByMachineNum(string machineId)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

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
        /// <param name="orderNum">The orders' corresponding id to retrieve orders for.</param>
        /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
        /// <returns>A list of orders for the given machine and order number.</returns>
        public static async Task<DataTable> GetOrdersByIdAndMachine(string orderNum, string machineId)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

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
        public static async Task<DataTable> GetBundle(string orderNum)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

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
        private static async Task<DataTable> SelectHelper(
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
        private static async Task<int> SetUsageData(DataTable usageData)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

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