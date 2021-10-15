using System;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AscCutlistEditor.Utility.MQTT;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditorTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.Utility.MQTT
{
    [TestClass]
    public class QueriesTests
    {
        // Set the connection string for testing before running our tests.
        [AssemblyInitialize]
        public static void QueryTestInit(TestContext context)
        {
            SqlConnectionViewModel.Builder = new SqlConnectionStringBuilder
            {
                ConnectionString = Strings.ConnectionString
            };
        }

        [TestMethod]
        [DataRow("", 202)]
        [DataRow("1", 114)]
        [DataRow("H", 108)]
        [DataRow("T1", 38)]
        [DataRow("1256", 1)]
        [DataRow("1257", 0)]
        public async Task GetCoilDataTest(string coilId, int count)
        {
            // Act
            DataTable results = await Queries.GetCoilData(coilId);

            /*
            Debug.WriteLine($"Getting coil data for id {coilId}, expected count {count}");
            foreach (DataRow row in results.Rows)
            {
                // DEBUG: gets rows, trimming fields first
                Debug.WriteLine(string.Join(", ", row.ItemArray
                    .Select(item => item.ToString()?.Trim())));
            }
            */

            // Assert
            Assert.AreEqual(results.Rows.Count, count);
        }

        [TestMethod]
        public async Task GetNonDepletedCoilsTest()
        {
            // Act
            DataTable results = await Queries.GetNonDepletedCoils();

            // Assert
            Assert.AreEqual(results.Rows.Count, 202);
        }

        [TestMethod]
        [DataRow("", 18)]
        [DataRow("10", 18)]
        [DataRow("1017", 3)]
        [DataRow("10170", 1)]
        [DataRow("Bad num", 0)]
        public async Task GetOrdersByIdTest(string orderNum, int count)
        {
            // Act
            DataTable results = await Queries.GetOrdersById(orderNum);

            // Assert
            Assert.AreEqual(results.Rows.Count, count);
        }

        [TestMethod]
        [DataRow("", 23)]
        [DataRow("1", 15)]
        [DataRow("5", 10)]
        [DataRow("Bad num", 0)]
        public async Task GetOrdersByMachineNumTest(string machineNum, int count)
        {
            // Act
            DataTable results = await Queries.GetOrdersByMachineNum(machineNum);

            // Assert
            Assert.AreEqual(results.Rows.Count, count);
        }

        [TestMethod]
        [DataRow("", "", 70)]
        [DataRow("10", "", 70)]
        [DataRow("", "5", 28)]
        [DataRow("1016", "JN", 4)]
        [DataRow("10170", "534", 1)]
        [DataRow("Bad order", "1", 0)]
        [DataRow("1", "Bad machine", 0)]
        public async Task GetOrdersByIdAndMachineTest(string orderNum, string machineNum, int count)
        {
            // Act
            DataTable results = await Queries.GetOrdersByIdAndMachine(orderNum, machineNum);

            // Assert
            Assert.AreEqual(results.Rows.Count, count);
        }

        [TestMethod]
        [DataRow("", 18)]
        [DataRow("10144", 1)]
        [DataRow("Bad bundle", 0)]
        public async Task GetBundleTest(string orderNum, int count)
        {
            // Act
            DataTable results = await Queries.GetBundle(orderNum);

            // Assert
            Assert.AreEqual(results.Rows.Count, count);
        }

        [TestMethod]
        public void DataTableToStringTest()
        {
            // Arrange
            DataTable table = new DataTable();
            table.Columns.Add("col1");
            table.Columns.Add("col2");
            table.Columns.Add("col3");
            table.Rows.Add("spam", "eggs");
            table.Rows.Add(1, 2, 3);
            table.Rows.Add();
            table.Rows.Add(true, false, 19.51234);

            // Act
            string tableString = Queries.DataTableToString(table);

            // Assert
            Assert.AreEqual(
                tableString,
                "spam,eggs,|1,2,3|,,|True,False,19.51234");
        }
    }
}