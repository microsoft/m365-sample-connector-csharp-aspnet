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

    /// <summary>
    /// API controller for all native connector setups
    /// </summary>
    [ApiAuthorizationModule]
    public class DataSourceSetupController : ApiController
    {
        /// <summary>
        /// Temporaray access code
        /// </summary>
        private const string TokenParam = "temporaryAccessCode";

        /// <summary>
        /// Source Provider Factory
        /// </summary>
        private readonly ConnectorSourceFactory connectorSourceFactory;

        private readonly AzureTableProvider azureTableProvider;

        private CloudTable PageJobMappingTable;

        public DataSourceSetupController()
        {
            // Can be done using dependency injection using Unity
            connectorSourceFactory = new ConnectorSourceFactory();

            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
        }

        /// <summary>
        /// Returns the list of entities for native connector job type
        /// </summary>
        /// <param name="jobType">Native Connector job type</param>
        /// <param name="jobId">job Id for current job</param>
        /// <returns>List of jobs owned by the tenant.</returns>
        [HttpGet]
        [Route("api/ConnectorSetup/GetEntities")]
        public async Task<IEnumerable<ConnectorEntity>> Get([FromUri] ConnectorJobType jobType, [FromUri] string jobId)
        {
            IConnectorSourceProvider sourceProvider = connectorSourceFactory.GetConnectorSourceInstance(jobType);
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
            ConnectorJobType jobType;
            Enum.TryParse(tokenInfo["jobType"], out jobType);

            IConnectorSourceProvider sourceProvider = connectorSourceFactory.GetConnectorSourceInstance(jobType);
            await sourceProvider.StoreOAuthToken(tokenInfo[TokenParam], tokenInfo["redirectUrl"], tokenInfo["jobId"]);
            return true;
        }
        
        [HttpGet]
        [Route("api/ConnectorSetup/DeleteToken")]
        public bool DeleteToken([FromUri] ConnectorJobType jobType, [FromUri] string jobId)
        {
            IConnectorSourceProvider sourceProvider = connectorSourceFactory.GetConnectorSourceInstance(jobType);
            sourceProvider.DeleteToken(jobType, jobId);
            Trace.TraceInformation("Token deleted succesfully. JobType: {0}", jobType);
            return true;
        }
        
        [HttpGet]
        [Route("api/ConnectorSetup/OAuthUrl")]
        public async Task<string> GetOAuthUrl([FromUri] ConnectorJobType jobType, [FromUri] string redirectUrl)
        {
            IConnectorSourceProvider sourceProvider = connectorSourceFactory.GetConnectorSourceInstance(jobType);

            string authUrl = await sourceProvider.GetOAuthUrl(redirectUrl);

            Trace.TraceInformation("GetOAuthUrl url generated successfully for Jobtype {0}", jobType);
            return authUrl;
        }

        /// <summary>
        /// Store the selected page
        /// </summary>
        /// <param name="page">page data</param>
        /// <param name="jobId">jobId partition key</param>
        /// <param name="jobType">type of job</param>
        /// <param name="tenantId">tenant id</param>
        /// <returns>if source is saved successfully</returns>
        [HttpPost]
        [Route("api/ConnectorSetup/SavePage")]
        public async Task<bool> SavePage([FromBody] ConnectorEntity page, [FromUri] string jobId, [FromUri] string jobType, [FromUri] string tenantId)
        {
            ConnectorJobType connectorjobType;
            Enum.TryParse(jobType, true, out connectorjobType);
            IConnectorSourceProvider connectorSourceProvider = connectorSourceFactory.GetConnectorSourceInstance(connectorjobType);
            string sourceInfo = await connectorSourceProvider.GetAuthTokenForResource(page.Id, jobId);

            PageJobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
            PageJobEntity pageJobEntity = new PageJobEntity(page.Id, jobId);
            pageJobEntity.SourceInfo = sourceInfo;
            pageJobEntity.TenantId = tenantId;

            await azureTableProvider.InsertEntityAsync(PageJobMappingTable, pageJobEntity);
            Trace.TraceInformation("Job Setup complete page succesfully saved for jobId: {0}", jobId);

            try
            {
                Trace.TraceInformation("Job with JobId: {0} subscribing to webhook", jobId);
                bool subscribed = await connectorSourceProvider.Subscribe(pageJobEntity.SourceInfo);
                Trace.TraceInformation("Job with JobId: {0} successfully subscribed to webhook", jobId);
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Job with JobId: {0} subscribed to webhook failed with error: {1}", jobId, e.Message);
                return false;
            }

            return true;
        }
    }
}