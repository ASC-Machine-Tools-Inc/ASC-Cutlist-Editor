using System;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AscCutlistEditor.Utility.MQTT;
using AscCutlistEditor.ViewModels.MQTT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.Utility.MQTT
{
    [TestClass]
    public class QueriesTests
    {
        public QueriesTests()
        {
        }

        [TestMethod]
        [DataRow("1", 10)]
        [DataRow("H", 10)]
        [DataRow("T1", 10)]
        [DataRow("1256", 10)]
        [DataRow("1257", 0)]
        public async Task GetCoilDataTest(string coilId, int count)
        {
            // Arrange
            SqlConnectionViewModel.Builder.ConnectionString =
                "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\PTON\\Documents\\cutlistEditorTestDb.mdf;Integrated Security=True;Connect Timeout=30";

            // Act
            DataTable results = await Queries.GetCoilData(coilId);

            // Assert
            Assert.AreEqual(results.Rows.Count, count);
        }

        [TestMethod]
        public async void GetNonDepletedCoilsTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async void GetOrdersByIdTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async void GetOrdersByMachineNumTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async void GetOrdersByIdAndMachineTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async void GetBundleTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async void SetUsageDataTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async void DataTableToStringTest()
        {
            throw new NotImplementedException();
        }
    }
}