using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class Bundles
    {
        /// <summary>
        /// Get the bundle data for a given order number.
        /// </summary>
        /// <param name="settings">Settings containing table/column names to use.</param>
        /// <param name="orderNum">The order number for the corresponding bundle.</param>
        /// <returns>A list of distinct bundle data for the order number.</returns>
        public static async Task<DataTable> GetBundle(ISettings settings, string orderNum)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string[] bundlesCols = settings.BundleColumns.Split(",");

            // Table name.
            string bundleTableName = builder.QuoteIdentifier(settings.BundleTableName);

            // Filter columns.
            string orderno = builder.QuoteIdentifier(settings.BundleOrderNumName);

            string queryStr =
                $"SELECT DISTINCT ";

            // Add columns.
            queryStr += string.Join(", ", bundlesCols);

            queryStr += $" FROM {bundleTableName} " +
                        $"WHERE {orderno} LIKE @orderNum";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            // Filter parameters.
            cmd.Parameters.AddWithValue("@orderNum", "%" + orderNum + "%");

            return await Utility.SelectHelper(conn, cmd);
        }
    }
}