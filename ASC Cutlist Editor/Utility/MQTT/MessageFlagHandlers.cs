using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.Utility.Constants;
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    // Collection of handlers to deal with the set flags in machine messages.
    internal class MessageFlagHandlers
    {
        /// <summary>
        /// Handle updating the display and returning order data depending on
        /// the set flags from the received machine message.
        /// </summary>
        internal static async Task OrderDatReqFlagHandler(
            MachineMessage message,
            MachineMessage returnMessage)
        {
            MqttPub pub = message.tags.set1.MqttPub;
            MqttSub sub = returnMessage.tags.set2.MqttSub;

            string orderDataRequested = pub.OrderDatReq;
            switch (orderDataRequested)
            {
                case "TRUE" when !string.IsNullOrEmpty(pub.OrderNo):
                    {
                        // Respond with the order and bundle data.
                        DataTable fullOrderTable = await Queries.GetOrdersByIdAndMachine(
                            pub.OrderNo,
                            pub.JobNumber);
                        sub.OrderDatRecv = Queries.DataTableToString(fullOrderTable);

                        DataTable bundlesTable = await Queries.GetBundle(pub.OrderNo);
                        sub.BundleDatRecv = Queries.DataTableToString(bundlesTable);
                        sub.OrderDatAck = "TRUE";
                        sub.OrderNo = pub.OrderNo;
                        break;
                    }
                case "TRUE":
                    // If data was requested but no order number was included,
                    // set the flag to clear the button.
                    sub.OrderDatAck = "TRUE";
                    break;

                case "FALSE":
                    {
                        // Otherwise, respond with the current job numbers.
                        DataTable orderNumTable = await Queries.GetOrdersByMachineNum(pub.JobNumber);

                        sub.MqttString = Queries.DataTableToString(orderNumTable);
                        sub.MqttDest = pub.JobNumber;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handle updating the display and returning coil data depending on
        /// the set flags from the received machine message.
        /// </summary>
        internal static async Task CoilDatReqFlagHandler(
            MachineMessage message,
            MachineMessage returnMessage)
        {
            // 5. Check coil dat req true and scan coil id not empty and order no not empty and order no not no order selected
            MqttPub pub = message.tags.set1.MqttPub;
            MqttSub sub = returnMessage.tags.set2.MqttSub;

            if (pub.CoilDatReq == MessageFlagResponses.Requested)
            {
                // Coil data requested, check for parameters.
                List<string> coilDataMessages = new List<string>();
                if (!string.IsNullOrEmpty(pub.ScanCoilID) &&
                    !string.IsNullOrEmpty(pub.OrderNo) &&
                    !pub.OrderNo.Equals("NoOrderSelected"))
                {
                    // Valid parameters, grab the coil data.
                    DataTable coilData = await Queries.GetCoilData(pub.ScanCoilID);
                    DataTable orderData = await Queries.GetOrdersById(pub.OrderNo);

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
                    }
                }
                else
                {
                    // Coil parameters or table invalid.
                    sub.CoilDatAck = "TRUE";
                    sub.CoilDatValidAck = "FALSE";

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

                // Attach our message for response to the operator.
                sub.CoilDatRecv = string.Join("|", coilDataMessages);
            }
            else if (pub.CoilDatReq == MessageFlagResponses.NotRequested)
            {
                // No data requested, clear flags before the next message to
                // prevent repeated popups.
                sub.CoilDatAck = "FALSE";
                sub.CoilDatValidAck = "FALSE";
            }
        }
    }
}