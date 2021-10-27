using System.Configuration;
using System.Data.SqlClient;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Properties;
using Microsoft.VisualBasic;

namespace AscCutlistEditor.Models.MQTT
{
    // Wrapper class over settings for SQL connection string.
    public class UserSqlSettings : ObservableObject, ISettings
    {
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
        /// Reset and save the user settings.
        /// </summary>
        public static void Reset()
        {
            Settings.Default.Reset();
            Save();
        }

        public string ConnectionString
        {
            get => (string)Settings.Default["ConnectionString"];
            set
            {
                Settings.Default["ConnectionString"] = value;
                RaisePropertyChangedEvent("ConnectionString");
            }
        }

        public bool UseConnectionString
        {
            get => (bool)Settings.Default["UseConnectionString"];
            set
            {
                Settings.Default["UseConnectionString"] = value;
                RaisePropertyChangedEvent("UseConnectionString");
            }
        }

        public string DataSource
        {
            get => (string)Settings.Default["DataSource"];
            set
            {
                Settings.Default["DataSource"] = value;
                RaisePropertyChangedEvent("DataSource");
            }
        }

        public string DatabaseName
        {
            get => (string)Settings.Default["DatabaseName"];
            set
            {
                Settings.Default["DatabaseName"] = value;
                RaisePropertyChangedEvent("DatabaseName");
            }
        }

        public string Username
        {
            get => (string)Settings.Default["Username"];
            set
            {
                Settings.Default["Username"] = value;
                RaisePropertyChangedEvent("Username");
            }
        }

        public string Password
        {
            get => (string)Settings.Default["Password"];
            set
            {
                Settings.Default["Password"] = value;
                RaisePropertyChangedEvent("Password");
            }
        }

        public string CoilTableName
        {
            get => (string)Settings.Default["CoilTableName"];
            set
            {
                Settings.Default["CoilTableName"] = value;
                RaisePropertyChangedEvent("CoilTableName");
            }
        }

        public string CoilStartLengthName
        {
            get => (string)Settings.Default["CoilStartLengthName"];
            set
            {
                Settings.Default["CoilStartLengthName"] = value;
                RaisePropertyChangedEvent("CoilStartLengthName");
            }
        }

        public string CoilLengthUsedName
        {
            get => (string)Settings.Default["CoilLengthUsedName"];
            set
            {
                Settings.Default["CoilLengthUsedName"] = value;
                RaisePropertyChangedEvent("CoilLengthUsedName");
            }
        }

        public string CoilMaterialName
        {
            get => (string)Settings.Default["CoilMaterialName"];
            set
            {
                Settings.Default["CoilMaterialName"] = value;
                RaisePropertyChangedEvent("CoilMaterialName");
            }
        }

        public string CoilNumberName
        {
            get => (string)Settings.Default["CoilNumberName"];
            set
            {
                Settings.Default["CoilNumberName"] = value;
                RaisePropertyChangedEvent("CoilNumberName");
            }
        }

        public string CoilDescriptionName
        {
            get => (string)Settings.Default["CoilDescriptionName"];
            set
            {
                Settings.Default["CoilDescriptionName"] = value;
                RaisePropertyChangedEvent("CoilDescriptionName");
            }
        }

        public string CoilDateName
        {
            get => (string)Settings.Default["CoilDateName"];
            set
            {
                Settings.Default["CoilDateName"] = value;
                RaisePropertyChangedEvent("CoilDateName");
            }
        }

        public string OrderTableName
        {
            get => (string)Settings.Default["OrderTableName"];
            set
            {
                Settings.Default["OrderTableName"] = value;
                RaisePropertyChangedEvent("OrderTableName");
            }
        }

        public string OrderNumName
        {
            get => (string)Settings.Default["OrderNumName"];
            set
            {
                Settings.Default["OrderNumName"] = value;
                RaisePropertyChangedEvent("OrderNumName");
            }
        }

