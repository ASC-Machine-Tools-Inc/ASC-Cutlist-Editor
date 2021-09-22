using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Text;
using AscCutlistEditor.Models;
using AscCutlistEditor.Models.MQTT;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class Queries
    {
        private SqlConnection conn = new SqlConnection("TODO: connection string, store somewhere safe and let user set");

        /// <summary>
        /// Get the order numbers, material type, and total run length of the orders for a specific machine.
        /// </summary>
        /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
        /// <returns>A list of the orders for a given machine.</returns>
        public IQueryable<dynamic> getOrders(string machineId)
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

        /// <summary>
        /// Get the coil data for a specific coil.
        /// </summary>
        /// <param name="coilId">The coil's corresponding id to get data for.</param>
        /// <returns>The data for that coil</returns>
        public IQueryable<dynamic> getCoil(string coilId)
        {
            return amscoil
                .Select(x => new
                {
                    x.startlength,
                    x.lengthused,
                    x.material
                })
                .Where(x => x.coilnumber.equals(coilId));
        }

        /// <summary>
        /// Get the list of coils that aren't currently depleted for displaying on an HMI.
        /// </summary>
        /// <returns>A list of non-depleted coils.</returns>
        public IQueryable<dynamic> getCoils()
        {
            return amscoil
                .Select(x => new
                {
                    x.coilnumber,
                    x.description,
                    x.material,
                    x.startlength,
                    x.lengthused
                })
                .Where(x => x.dateout == null);
        }

        /// <summary>
        /// Get the list of orders for the given order number on the given machine.
        /// </summary>
        /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
        /// <param name="orderNum">The orders' corresponding id to retrieve orders for.</param>
        /// <returns>A list of orders for the given machine and order number.</returns>
        public IQueryable<dynamic> getCoils(string machineId, string orderNum)
        {
            return amsorder
                .Select(x => new
                {
                    x.item_id,
                    x.length,
                    x.quantity,
                    x.bundle
                })
                .Where(x => x.machinenum.equals(machineId) && x.orderno.equals(orderNum));
        }

        /// <summary>
        /// Get the bundle data for a given order number.
        /// </summary>
        /// <param name="orderNum">The order number for the corresponding bundle.</param>
        /// <returns>A list of distinct bundle data for the order number.</returns>
        public IQueryable<dynamic> getBundle(string orderNum)
        {
            return amsbundle
                .Select(x => new
                {
                    x.material,
                    x.prodcode,
                    x.user1,
                    x.user2,
                    x.user3,
                    x.user4,
                    x.custname,
                    x.custaddr1,
                    x.custaddr2,
                    x.custcity,
                    x.custstate,
                    x.custzip
                })
                .Distinct()
                .Where(x => x.orderno.equals(orderNum));
        }

        /// <summary>
        /// Update amsproduct by inserting the contents of usageData.
        /// </summary>
        /// <param name="usageData">The data to add to amsproduct.</param>
        private async void setUsageData(IQueryable<dynamic> usageData)
        {
            foreach (var Product in usageData)
            {
                // Something along these lines - need model for product, set up context
                //_context.TimeLog.Add(TimeLog);
                //await _context.SaveChangesAsync();
            }
        }
    }
}