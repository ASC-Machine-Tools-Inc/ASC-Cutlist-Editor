using AscCutlistEditor.Frameworks;
using System.Diagnostics;
using System.Windows;
using AscCutlistEditor.Properties;

namespace AscCutlistEditorTests.Common
{
    internal class Mocks
    {
        public class MockSettings : ISettings
        {
            public string ConnectionString { get; set; }

            public bool UseConnectionString { get; set; }

            public string DataSource { get; set; }

            public string DatabaseName { get; set; }

            public string Username { get; set; }

            public string Password { get; set; }

            public string CoilTableName { get; set; }

            public string CoilStartLengthName { get; set; }

            public string CoilLengthUsedName { get; set; }

            public string CoilMaterialName { get; set; }

            public string CoilNumberName { get; set; }

            public string CoilDescriptionName { get; set; }

            public string CoilDateName { get; set; }

            public string OrderTableName { get; set; }

            public string OrderNumName { get; set; }

            public string OrderMaterialName { get; set; }

            public string OrderQuantityName { get; set; }

            public string OrderPartNumName { get; set; }

            public string OrderMachineNumName { get; set; }

            public string OrderItemIdName { get; set; }

            public string OrderLengthName { get; set; }

            public string OrderBundleName { get; set; }

            public string OrderScheduledDateName { get; set; }

            public string OrderDeleteSentName { get; set; }

            public string BundleTableName { get; set; }

            public string BundleOrderNumName { get; set; }

            public string BundleColumns { get; set; }

            public string UsageTableName { get; set; }

            public string UsageOrderNumName { get; set; }

            public string UsageMaterialName { get; set; }

            public string UsageItemIdName { get; set; }

            public string UsageLengthName { get; set; }

            public string UsageDateName { get; set; }

            public string UsageScrapName { get; set; }

            public string UsageTypeName { get; set; }

            /// <summary>
            /// Use testing values for settings.
            /// </summary>
            public void InitializeWithDefaults()
            {
                ConnectionString = Strings.ConnectionString;
                UseConnectionString = true;
                CoilTableName = "amscoil";
                CoilStartLengthName = "startlength";
                CoilLengthUsedName = "lengthused";
                CoilMaterialName = "material";
                CoilNumberName = "coilnumber";
                CoilDescriptionName = "description";
                CoilDateName = "dateout";
                OrderTableName = "amsorder";
                OrderNumName = "orderno";
                OrderMaterialName = "material";
                OrderQuantityName = "quantity";
                OrderPartNumName = "partno";
                OrderMachineNumName = "machinenum";
                OrderItemIdName = "item_id";
                OrderLengthName = "length";
                OrderBundleName = "bundle";
                OrderScheduledDateName = "schedate";
                OrderDeleteSentName = "delsent";
                BundleTableName = "amsbundle";
                BundleOrderNumName = "orderno";
                BundleColumns =
                    "material, prodcode, user1, user2, user3, user4, custname, custaddr1, custaddr2, custcity, custstate, custzip";
                UsageTableName = "amsproduct";
                UsageOrderNumName = "orderno";
                UsageMaterialName = "material";
                UsageItemIdName = "itemid";
                UsageLengthName = "totallength";
                UsageDateName = "adddate";
                UsageScrapName = "scrapfoot";
                UsageTypeName = "type";
            }
        }

        public class MockDialog : IDialogService
        {
            public void ShowMessageBox(
                string title,
                string contents,
                string cancelText)
            {
                Debug.WriteLine("Testing display message: " + contents);
            }
        }
    }
}