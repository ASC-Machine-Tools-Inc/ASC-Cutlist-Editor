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
using System.Windows.Threading;
using AscCutlistEditor.Models.MQTT;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// SqlConnectionViewModel handles saving the user's connection string and
    /// checking that it is a valid connection string.
    /// </summary>
    public class SqlConnectionViewModel : ObservableObject
    {
        public static SqlConnectionStringBuilder Builder;

        public ISettings UserSqlSettings;

        private readonly IDialogService _dialog;

        public SqlConnectionViewModel(
            ISettings settings = null,
            IDialogService dialog = null)
        {
            UserSqlSettings = settings ?? new UserSqlSettings();
            _dialog = dialog ?? new Dialog();
        }

        /// <summary>
        /// Attempt opening a connection to the current connection string in Builder.
        /// </summary>
        /// <param name="connString">
        /// Optional parameter to test as connection string.
        /// </param>
        /// <param name="toggleCursor">
        /// Optional parameter to change cursor appearance while waiting to connect.
        /// </param>
        /// <returns>
        /// Returns true if the connection was successful, false otherwise.
        /// </returns>
        public async Task<bool> TestConnection(
            string connString = null,
            bool toggleCursor = true)
        {
            // Check that we can create a connection string.
            if (!CreateConnectionString(connString))
            {
                return false;
            }

            await using SqlConnection conn = new SqlConnection(Builder.ConnectionString);
            try
            {
                // Set the loading cursor while connecting.
                if (toggleCursor)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                }

                await conn.OpenAsync();

                // Show if connection successful.
                _dialog.ShowMessageBox(
                    "Connection successful!",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (SqlException)
            {
                _dialog.ShowMessageBox(
                    "Error connecting to the server.",
                    "ASC Cutlist Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            finally
            {
                // Reset to default.
                if (toggleCursor)
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// Save the user settings and encrypt them.
        /// </summary>
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
        /// Creates a new connection string from the SQL Server connection
        /// parameters.
        /// </summary>
        /// <param name="connString">
        /// Optional parameter: if provided, this will be used to connect. If
        /// not, the saved user settings will be used instead.
        /// </param>
        /// <returns>
        /// True if a connection string was created, false otherwise.
        /// </returns>
        public bool CreateConnectionString(string connString = null)
        {
            if (!string.IsNullOrEmpty(connString))
            {
                Builder = new SqlConnectionStringBuilder(connString);
                return true;
            }

            string dataSource = UserSqlSettings.DataSource;
            string databaseName = UserSqlSettings.DatabaseName;
            string username = UserSqlSettings.Username;
            string password = UserSqlSettings.Password;

            if (dataSource == null || databaseName == null ||
                username == null || password == null)
            {
                _dialog.ShowMessageBox(
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