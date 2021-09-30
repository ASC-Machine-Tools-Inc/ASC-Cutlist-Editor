﻿using System.Data.SqlClient;
using System.Windows;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Properties;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// This handles Bryan's code - connecting to the database, querying,
    /// and updating the ams data.
    /// </summary>
    internal class SettingsViewModel : ObservableObject
    {
        public object this[string settingsName]
        {
            get => Settings.Default[settingsName];
            set
            {
                Settings.Default[settingsName] = value;
                RaisePropertyChangedEvent(settingsName);
            }
        }

        public void Save()
        {
            Settings.Default.Save();
        }

        public SqlConnectionStringBuilder Builder;

        public async void CreateConnectionString(
            string dataSource,
            string databaseName,
            string username,
            string password)
        {
            if (dataSource == null || databaseName == null ||
                username == null || password == null)
            {
                MessageBox.Show(
                    "Please fill out all the fields.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            Builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = databaseName,
                UserID = username,
                Password = password,
                ConnectTimeout = 3  // Might remove and replace with loading later?
            };

            // SettingsViewModel["DataSource"] = dataSource;

            Settings.Default.DataSource = dataSource;
            Settings.Default.DatabaseName = databaseName;
            Settings.Default.Username = username;
            Settings.Default.Password = password;
            Settings.Default.Save();

            await using (SqlConnection conn = new SqlConnection(Builder.ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    // Show if connection successful.
                    MessageBox.Show(
                        "Connection successful!",
                        "ASC Cutlist Editor",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (SqlException)
                {
                    MessageBox.Show(
                        "Error connecting to the server.",
                        "ASC Cutlist Editor",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            // TODO: see if we need to set persist security info true: might need it for SQL auth

            string testConn = "Data Source=67.192.150.132,1452;Initial Catalog=taylormetals;Persist Security Info=True;User ID=TayMet_adj;Password=***********";
        }

        // TODO: message recieved handler

        /*
         *
             /// <summary>
    /// Get the order numbers, material type, and total run length of the orders for a specific machine.
    /// </summary>
    /// <param name="machineId">The machine's corresponding id to retrieve orders for.</param>
    /// <returns>A list of the orders for a given machine.</returns>
    public List<dynamic> GetOrders(string machineId)
    {
        using

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
    public IQueryable<dynamic> GetCoil(string coilId)
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
    public IQueryable<dynamic> GetCoils()
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
    public IQueryable<dynamic> GetCoils(string machineId, string orderNum)
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
    public IQueryable<dynamic> GetBundle(string orderNum)
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
    private async void SetUsageData(IQueryable<dynamic> usageData)
    {
        foreach (var product in usageData)
        {
            // Something along these lines - need model for product, set up context
            //_context.TimeLog.Add(TimeLog);
            //await _context.SaveChangesAsync();
        }
    }
         */
    }
}