using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Linq.Expressions;
    using Microsoft.WindowsAzure.Storage.Table;
    using Sample.Connector;
    using Sample.Connector.FacebookSDK;
    using System.Net.Http;

    public class ConnectorJobController : ApiController
    {
        private readonly AzureTableProvider azureTableProvider;
        private readonly IConnectorSourceProvider connectorSourceProvider;

        public ConnectorJobController()
        {
            azureTableProvider = new AzureTableProvider(Settings.StorageAccountConnectionString);
            connectorSourceProvider = new FacebookProvider(azureTableProvider);
        }
        /// <summary>
        /// Final job setup for Connector service
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("job/IsCreated")]
        public async Task<HttpResponseMessage> ConnectorOAuth([FromUri] string jobId)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            request.SetConfiguration(configuration);
            CloudTable jobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);

            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == jobId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(jobMappingTable, filter);

            if (!pageJobEntityList.Any())
            {
                return request.CreateResponse<JobCreationResponse>(HttpStatusCode.OK, new JobCreationResponse(false, null));
            }

            Trace.TraceInformation("Job with JobId: {0} successfully set up", jobId);
            return request.CreateResponse<JobCreationResponse>(HttpStatusCode.OK, new JobCreationResponse(true, null));
        }

        /// <summary>
        /// Delete job page
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <returns>success or failure</returns>
        [HttpDelete]
        [Route("job/OnDeleted")]
        public async Task<HttpResponseMessage> DeleteJob([FromUri] string jobId)
        {
            CloudTable jobMappingTable = azureTableProvider.GetAzureTableReference(Settings.PageJobMappingTableName);

            Expression<Func<PageJobEntity, bool>> filter = (entity => entity.RowKey == jobId);
            List<PageJobEntity> pageJobEntityList = await azureTableProvider.QueryEntitiesAsync<PageJobEntity>(jobMappingTable, filter);

            if (!pageJobEntityList.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            PageJobEntity pageJobEntity = pageJobEntityList?[0];

            bool unsubscribed = await connectorSourceProvider.Unsubscribe(pageJobEntity.SourceInfo);

            if (unsubscribed == false)
            {
                Trace.TraceError("Job with JobId: {0} failed to unsubscribe to webhook", jobId);
            }
            else
            {
                Trace.TraceInformation("Job with JobId: {0} successfully unsubscribed to webhook", jobId);
            }

            await azureTableProvider.DeleteEntityAsync<PageJobEntity>(jobMappingTable, pageJobEntity);
            Trace.TraceInformation("Job with JobId: {0} successfully deleted", jobId);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        /// <summary>
        /// Remediate job 
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <param name="RemediationType">Remediationtype</param>
        /// <returns>success or failure</returns>
        [HttpGet]
        [Route("job/OnRemediation")]
        public Task<HttpResponseMessage> RemediateJob([FromUri] string jobId, JobRemediationType remediationType)
        {
            return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
