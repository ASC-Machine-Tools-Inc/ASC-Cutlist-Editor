using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class Usage
    {
        /// <summary>
        /// Handle adding the coil usage data to the database when received
        /// from the machine.
        /// </summary>
        /// <returns>The number of rows added to the database.</returns>
        internal static async Task<int> CoilUsageSendFlagHandler(
            ISettings settings,
            MachineMessage message,
            MachineMessage returnMessage)
        {
            MqttPub pub = message.tags.set1.MqttPub;
            MqttSub sub = returnMessage.tags.set2.MqttSub;

            // Check if we have coil usage data requesting to be added.
            if (pub.CoilUsageSend != "TRUE" ||
                pub.CoilUsageDat == null)
            {
                return 0;
            }

            List<List<string>> coilUsageFields = pub.CoilUsageDat
                // Split the usage data into rows.
                .Split("|", StringSplitOptions.RemoveEmptyEntries)
                // Split those rows into a list of fields.
                .Select(usageRows =>
                    new List<string>(usageRows.Split(",")))
                .ToList();

            List<CoilUsage> coilUsageList = coilUsageFields
                // Group the fields into the ones with unique coil and item ids,
                // with the total length for those groupings.
                .GroupBy(
                    usageRow =>
                        (coilId: usageRow[1], itemId: usageRow[3]),
                    (key, fields) =>
                    {
                        List<List<string>> enumerable = fields.ToList();

                        // Grab the first group of fields for order & material.
                        List<string> firstFields = enumerable[0];

                        // Sum the length of all the groups to get the total
                        // length for this coil and item id grouping.
                        decimal totalLength = enumerable.Sum(x =>
                            Convert.ToDecimal(x[4]));

                        return new CoilUsage
                        {
                            orderno = firstFields[0],
                            CoilId = key.coilId,
                            CoilMatl = firstFields[0],
                            ItemID = key.itemId,
                            Length = totalLength,
                            Time = DateTime.Now
                        };
                    })
                .ToList();

            sub.CoilUsageRecvAck = "TRUE";

            // Update the database.
            return await SetUsageData(settings, coilUsageList);
        }

        /// <summary>
        /// Update the usage table by inserting the contents of usageData.
        /// </summary>
        /// <param name="settings">Settings containing table/column names to use.</param>
        /// <param name="usageData">The data to add to the usage table.</param>
        /// <returns>The number of rows added to the usage table.</returns>
        public static async Task<int> SetUsageData(
            ISettings settings,
            List<CoilUsage> usageData)
        {
            if (usageData.Count == 0)
            {
                return 0;
            }

            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string ordernoCol = builder.QuoteIdentifier(settings.UsageOrderNumName);
            string materialCol = builder.QuoteIdentifier(settings.OrderMaterialName);
            string itemIdCol = builder.QuoteIdentifier(settings.UsageItemIdName);
            string totalLengthCol = builder.QuoteIdentifier(settings.UsageLengthName);
            string addDateCol = builder.QuoteIdentifier(settings.UsageDateName);

            // Table name.
            string usageTableName = builder.QuoteIdentifier(settings.UsageTableName);

            string queryStr =
                $"INSERT INTO {usageTableName} " +
                $"({ordernoCol}, {materialCol}, {itemIdCol}, {totalLengthCol}, {addDateCol}) " +
                "VALUES ";

            // Append the fields to add from our DataTable to our SqlCommand text.
            foreach (CoilUsage coil in usageData)
            {
                queryStr += $"('{coil.orderno}', '{coil.CoilMatl}', " +
                            $"'{coil.ItemID}', '{coil.Length}', '{coil.Time}')";
            }

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            conn.Open();
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}