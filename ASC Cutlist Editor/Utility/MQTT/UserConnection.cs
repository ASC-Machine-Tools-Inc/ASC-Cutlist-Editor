using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Windows;

namespace AscCutlistEditor.Utility.MQTT
{
    internal class UserConnection
    {
        public static async void CreateConnectionString(
            string dataSource,
            string databaseName,
            string username,
            string password)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = databaseName,
                UserID = username,
                Password = password
            };

            SqlConnection conn = new SqlConnection(builder.ConnectionString);

            try
            {
                await conn.OpenAsync();
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Error connecting to the server.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // TODO: we want to use LINQ instead of sqlcommand?
            SqlCommand command = new SqlCommand
            {
                CommandText = "test",
                Connection = conn
            };

            int rowsChanged = await command.ExecuteNonQueryAsync();

            // Show if connection successful.
            MessageBox.Show(
                "Connection successful!",
                "ASC Cutlist Editor",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // TODO: see if we need to set persist security info true: might need it for SQL auth

            string testConn = "Data Source=67.192.150.132,1452;Initial Catalog=taylormetals;Persist Security Info=True;User ID=TayMet_adj;Password=***********";
        }

        // TODO: message recieved handler
    }
}