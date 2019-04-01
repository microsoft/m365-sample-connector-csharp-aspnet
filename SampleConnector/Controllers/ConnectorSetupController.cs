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
    public class ConnectorSetupController : ApiController
    {
        private readonly AzureTableProvider azureTableProvider;

        public ConnectorSetupController()
        {
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
        }

        /// <summary>
        /// Validate Connector Setup 
        /// </summary>
        /// <returns>true for validation success</returns>
        [HttpGet]
        [Route("api/ConnectorSetup/ValidateSetup")]
        public Task<bool> ValidateSetup()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Final job setup for Connector service
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <param name="tenantId">tenant Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ConnectorSetup/ConnectorOAuth")]
        public async Task<bool> ConnectorOAuth([FromUri] string jobId, [FromUri] string tenantId)
        {
            CloudTable jobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);
            
            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == jobId && entity.TenantId == tenantId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(jobMappingTable, filter);

            if (!pageJobEntityList.Any())
            {
                return false;
            }

            Trace.TraceInformation("Job with JobId: {0}, tenantId: {1} successfully set up", jobId, tenantId);
            PageJobEntity pageJobEntity = pageJobEntityList?[0];
            return true;
        }
    }
}