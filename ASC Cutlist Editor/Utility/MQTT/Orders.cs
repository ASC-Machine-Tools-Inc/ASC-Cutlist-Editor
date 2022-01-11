using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class Orders
    {
        /// <summary>
        /// Handle updating the display and returning order data depending on
        /// the set flags from the received machine message.
        /// </summary>
        internal static async Task OrderDatReqFlagHandler(
            ISettings settings,
            MachineMessage message,
            MachineMessage returnMessage)
        {
            MqttPub pub = message.tags.set1.MqttPub;
            MqttSub sub = returnMessage.tags.set2.MqttSub;

            if (pub.OrderDatReq == "TRUE" && pub.OrderNo != null)
            {
                // Retrieve order data.
                DataTable fullOrderTable = await GetOrdersByIdAndMachine(
                    settings,
                    pub.OrderNo,
                    pub.JobNumber);
                sub.OrderDatRecv = Utility.DataTableToString(fullOrderTable);

                // Retrieve bundle data.
                DataTable bundlesTable = await Bundles.GetBundle(
                    settings,
                    pub.OrderNo);
                sub.BundleDatRecv = Utility.DataTableToString(bundlesTable);

                sub.OrderDatAck = "TRUE";
                sub.OrderNo = pub.OrderNo;
            }
            else if (pub.OrderDatReq == "TRUE")
            {
                // If data was requested but no order number was included,
                // set the flag to clear the button.
                sub.OrderDatAck = "TRUE";
            }
            else if (pub.OrderDatReq == "FALSE")
            {
                // Otherwise, respond with the current job numbers.
                DataTable orderNumTable = await GetOrdersByMachineNum(
                    settings,
                    pub.JobNumber);

                // Apply order data filters to orders.
                orderNumTable = OrderDatFiltersFlagHandler(settings, pub, orderNumTable);

                sub.MqttString = Utility.DataTableToString(orderNumTable);
            }

            // Set the destination as a workaround so the data always gets written.
            sub.MqttDest = pub.JobNumber;
        }

        /// <summary>
        /// Apply the given order data filters to the order table.
        /// </summary>
        /// <returns>The order data table with the filters applied.</returns>
        internal static DataTable OrderDatFiltersFlagHandler(
            ISettings settings,
            MqttPub pub,
            DataTable orderDat)
        {
            string[] filters = pub.OrderDatFilters.Split(",");
            string scheduledDate = settings.OrderScheduledDateName;

            foreach (string filter in filters)
            {
                string[] filterPair = filter.Split(":");

                // Convert orderDat to enumerable rows so we can perform LINQ
                // operations on them.
                var orders = orderDat.AsEnumerable();

                switch (filterPair[0])
                {
                    case "OrderDate": // Get orders scheduled from today to given amount of days in future.
                        int daysToAdd = int.Parse(filterPair[1]);

                        orders = orders.Where(o =>
                            o.Field<DateTime>(scheduledDate) >= DateTime.Now &&
                            o.Field<DateTime>(scheduledDate) <= DateTime.Now.AddDays(daysToAdd));
                        break;

                    case "OrderLen": // Sort orders by length, short to long or long to short.
                        orders = filterPair[1] == "SL" ?
                            orders.OrderBy(o => o.Field<decimal>("orderlen")) : // Short to long.
                            orders.OrderByDescending(o => o.Field<decimal>("orderlen")); // LS, long to short.
                        break;

                    case "CoilMatlID": // Return orders matching the given material ID.
                        string material = settings.OrderMaterialName;
                        orders = orders.Where(o => o.Field<string>(material) == filterPair[1]);
                        break;
                }

                if (orders.Any()) // Convert orders back to data table.
                {
                    orderDat = orders.CopyToDataTable();
                }
                else // Empty orders, clear the table instead.
                {
                    orderDat.Clear();
                    break;
                }
            }

            return orderDat;
        }

        /// <summary>
        /// Get the order numbers, material type, and total run length of the
        /// orders for a specific order number.
        /// </summary>
        /// <param name="settings">Settings containing table/column names to use.</param>
        /// <param name="orderNum">
        /// The order number to retrieve orders for.
        /// </param>
        /// <returns>A list of the orders for a given order number.</returns>
        public static async Task<DataTable> GetOrdersById(ISettings settings, string orderNum)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string orderno = builder.QuoteIdentifier(settings.OrderNumName);
            string material = builder.QuoteIdentifier(settings.OrderMaterialName);
            string length = builder.QuoteIdentifier(settings.OrderLengthName);
            string quantity = builder.QuoteIdentifier(settings.OrderQuantityName);

            // Table name.
            string orderTableName = builder.QuoteIdentifier(settings.OrderTableName);

            string queryStr =
                $"SELECT {orderno}, {material}, " +
                $"SUM({length} * CONVERT(DECIMAL(10,2),{quantity})) AS orderlen " +
                $"FROM {orderTableName} " +
                $"WHERE {orderno} LIKE @orderNum " +
                $"GROUP BY {orderno}, {material}";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            // Filter parameters.
            cmd.Parameters.AddWithValue("@orderNum", "%" + orderNum + "%");

            return await Utility.SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the order numbers, material type, part number, and total run
        /// length of the orders for a specific machine.
        /// </summary>
        /// <param name="settings">Settings containing table/column names to use.</param>
        /// <param name="machineId">
        /// The machine's corresponding id to retrieve orders for.
        /// </param>
        /// <returns>A list of the orders for a given machine.</returns>
        public static async Task<DataTable> GetOrdersByMachineNum(ISettings settings, string machineId)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string orderno = builder.QuoteIdentifier(settings.OrderNumName);
            string material = builder.QuoteIdentifier(settings.OrderMaterialName);
            string length = builder.QuoteIdentifier(settings.OrderLengthName);
            string quantity = builder.QuoteIdentifier(settings.OrderQuantityName);
            string partno = builder.QuoteIdentifier(settings.OrderPartNumName);
            string scheduledDate = builder.QuoteIdentifier(settings.OrderScheduledDateName);

            // Table name.
            string orderTableName = builder.QuoteIdentifier(settings.OrderTableName);

            // Filter columns.
            string machinenum = builder.QuoteIdentifier(settings.OrderMachineNumName);

            string queryStr =
                $"SELECT {orderno}, {material}, " +
                $"SUM({length} * CONVERT(DECIMAL(10,2),{quantity})) AS orderlen, " +
                $"{partno}, {scheduledDate} " +
                $"FROM {orderTableName} " +
                $"WHERE {orderno} IS NOT NULL AND {machinenum} LIKE @machineID " +
                $"GROUP BY {orderno}, {material}, {partno}, {machinenum}, {scheduledDate}";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            // Filter parameters.
            cmd.Parameters.AddWithValue("@machineID", "%" + machineId + "%");

            return await Utility.SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the list of orders for the given order number on the given machine.
        /// </summary>
        /// <param name="settings">Settings containing table/column names to use.</param>
        /// <param name="orderNum">The orders' corresponding id to retrieve orders for.</param>
        /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
        /// <returns>A list of orders for the given machine and order number.</returns>
        public static async Task<DataTable> GetOrdersByIdAndMachine(
            ISettings settings,
            string orderNum,
            string machineId)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string itemId = builder.QuoteIdentifier(settings.OrderItemIdName);
            string length = builder.QuoteIdentifier(settings.OrderLengthName);
            string quantity = builder.QuoteIdentifier(settings.OrderQuantityName);
            string bundle = builder.QuoteIdentifier(settings.OrderBundleName);

            // Table name.
            string orderTableName = builder.QuoteIdentifier(settings.OrderTableName);

            // Filter columns.
            string machinenum = builder.QuoteIdentifier(settings.OrderMachineNumName);
            string orderno = builder.QuoteIdentifier(settings.OrderNumName);

            string queryStr =
                $"SELECT {itemId}, {length}, {quantity}, {bundle} " +
                $"FROM {orderTableName} " +
                $"WHERE {machinenum} LIKE @machineId AND {orderno} LIKE @orderNum";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            // Filter parameters.
            cmd.Parameters.AddWithValue("@machineID", "%" + machineId + "%");
            cmd.Parameters.AddWithValue("@orderNum", "%" + orderNum + "%");

            return await Utility.SelectHelper(conn, cmd);
        }
    }
}