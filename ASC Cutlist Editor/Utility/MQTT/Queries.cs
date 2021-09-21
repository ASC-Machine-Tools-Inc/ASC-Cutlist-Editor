using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using AscCutlistEditor.Models;
using AscCutlistEditor.Models.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class Queries
    {
        private SqlConnection conn = new SqlConnection("TODO: connection string, store somewhere safe and let user set");

        private IQueryable<OrderData> amsorder;

        /// <summary>
        /// Select the order numbers, material type, and total run length of the orders for a specific machine.
        /// </summary>
        /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
        /// <returns>A list of the orders for a given machine.</returns>
        public Task<IQueryable<dynamic>> getOrders(string machineId)
        {
            return amsorder
                .Select(x => new
                {
                    OrderNo = x.orderno,
                    Material = x.material,
                    OrderLen = x.Sum(y => y.length * Math.Round(y.quantity, 2)),
                    PartNo = x.partno
                })
                .Where(x => x.orderno != null && x.machineNum.equals(machineId))
                .GroupBy(x => new { x.orderno, x.material, x.machinenum, x.partno });
        }
    }
}