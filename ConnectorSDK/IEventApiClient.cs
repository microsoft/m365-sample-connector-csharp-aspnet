// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods for use by Connector Service to send updates to O365 Import.
    /// </summary>
    public interface IEventApiClient
    {
        /// <summary>
        /// Should be called once the data from the data source is downloaded for the given task
        /// to transfer the downloaded data to O365 Import.
        /// </summary>
        /// <param name="tenantId">The id of the tenant, making the request, validated by the service</param>
        /// <param name="jobId">The job id given by O365 Import, to be passed as is</param>
        /// <param name="taskId">The task id given by O365 Import, to be passed as is</param>
        /// <param name="status">Status of the download</param>
        /// <param name="syncMetadata">Sync metadata to keep the data in O365 synced (upate/delete as needed)</param>
        Task OnDownloadCompleteAsync(string tenantId, string jobId, string taskId, Status status, List<ItemMetadata> syncMetadata);

        /// <summary>
        /// Should be called to indicate that a webhook event has been received from the data source.
        /// </summary>
        /// <param name="tenantId">The id of the tenant, making the request, validated by the service</param>
        /// <param name="jobId">The job id given by O365 Import, to be passed as is</param>
        /// <param name="timeStamp">Event Time Stamp</param>
        /// <param name="itemId">The id of the item that resulted in the event</param>
        /// <param name="change">type of change</param>
        Task OnWebhookEvent(string tenantId, string jobId, string timeStamp, string itemId, string change);
    }
}