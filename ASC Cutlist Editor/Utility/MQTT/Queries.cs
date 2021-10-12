using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    // Collection of queries that handle data for machine messages.
    public class Queries
    {
        /// <summary>
        /// Get the starting length, used length, and material for a specific coil.
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
        /// Get the coil number, description, material, starting length, and
        /// length used for all non-depleted coils.
        /// </summary>
        /// <returns>The list of non-depleted coils.</returns>
        public static async Task<DataTable> GetNonDepletedCoils()
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            string queryStr = "SELECT coilnumber, description, material, " +
                              "(CONVERT(DECIMAL(10,2),startlength) - " +
                              "CONVERT(DECIMAL(10,2),lengthused)) AS currlength " +
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
                "SELECT orderno, material, " +
                "SUM(length * CONVERT(DECIMAL(10,2),quantity)) AS orderlen, partno " +
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
        public static async Task<int> SetUsageData(DataTable usageData)
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

        /// <summary>
        /// Converts a data table into a string where fields are separated with
        /// commas and rows are separated with vertical bars for the HMI to parse.
        /// Example result: row1-field1,row1-field2|row2-field1,row2-field2
        /// </summary>
        /// <param name="table">The table to convert.</param>
        /// <returns>The string representation of the table for HMIs.</returns>
        public static string DataTableToString(DataTable table)
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