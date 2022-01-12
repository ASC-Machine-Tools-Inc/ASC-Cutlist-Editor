using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class Utility
    {
        /// <summary>
        /// Handles running a SQL command and returning the results.
        /// </summary>
        /// <param name="conn">The connection to execute the command on.</param>
        /// <param name="cmd">The command to execute.</param>
        /// <returns>The results of the select statement.</returns>
        public static async Task<DataTable> SelectHelper(
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