        public string OrderMaterialName
        {
            get => (string)Settings.Default["OrderMaterialName"];
            set
            {
                Settings.Default["OrderMaterialName"] = value;
                RaisePropertyChangedEvent("OrderMaterialName");
            }
        }

        public string OrderQuantityName
        {
            get => (string)Settings.Default["OrderQuantityName"];
            set
            {
                Settings.Default["OrderQuantityName"] = value;
                RaisePropertyChangedEvent("OrderQuantityName");
            }
        }

        public string OrderPartNumName
        {
            get => (string)Settings.Default["OrderPartNumName"];
            set
            {
                Settings.Default["OrderPartNumName"] = value;
                RaisePropertyChangedEvent("OrderPartNumName");
            }
        }

        public string OrderMachineNumName
        {
            get => (string)Settings.Default["OrderMachineNumName"];
            set
            {
                Settings.Default["OrderMachineNumName"] = value;
                RaisePropertyChangedEvent("OrderMachineNumName");
            }
        }

        public string OrderItemIdName
        {
            get => (string)Settings.Default["OrderItemIdName"];
            set
            {
                Settings.Default["OrderItemIdName"] = value;
                RaisePropertyChangedEvent("OrderItemIdName");
            }
        }

        public string OrderLengthName
        {
            get => (string)Settings.Default["OrderLengthName"];
            set
            {
                Settings.Default["OrderLengthName"] = value;
                RaisePropertyChangedEvent("OrderLengthName");
            }
        }

        public string OrderBundleName
        {
            get => (string)Settings.Default["OrderBundleName"];
            set
            {
                Settings.Default["OrderBundleName"] = value;
                RaisePropertyChangedEvent("OrderBundleName");
            }
        }

        public string BundleTableName
        {
            get => (string)Settings.Default["BundleTableName"];
            set
            {
                Settings.Default["BundleTableName"] = value;
                RaisePropertyChangedEvent("BundleTableName");
            }
        }

        public string BundleOrderNumName
        {
            get => (string)Settings.Default["BundleOrderNumName"];
            set
            {
                Settings.Default["BundleOrderNumName"] = value;
                RaisePropertyChangedEvent("BundleOrderNumName");
            }
        }

        public string BundleColumns
        {
            get => (string)Settings.Default["BundleColumns"];
            set
            {
                Settings.Default["BundleColumns"] = value;
                RaisePropertyChangedEvent("BundleColumns");
            }
        }

        public string UsageTableName
        {
            get => (string)Settings.Default["UsageTableName"];
            set
            {
                Settings.Default["UsageTableName"] = value;
                RaisePropertyChangedEvent("UsageTableName");
            }
        }

        public string UsageOrderNumName
        {
            get => (string)Settings.Default["UsageOrderNumName"];
            set
            {
                Settings.Default["UsageOrderNumName"] = value;
                RaisePropertyChangedEvent("UsageOrderNumName");
            }
        }

        public string UsageMaterialName
        {
            get => (string)Settings.Default["UsageMaterialName"];
            set
            {
                Settings.Default["UsageMaterialName"] = value;
                RaisePropertyChangedEvent("UsageMaterialName");
            }
        }

        public string UsageItemIdName
        {
            get => (string)Settings.Default["UsageItemIdName"];
            set
            {
                Settings.Default["UsageItemIdName"] = value;
                RaisePropertyChangedEvent("UsageItemIdName");
            }
        }

        public string UsageLengthName
        {
            get => (string)Settings.Default["UsageLengthName"];
            set
            {
                Settings.Default["UsageLengthName"] = value;
                RaisePropertyChangedEvent("UsageLengthName");
            }
        }

        public string UsageDateName
        {
            get => (string)Settings.Default["UsageDateName"];
            set
            {
                Settings.Default["UsageDateName"] = value;
                RaisePropertyChangedEvent("UsageDateName");
            }
        }
    }
}