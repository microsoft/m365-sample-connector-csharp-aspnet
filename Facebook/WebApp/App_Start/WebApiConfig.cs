// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Threading.Tasks;
    using Sample.Connector.FacebookSDK;

    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var routes = config.Routes;

            routes.MapHttpRoute("DefaultApiWithId", "Api/{controller}/{id}", new { id = RouteParameter.Optional });
            routes.MapHttpRoute("DefaultApiGet", "Api/{controller}", new { action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            routes.MapHttpRoute("DefaultApiPost", "Api/{controller}", new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());

            config.Filters.Add(new ExceptionFilter());

            InitializeStorageEntities().Wait();
        }

        private static async Task InitializeStorageEntities()
        {
            AzureTableProvider azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            CloudTable tokenTable = await azureTableProvider.EnsureTableExistAsync(Settings.TokenTableName);
            CloudTable pageJobMappingTable = await azureTableProvider.EnsureTableExistAsync(Settings.PageJobMappingTableName);
            CloudTable settingsTable = await azureTableProvider.EnsureTableExistAsync(Settings.ConfigurationSettingsTableName);
            await GetConfigurationSettingFomStorge(azureTableProvider, settingsTable);
        }

        private static async Task GetConfigurationSettingFomStorge(AzureTableProvider azureTableProvider, CloudTable settingsTable)
        {
            SettingsFB.FacebookAppId = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "FacebookAppId"))?.settingValue;
            SettingsFB.FacebookAppSecret = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "FacebookAppSecret"))?.settingValue;
            SettingsFB.FacebookVerifyToken = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "FacebookVerifyToken"))?.settingValue;
            Settings.AAdAppId = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "AAdAppId"))?.settingValue;
            Settings.AAdAppSecret = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "AAdAppSecret"))?.settingValue;
            Settings.AADAppUri = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "AADAppUri"))?.settingValue;
            Settings.APPINSIGHTS_INSTRUMENTATIONKEY = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "APPINSIGHTS_INSTRUMENTATIONKEY"))?.settingValue;
            if (Settings.APPINSIGHTS_INSTRUMENTATIONKEY == null)
            {
                Settings.APPINSIGHTS_INSTRUMENTATIONKEY = string.Empty;
            }
        }
    }
}