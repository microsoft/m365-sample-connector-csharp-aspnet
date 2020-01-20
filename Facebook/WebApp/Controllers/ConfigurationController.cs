// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.WindowsAzure.Storage.Table;
    using Sample.Connector.FacebookSDK;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    [ApiAuthorizationModule]
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

            if (!string.IsNullOrEmpty(configSettings["FBAppIdValue"]))
            {
                SettingsFB.FacebookAppId = configSettings["FBAppIdValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("FacebookAppId", configSettings["FBAppIdValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["FBAppSecretValue"]))
            {
                SettingsFB.FacebookAppSecret = configSettings["FBAppSecretValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("FacebookAppSecret", configSettings["FBAppSecretValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["FBVerifyTokenValue"]))
            {
                SettingsFB.FacebookVerifyToken = configSettings["FBVerifyTokenValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("FacebookVerifyToken", configSettings["FBVerifyTokenValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["AADAppIdValue"]))
            {
                Settings.AAdAppId = configSettings["AADAppIdValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("AAdAppId", configSettings["AADAppIdValue"]));
            }

            if (!string.IsNullOrEmpty(configSettings["AADAppSecretValue"]))
            {
                Settings.AAdAppSecret = configSettings["AADAppSecretValue"];
                await azureTableProvider.InsertOrReplaceEntityAsync(SettingsTable, new ConfigurationSettingsEntity("AAdAppSecret", configSettings["AADAppSecretValue"]));
            }
            return true;
        }

        /// <summary>
        /// Get configuration settings
        /// </summary>
        /// <returns>configuration settings</returns>
        [HttpGet]
        [Route("api/Configuration")]
        public Task<Dictionary<string,string>> GetConfiguration()
        {
            Dictionary<string, string> configurationSettings = new Dictionary<string, string>();
            configurationSettings.Add("FBAppIdValue", SettingsFB.FacebookAppId);
            configurationSettings.Add("FBAppSecretValue", SettingsFB.FacebookAppSecret);
            configurationSettings.Add("FBVerifyTokenValue", SettingsFB.FacebookVerifyToken);
            configurationSettings.Add("AADAppIdValue", Settings.AAdAppId);
            configurationSettings.Add("AADAppSecretValue", Settings.AAdAppSecret);
            return Task.FromResult(configurationSettings);
        }
    }
}