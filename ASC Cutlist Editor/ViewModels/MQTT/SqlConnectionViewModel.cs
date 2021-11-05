using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models.MQTT;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// SqlConnectionViewModel handles saving the user's connection string and
    /// checking that it is a valid connection string.
    /// </summary>
    public class SqlConnectionViewModel : ObservableObject
    {
        /// <summary>
        /// Connection string builder to build from user's input.
        /// </summary>
        public static SqlConnectionStringBuilder Builder { get; set; }

        /// <summary>
        /// Application settings for user SQL connection.
        /// </summary>
        public ISettings UserSqlSettings { get; set; }

        /// <summary>
        /// Message to display for which settings are required to start
        /// listening for connections.
        /// </summary>
        public string SettingsRequiredMessage
        {
            get => _settingsRequiredMessage;
            set
            {
                _settingsRequiredMessage = value;
                RaisePropertyChangedEvent("SettingsRequiredMessage");
            }
        }

        public bool CanConnect
        {
            get => _canConnect;
            set
            {
                _canConnect = value;
                RaisePropertyChangedEvent("CanConnect");
            }
        }

        private string _settingsRequiredMessage;

        private bool _canConnect;

        // Used for displaying message boxes.
        private readonly IDialogService _dialog;

        public SqlConnectionViewModel(
            ISettings settings = null,
            IDialogService dialog = null)
        {
            UserSqlSettings = settings ?? new UserSqlSettings();
            _dialog = dialog ?? new Dialog();

            UpdateSettingsRequiredMessage();
        }

        public async Task<bool> TestConnection(
            bool updateConnectionString = true,
            bool toggleCursor = true)
        {
            // Check that we can create a connection string if requested.
            if (updateConnectionString && !UpdateConnectionString())
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
        public bool UpdateConnectionString(
            string connString = null)
        {
            if (!string.IsNullOrEmpty(connString))
            {
                Builder = new SqlConnectionStringBuilder(connString);
                return true;
            }

            if (UserSqlSettings.UseConnectionString)
            {
                string savedConnectionString = UserSqlSettings.ConnectionString;

                if (string.IsNullOrEmpty(savedConnectionString))
                {
                    _dialog.ShowMessageBox(
                        "Please enter a connection string.",
                        "ASC Cutlist Editor",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                try
                {
                    Builder = new SqlConnectionStringBuilder(savedConnectionString);
                }
                catch (Exception)
                {
                    _dialog.ShowMessageBox(
                        "Invalid connection string.",
                        "ASC Cutlist Editor",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

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

        /// <summary>
        /// Update the message that tells the user which settings they need to
        /// enter before listening for connections.
        /// </summary>
        public void UpdateSettingsRequiredMessage()
        {
            ISettings settings = UserSqlSettings;
            string message = "";

            // Set error message for invalid connection.
            if (settings.UseConnectionString)
            {
                if (string.IsNullOrEmpty(settings.ConnectionString))
                {
                    message += "Please enter a valid connection string or " +
                               "switch to individual fields.\n" +
                               "- Connection string is required.\n";
                }
            }
            else
            {
                string[] fields = { "DataSource", "DatabaseName", "Username", "Password" };
                foreach (string field in fields)
                {
                    string reqField = (string)settings.GetType()
                        .GetProperty(field)
                        ?.GetValue(settings, null);
                    if (string.IsNullOrEmpty(reqField))
                    {
                        message += $"- {field} is required.\n";
                    }
                }

                if (message.Length > 0)
                {
                    message = "Please fill out all the connection fields or " +
                              "switch to a connection string.\n" + message;
                }
            }

            Dictionary<string, string> tableFields = new Dictionary<string, string>
            {
                { "CoilTableName", "Coil Table" },
                { "CoilNumberName", "Coil Number" },
                { "CoilMaterialName", "Coil Material" },
                { "CoilDescriptionName", "Coil Description" },
                { "CoilLengthUsedName", "Coil Length Used" },
                { "CoilStartLengthName", "Coil Starting Length" },
                { "CoilDateName", "Coil Date" },
                { "OrderTableName", "Order Table" },
                { "OrderNumName", "Order Number" },
                { "OrderMaterialName", "Order Material" },
                { "OrderQuantityName", "Order Quantity" },
                { "OrderPartNumName", "Order Part Number" },
                { "OrderMachineNumName", "Order Machine Number" },
                { "OrderItemIdName", "Order Item Id" },
                { "OrderLengthName", "Order Length" },
                { "OrderBundleName", "Order Bundle" },
                { "BundleTableName", "Bundle Table" },
                { "BundleOrderNumName", "Bundle Order Number" },
                { "BundleColumns", "Bundle Columns" },
                { "UsageTableName", "Usage Table" },
                { "UsageOrderNumName", "Usage Order Number"},
                { "UsageMaterialName", "Usage Material" },
                { "UsageItemIdName", "Usage Item Id" },
                { "UsageLengthName", "Usage Length" },
                { "UsageDateName", "Usage Date"}
            };

            message += UpdateSettingsHelper(tableFields);

            // Check to see if we can connect.
            CanConnect = message.Length == 0;

            // Set error message for missing table/column names.
            SettingsRequiredMessage = message;
        }

        // Add missing required table and field names.
        private string UpdateSettingsHelper(Dictionary<string, string> fields)
        {
            ISettings settings = UserSqlSettings;
            string message = "";

            foreach (KeyValuePair<string, string> pair in fields)
            {
                string reqField = (string)settings.GetType()
                    .GetProperty(pair.Key)
                    ?.GetValue(settings, null);
                if (string.IsNullOrEmpty(reqField))
                {
                    message += $"- {pair.Value} is required.\n";
                }
            }

            if (message.Length > 0)
            {
                message = "Required table/column fields: \n" + message;
            }

            return message;
        }
    }
}