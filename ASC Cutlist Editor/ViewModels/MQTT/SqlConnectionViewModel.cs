using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Properties;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// SqlConnectionViewModel handles saving the user's connection string and
    /// checking that it is a valid connection string..
    /// </summary>
    internal class SqlConnectionViewModel : ObservableObject
    {
        public static SqlConnectionStringBuilder Builder;

        public object this[string settingsName]
        {
            get => Settings.Default[settingsName];
            set
            {
                Settings.Default[settingsName] = value;
                RaisePropertyChangedEvent(settingsName);
            }
        }

        // Save the user settings and encrypt them.
        public static void Save()
        {
            string sectionName = "userSettings/AscCutlistEditor.Properties.Settings";
            string protectionProvider = "DataProtectionConfigurationProvider";

            Configuration config = ConfigurationManager.OpenExeConfiguration(
                ConfigurationUserLevel.PerUserRoamingAndLocal);
            ConfigurationSection userSettings = config.GetSection(sectionName);
            userSettings.SectionInformation.ProtectSection(protectionProvider);

            config.Save();
            Settings.Default.Save();
        }

        /// <summary>
        /// Attempt opening a connection to the current connection string in Builder.
        /// </summary>
        /// <returns>Returns true if the connection was successful, false otherwise.</returns>
        public async Task<bool> TestConnection()
        {
            // Check that we can create a connection string.
            if (!CreateConnectionString())
            {
                return false;
            }

            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);
            try
            {
                // Set the loading cursor while connecting.
                Mouse.OverrideCursor = Cursors.Wait;

                await conn.OpenAsync();

                // Show if connection successful.
                MessageBox.Show(
                    "Connection successful!",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Error connecting to the server.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            finally
            {
                // Reset to default.
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Creates a new connection string from the SQL Server connection parameters.
        /// </summary>
        /// <returns>
        /// True if a connection string was created, false otherwise.
        /// </returns>
        public bool CreateConnectionString()
        {
            string dataSource = (string)this["DataSource"];
            string databaseName = (string)this["DatabaseName"];
            string username = (string)this["Username"];
            string password = (string)this["Password"];

            if (dataSource == null || databaseName == null ||
                username == null || password == null)
            {
                MessageBox.Show(
                    "Please fill out all the fields.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            Builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = databaseName,
                UserID = username,
                Password = password
            };

            return true;
        }
    }
}