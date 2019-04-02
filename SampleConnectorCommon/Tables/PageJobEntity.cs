// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class PageJobEntity : TableEntity
    {
        public PageJobEntity(string resourceId, string jobId)
            : base()
        {
            this.PartitionKey = resourceId;
            this.RowKey = jobId;
        }

        public PageJobEntity()
        {

        }

        public string SourceInfo { get; set; }

        public string TenantId { get; set; }
    }
}