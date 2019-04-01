namespace Sample.Connector
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class ConfigurationSettingsEntity : TableEntity
    {
        public ConfigurationSettingsEntity(string settingName, string settingValue)
            : base()
        {
            this.PartitionKey = "ConfigurationSetting";
            this.RowKey = settingName;
            this.settingValue = settingValue;
        }

        public ConfigurationSettingsEntity()
        {

        }

        public string settingValue { get; set; }
    }
}