using AscCutlistEditor.Utility.MQTT;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditorTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AscCutlistEditorTests.Utility.MQTT
{
    [TestClass]
    public class QueriesTests
    {
        private static Mocks.MockSettings _settings;

        // Set the connection string for testing before running our tests.
        [AssemblyInitialize]
        public static void QueryTestInit(TestContext context)
        {
            // Writing these in so if the default fields change later, this
            // will still work with our test database.
            _settings = new Mocks.MockSettings();
            _settings.InitializeWithDefaults();
        }

        /// <summary>
        /// Make sure we have the right connection string before running queries.
        /// </summary>
        public void SetConnectionString()
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
            SetConnectionString();
            DataTable results = await Coils.GetCoilData(_settings, coilId);

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
            SetConnectionString();
            DataTable results = await Coils.GetNonDepletedCoils(_settings);

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
            SetConnectionString();
            DataTable results = await Orders.GetOrdersById(_settings, orderNum);

            // Assert
            Assert.AreEqual(results.Rows.Count, count);
        }

        [TestMethod]
        [DataRow("", 24)]
        [DataRow("1", 16)]
        [DataRow("5", 10)]
        [DataRow("Bad num", 0)]
        public async Task GetOrdersByMachineNumTest(string machineNum, int count)
        {
            // Act
            SetConnectionString();
            DataTable results = await Orders.GetOrdersByMachineNum(_settings, machineNum);

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
            SetConnectionString();
            DataTable results = await Orders.GetOrdersByIdAndMachine(_settings, orderNum, machineNum);

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
            SetConnectionString();
            DataTable results = await Bundles.GetBundle(_settings, orderNum);

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
            SetConnectionString();
            string tableString = AscCutlistEditor.Utility.MQTT.Utility.DataTableToString(table);

            // Assert
            Assert.AreEqual(
                tableString,
                "spam,eggs,|1,2,3|,,|True,False,19.51234");
        }
    }
}