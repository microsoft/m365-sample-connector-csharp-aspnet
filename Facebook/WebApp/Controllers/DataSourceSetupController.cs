// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Linq.Expressions;
    using Microsoft.WindowsAzure.Storage.Table;
    using Sample.Connector.FacebookSDK;

    /// <summary>
    /// API controller for all connector setups
    /// </summary>
    [ApiAuthorizationModule]
    public class DataSourceSetupController : ApiController
    {
        /// <summary>
        /// Temporaray access code
        /// </summary>
        private const string TokenParam = "temporaryAccessCode";

        private readonly AzureTableProvider azureTableProvider;

        private readonly IConnectorSourceProvider sourceProvider;

        private CloudTable PageJobMappingTable;

        public DataSourceSetupController()
        {
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            sourceProvider = new FacebookProvider(azureTableProvider);
        }

        /// <summary>
        /// Returns the list of entities for connector job type
        /// </summary>
        /// <param name="jobType">Connector job type</param>
        /// <param name="jobId">job Id for current job</param>
        /// <returns>List of jobs owned by the tenant.</returns>
        [HttpGet]
        [Route("api/ConnectorSetup/GetEntities")]
        public async Task<IEnumerable<ConnectorEntity>> Get([FromUri] string jobType, [FromUri] string jobId)
        {
            CloudTable jobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);

            Trace.TraceInformation("Getting connector Entities for JobType {0}", jobType.ToString());

            IEnumerable<ConnectorEntity> entities = await sourceProvider.GetEntities(jobId);

            Trace.TraceInformation("Entities retrieved: {0}", entities?.Count());

            if (entities != null)
            {
                foreach (ConnectorEntity entity in entities)
                {
                    Expression<Func<PageJobEntity, bool>> filter = (e => e.PartitionKey == entity.Id);
                    List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(jobMappingTable, filter);

                    entity.AlreadyUsed = pageJobEntityList.Any();
                }
            }

            return entities;
        }

        [HttpPost]
        [Route("api/ConnectorSetup/StoreToken")]
        public async Task<bool> StoreToken([FromBody] Dictionary<string, string> tokenInfo)
        {
            await sourceProvider.StoreOAuthToken(tokenInfo[TokenParam], tokenInfo["redirectUrl"], tokenInfo["jobId"]);
            return true;
        }

        [HttpGet]
        [Route("api/ConnectorSetup/DeleteToken")]
        public bool DeleteToken([FromUri] string jobType, [FromUri] string jobId)
        {
            sourceProvider.DeleteToken(jobType, jobId);
            Trace.TraceInformation("Token deleted succesfully. JobType: {0}", jobType);
            return true;
        }

        [HttpGet]
        [Route("api/ConnectorSetup/OAuthUrl")]
        public async Task<string> GetOAuthUrl([FromUri] string jobType, [FromUri] string redirectUrl)
        {
            string authUrl = await sourceProvider.GetOAuthUrl(redirectUrl);

            Trace.TraceInformation("GetOAuthUrl url generated successfully for Jobtype {0}", jobType);
            return authUrl;
        }

        /// <summary>
        /// Store the selected page
        /// </summary>
        /// <param name="page">page data</param>
        /// <param name="jobId">jobId partition key</param>
        /// <returns>if source is saved successfully</returns>
        [HttpPost]
        [Route("api/ConnectorSetup/SavePage")]
        public async Task<bool> SavePage([FromBody] ConnectorEntity page, [FromUri] string jobId)
        {
            string sourceInfo = await sourceProvider.GetAuthTokenForResource(page.Id, jobId);

            PageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
            PageJobEntity pageJobEntity = new PageJobEntity(page.Id, jobId);
            pageJobEntity.SourceInfo = sourceInfo;

            await azureTableProvider.InsertEntityAsync(PageJobMappingTable, pageJobEntity);
            Trace.TraceInformation("Job Setup complete page succesfully saved for jobId: {0}", jobId);

            try
            {
                Trace.TraceInformation("Job with JobId: {0} subscribing to webhook", jobId);
                await sourceProvider.Subscribe(pageJobEntity.SourceInfo);
                Trace.TraceInformation("Job with JobId: {0} successfully subscribed to webhook", jobId);
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Job with JobId: {0} subscribed to webhook failed with error: {1}", jobId, e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update the job after relogin
        /// </summary>
        /// <param name="jobId">jobId partition key</param>
        /// <returns>if source is saved successfully</returns>
        [HttpPost]
        [Route("api/ConnectorSetup/Update")]
        public async Task<bool> Update([FromUri] string jobId)
        {
            PageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
            Expression<Func<PageJobEntity, bool>> filter = (e => e.RowKey == jobId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(PageJobMappingTable, filter);

            PageJobEntity page = pageJobEntityList[0];
            string sourceInfo = await sourceProvider.GetAuthTokenForResource(page.PartitionKey, jobId);

            PageJobEntity pageJobEntity = new PageJobEntity(page.PartitionKey, jobId);
            pageJobEntity.SourceInfo = sourceInfo;

            await azureTableProvider.InsertOrReplaceEntityAsync(PageJobMappingTable, pageJobEntity);
            Trace.TraceInformation("Job update complete page successfully saved for jobId: {0}", jobId);

            try
            {
                Trace.TraceInformation("Job with JobId: {0} subscribing to webhook", jobId);
                await sourceProvider.Subscribe(pageJobEntity.SourceInfo);
                Trace.TraceInformation("Job with JobId: {0} successfully subscribed to webhook", jobId);
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Job with JobId: {0} subscribed to webhook failed with error: {1}", jobId, e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the job is for setup or relogin
        /// </summary>
        /// <param name="jobId">jobId partition key</param>
        /// <returns>true if scenario is for Relogin</returns>
        [HttpGet]
        [Route("api/ConnectorSetup/IsRelogin")]
        public async Task<bool> IsRelogin([FromUri] string jobId)
        {
            PageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
            Expression<Func<PageJobEntity, bool>> filter = (e => e.RowKey == jobId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(PageJobMappingTable, filter);

            if (pageJobEntityList.Any())
            {
                return true;
            }

            return false;
        }
    }
}