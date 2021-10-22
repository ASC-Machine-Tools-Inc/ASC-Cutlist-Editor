namespace AscCutlistEditor.Frameworks
{
    public interface ISettings
    {
        string ConnectionString { get; set; }

        bool UseConnectionString { get; set; }

        string DataSource { get; set; }

        string DatabaseName { get; set; }

        string Username { get; set; }

        string Password { get; set; }

        string CoilTableName { get; set; }

        string CoilStartLengthName { get; set; }

        string CoilLengthUsedName { get; set; }

        string CoilMaterialName { get; set; }

        string CoilNumberName { get; set; }

        string CoilDescriptionName { get; set; }

        string CoilDateName { get; set; }

        string OrderTableName { get; set; }

        string OrderNumName { get; set; }

        string OrderMaterialName { get; set; }

        string OrderQuantityName { get; set; }

        string OrderPartNumName { get; set; }

        string OrderMachineNumName { get; set; }

        string OrderItemIdName { get; set; }

        string OrderLengthName { get; set; }

        string OrderBundleName { get; set; }

        string BundleTableName { get; set; }

        string BundleOrderNumName { get; set; }

        string BundleColumns { get; set; }

        string UsageTableName { get; set; }

        string UsageOrderNumName { get; set; }

        string UsageMaterialName { get; set; }

        string UsageItemIdName { get; set; }

        string UsageLengthName { get; set; }

        string UsageDateName { get; set; }
    }
}