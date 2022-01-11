using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class Coils
    {
        /// <summary>
        /// Handle updating the display and returning coil data depending on
        /// the set flags from the received machine message.
        /// </summary>
        internal static async Task CoilDatReqFlagHandler(
            ISettings settings,
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
                    DataTable coilData = await GetCoilData(settings, pub.ScanCoilID);
                    DataTable orderData = await Orders.GetOrdersById(settings, pub.OrderNo);

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

                    if (string.IsNullOrEmpty(pub.OrderNo) ||
                        pub.OrderNo == "NoOrderSelected")
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
        internal static async Task CoilStoreReqFlagHandler(
            ISettings settings,
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

            DataTable coils = await GetNonDepletedCoils(settings);
            int count = coils.Rows.Count;

            sub.CoilStoreRecv = count + "|" + Utility.DataTableToString(coils);
            sub.CoilStoreAck = "TRUE";
        }

        /// <summary>
        /// Get the starting length, used length, and material for a specific coil.
        /// </summary>
        /// <param name="settings">Settings containing table/column names to use.</param>
        /// <param name="coilId">The coil's corresponding id to get data for.</param>
        /// <returns>The data for that coil.</returns>
        public static async Task<DataTable> GetCoilData(ISettings settings, string coilId)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string startlength = builder.QuoteIdentifier(settings.CoilStartLengthName);
            string lengthused = builder.QuoteIdentifier(settings.CoilLengthUsedName);
            string material = builder.QuoteIdentifier(settings.CoilMaterialName);

            // Table name.
            string coilTableName = builder.QuoteIdentifier(settings.CoilTableName);

            // Filter columns.
            string coilnumber = builder.QuoteIdentifier(settings.CoilNumberName);

            string queryStr =
                $"SELECT {startlength}, {lengthused}, {material} " +
                $"FROM {coilTableName} " +
                $"WHERE {coilnumber} LIKE @coilID";

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            // Filter parameters.
            cmd.Parameters.AddWithValue("@coilID", "%" + coilId + "%");

            return await Utility.SelectHelper(conn, cmd);
        }

        /// <summary>
        /// Get the coil number, description, material, starting length, and
        /// length used for all non-depleted coils.
        /// </summary>
        /// <param name="settings">Settings containing table/column names to use.</param>
        /// <returns>The list of non-depleted coils.</returns>
        public static async Task<DataTable> GetNonDepletedCoils(ISettings settings)
        {
            await using SqlConnection conn =
                new SqlConnection(SqlConnectionViewModel.Builder.ConnectionString);

            SqlCommandBuilder builder = new SqlCommandBuilder();

            // Selected columns.
            string coilnumber = builder.QuoteIdentifier(settings.CoilNumberName);
            string description = builder.QuoteIdentifier(settings.CoilDescriptionName);
            string material = builder.QuoteIdentifier(settings.CoilMaterialName);
            string startlength = builder.QuoteIdentifier(settings.CoilStartLengthName);
            string lengthused = builder.QuoteIdentifier(settings.CoilLengthUsedName);

            // Table name.
            string coilTableName = builder.QuoteIdentifier(settings.CoilTableName);

            // Filter columns.
            string dateout = builder.QuoteIdentifier(settings.CoilDateName);

            string queryStr = $"SELECT {coilnumber}, {description}, {material}, " +
                              $"(CONVERT(DECIMAL(10,2),{startlength}) - " +
                              $"CONVERT(DECIMAL(10,2),{lengthused})) AS currlength " +
                              $"FROM {coilTableName} " +
                              $"WHERE {dateout} IS NULL";
            Debug.WriteLine(queryStr);

            await using SqlCommand cmd = new SqlCommand(queryStr, conn);

            return await Utility.SelectHelper(conn, cmd);
        }
    }
}