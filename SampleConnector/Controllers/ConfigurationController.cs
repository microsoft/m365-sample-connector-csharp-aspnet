// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class ConfigurationController : ApiController
    {
        private readonly AzureTableProvider azureTableProvider;
        private CloudTable SettingsTable;

        public ConfigurationController()
        {
            this.azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
        }

        /// <summary>
        /// Configure settings
        /// </summary>
        /// <param name="configSettings">Configuration settings</param>
        /// <returns>if configuration is saved successfully</returns>
        [HttpPost]
        [Route("api/Configuration")]
        public async Task<bool> Configure([FromBody] Dictionary<string, string> configSettings)
        {
            SettingsTable = azureTableProvider.GetAzureTableReference(Settings.ConfigurationSettingsTableName);
            Settings.FacebookAppId = configSettings["FBAppIdValue"];
            await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("FacebookAppId", configSettings["FBAppIdValue"]));

            Settings.FacebookAppSecret = configSettings["FBAppSecretValue"];
            await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("FacebookAppSecret", configSettings["FBAppSecretValue"]));

            Settings.FacebookVerifyToken = configSettings["FBVerifyTokenValue"];
            await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("FacebookVerifyToken", configSettings["FBVerifyTokenValue"]));

            Settings.AAdAppId = configSettings["AADAppIdValue"];
            await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("AAdAppId", configSettings["AADAppIdValue"]));

            Settings.AAdAppSecret = configSettings["AADAppSecretValue"];
            await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("AAdAppSecret", configSettings["AADAppSecretValue"]));

            Settings.APPINSIGHTS_INSTRUMENTATIONKEY = configSettings["InstrumentationKeyValue"];
            await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("APPINSIGHTS_INSTRUMENTATIONKEY", configSettings["InstrumentationKeyValue"]));
            TelemetryConfiguration.Active.InstrumentationKey = configSettings["InstrumentationKeyValue"];
            return true;
        }
    }
}