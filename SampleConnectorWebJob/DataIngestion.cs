// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class DataIngestion
    {
        private readonly HttpClient httpClient;
        private CloudTable PageJobMappingTable;
        private readonly AzureTableProvider azureTableProvider;

        public DataIngestion(HttpClient httpClient, AzureTableProvider azureTableProvider, CloudTable cloudTable)
        {
            this.httpClient = httpClient;
            this.PageJobMappingTable = cloudTable;
            this.azureTableProvider = azureTableProvider;
        }

        public async Task Execute(string jobMessage)
        {
            if (string.IsNullOrEmpty(Settings.AAdAppId) || string.IsNullOrEmpty(Settings.AAdAppSecret))
            {
                // Will throw exception if the connector is not configured.
                await GetConfigurationSettingFomStorge();
            }
            ConnectorTask taskInfo = JsonConvert.DeserializeObject<ConnectorTask>(jobMessage);
            IEventApiClient eventApiClient = new EventApiClient(new Auth(Settings.AAdAppId, Settings.AAdAppSecret), Settings.EventAPIBaseUrl);
            IUploader uploader = new BlobUploader(taskInfo.BlobSasUri);
            string sourceInfo = await GetSourceInfoFromTable(taskInfo);
            Trace.TraceInformation($"Fetched job info from PageJobEntity Table for JobId: {taskInfo.JobId} and TaskId: {taskInfo.TaskId}");
            Status status;
            List<ItemMetadata> itemMetadata = new List<ItemMetadata>();
            JobProcessorFB jfb = new JobProcessorFB(new Downloader(), uploader);
            try
            {
                itemMetadata = await jfb.FetchData(taskInfo, sourceInfo);
                status = Status.Success;
                Trace.TraceInformation($"Successfully completed Job Execution, JobId:{taskInfo.JobId}, TaskId:{taskInfo.TaskId}");
            }
            catch (ClientException<FacebookSDK.ErrorsFB> e)
            {
                status = Status.PermanentFailure;
                Trace.TraceError($"Error while downloading data from source, JobId:{taskInfo.JobId}, TaskId:{taskInfo.TaskId}, Error: {e.error.Error.ErrorMessage}");
            }
            catch (HttpRequestException e)
            {
                status = Status.TemporaryFailure;
                Trace.TraceError($"Connectivity Error, JobId:{taskInfo.JobId}, TaskId:{taskInfo.TaskId}, Error: {e.Message}, ErrorStackTrace: {e.StackTrace}");
            }
            catch (Exception e)
            {
                status = Status.PermanentFailure;
                Trace.TraceError($"Unknown Failure, Requires Attention, JobId:{taskInfo.JobId}, TaskId:{taskInfo.TaskId}, Error: {e.Message}, ErrorStackTrace: {e.StackTrace}");
            }
            await eventApiClient.OnDownloadCompleteAsync(taskInfo.TenantId, taskInfo.JobId, taskInfo.TaskId, status, itemMetadata);
        }

        private async Task<string> GetSourceInfoFromTable(ConnectorTask taskInfo)
        {
            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == taskInfo.JobId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(PageJobMappingTable, filter);
            PageJobEntity pageJobEntity = pageJobEntityList?[0];
            return pageJobEntity.SourceInfo;
        }

        private async Task GetConfigurationSettingFomStorge()
        {
            CloudTable settingsTable = azureTableProvider.GetAzureTableReference(Settings.ConfigurationSettingsTableName);
            Settings.AAdAppId = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "AAdAppId")).settingValue;
            Settings.AAdAppSecret = (await azureTableProvider.GetEntityAsync<ConfigurationSettingsEntity>(settingsTable, "ConfigurationSetting", "AAdAppSecret")).settingValue;
        }
    }
}
