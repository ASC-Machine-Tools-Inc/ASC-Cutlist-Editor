using AscCutlistEditor.Models.MQTT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    // Collection of handlers to deal with the set flags in machine messages.
    internal class MessageFlagHandlers
    {
        private readonly Queries _queries;

        public MessageFlagHandlers(SqlConnectionViewModel sqlConn)
        {
            _queries = new Queries(sqlConn.UserSqlSettings);
        }

        /// <summary>
        /// Handle updating the display and returning order data depending on
        /// the set flags from the received machine message.
        /// </summary>
        internal async Task OrderDatReqFlagHandler(
            MachineMessage message,
            MachineMessage returnMessage)
        {
            MqttPub pub = message.tags.set1.MqttPub;
            MqttSub sub = returnMessage.tags.set2.MqttSub;

            string orderDataRequested = pub.OrderDatReq;
            if (orderDataRequested == "TRUE" && pub.OrderNo != null)
            {
                // Respond with the order and bundle data.
                DataTable fullOrderTable = await _queries.GetOrdersByIdAndMachine(
                    pub.OrderNo,
                    pub.JobNumber);
                sub.OrderDatRecv = _queries.DataTableToString(fullOrderTable);

                DataTable bundlesTable = await _queries.GetBundle(pub.OrderNo);
                sub.BundleDatRecv = _queries.DataTableToString(bundlesTable);
                sub.OrderDatAck = "TRUE";
                sub.OrderNo = pub.OrderNo;
            }
            else if (orderDataRequested == "TRUE")
            {
                // If data was requested but no order number was included,
                // set the flag to clear the button.
                sub.OrderDatAck = "TRUE";
            }
            else if (orderDataRequested == "FALSE")
            {
                // Otherwise, respond with the current job numbers.
                DataTable orderNumTable = await _queries
                    .GetOrdersByMachineNum(pub.JobNumber);

                sub.MqttString = _queries.DataTableToString(orderNumTable);
                sub.MqttDest = pub.JobNumber;
            }
        }

        /// <summary>
        /// Handle updating the display and returning coil data depending on
        /// the set flags from the received machine message.
        /// </summary>
        internal async Task CoilDatReqFlagHandler(
            MachineMessage message,
            MachineMessage returnMessage)
        {
            MqttPub pub = message.tags.set1.MqttPub;
            MqttSub sub = returnMessage.tags.set2.MqttSub;
            bool coilValid = false;

            if (pub.CoilDatReq == "TRUE")
            {
                // Coil data requested, check for parameters.
                var coilDataMessages = new List<string>();
                if (pub.ScanCoilID != "" &&
                    pub.OrderNo != null &&
                    pub.OrderNo != "NoOrderSelected")
                {
                    // Valid parameters, grab the coil data.
                    DataTable coilData = await _queries.GetCoilData(pub.ScanCoilID);
                    DataTable orderData = await _queries.GetOrdersById(pub.OrderNo);

                    if (coilData.Rows.Count == 0)
                    {
                        coilDataMessages.Add("Coil not in the database!");
                    }
                    else if (orderData.Rows.Count == 0)
                    {
                        coilDataMessages.Add("No orders found for " +
                                             "this order number!");
                    }
                    else
                    {
                        DataRow firstCoil = coilData.Rows[0];
                        DataRow firstOrder = orderData.Rows[0];

                        try
                        {
                            double startingLength = double.Parse(firstCoil[0].ToString());
                            double usedLength = double.Parse(firstCoil[1].ToString());
                            double orderLength = double.Parse(firstOrder[2].ToString());
                            double lengthRemaining =
                                (startingLength - usedLength) * 12 - orderLength;

                            string coilMaterial = firstCoil[2].ToString()?.Trim();
                            string orderMaterial = firstOrder[1].ToString()?.Trim();

                            // Check that the coil has enough length remaining to
                            // run and is the right material.
                            if (coilMaterial == orderMaterial && lengthRemaining > 0.0)
                            {
                                coilValid = true;
                                coilDataMessages.Add("Coil is OKAY to run!");
                            }
                            else
                            {
                                // Can't run this coil, add error messages.
                                coilDataMessages.Add("Coil is NOT OKAY to run!");

                                if (coilMaterial != orderMaterial)
                                {
                                    coilDataMessages.Add("Coil and order " +
                                                         "material are different.");
                                }

                                if (lengthRemaining <= 0.0)
                                {
                                    coilDataMessages.Add("Not enough " +
                                                         "material remaining.");
                                }
                            }
                        }
                        catch (Exception)
                        {
                            coilDataMessages.Add("Error parsing database " +
                                                 "coil/order information.");
                        }
                    }
                }
                else
                {
                    // Coil parameters invalid, add error message.
                    if (string.IsNullOrEmpty(pub.ScanCoilID))
                    {
                        coilDataMessages.Add("Coil ID blank - Tag or " +
                                             "inventory data invalid!");
                    }

                    if (string.IsNullOrEmpty(pub.OrderNo))
                    {
                        coilDataMessages.Add("No order number " +
                                             "selected! Select order " +
                                             "number to verify coil " +
                                             "data.");
                    }
                }

                if (coilValid)
                {
                    // Coil good to run.
                    sub.CoilDatAck = "TRUE";
                    sub.CoilDatValidAck = "TRUE";
                }
                else
                {
                    // Coil parameters or table invalid.
                    sub.CoilDatAck = "TRUE";
                    sub.CoilDatValidAck = "FALSE";
                }

                // Attach our message for response to the operator.
                sub.CoilDatRecv = string.Join("|", coilDataMessages);
            }
            else if (pub.CoilDatReq == "FALSE")
            {
                // No data requested, clear flags before the next message to
                // prevent repeated popups.
                sub.CoilDatAck = "FALSE";
                sub.CoilDatValidAck = "FALSE";
            }
        }

        /// <summary>
        /// Handle updating the display when the HMI requests the list of coils.
        /// </summary>
        internal async Task CoilStoreReqFlagHandler(
            MachineMessage message,
            MachineMessage returnMessage)
        {
            MqttPub pub = message.tags.set1.MqttPub;
            MqttSub sub = returnMessage.tags.set2.MqttSub;

            // Check if the coils are requested by the HMI.
            if (pub.CoilStoreReq != "TRUE" || pub.CoilUsageDat == null)
            {
                return;
            }

            DataTable coils = await _queries.GetNonDepletedCoils();
            int count = coils.Rows.Count - 1;

            sub.CoilStoreRecv = count + "|" + _queries.DataTableToString(coils);
            sub.CoilStoreAck = "TRUE";
        }

        /// <summary>
        /// Handle adding the coil usage data to the database when received
        /// from the machine.
        /// </summary>
        /// <returns>The number of rows added to the database.</returns>
        internal async Task<int> CoilUsageSendFlagHandler(
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
                .Split("|")
                // Split those rows into a list of fields.
                .Select(usageRows =>
                    new List<string>(usageRows.Split(",")))
                .ToList();

            List<CoilUsage> coilUsageList = coilUsageFields
                // Group the fields into the ones with unique coil and item ids,
                // with the total length for those groupings.
                .GroupBy(
                    usageRow =>
                        new { coilId = usageRow[1], itemId = usageRow[3] },
                    (key, fields) =>
                    {
                    // Convert fields to list to prevent multiple enumeration.
                    var enumerable = fields.ToList();
                        return new CoilUsage
                        {
                            orderno = enumerable.ElementAt(0).ToString(),
                            CoilId = key.coilId,
                            CoilMatl = enumerable.ElementAt(2).ToString(),
                            ItemID = key.itemId,
                            Length = enumerable.ElementAt(4).Sum(Convert.ToDecimal),
                            Time = DateTime.Now
                        };
                    })
                .ToList();

            // Add our rows to the DataTable for updating the database.
            DataTable usageData = new DataTable();
            foreach (CoilUsage usageRow in coilUsageList)
            {
                usageData.Rows.Add(usageRow);
            }

            sub.CoilUsageRecvAck = "TRUE";

            // Update the database.
            return await _queries.SetUsageData(usageData);
        }
    }
}