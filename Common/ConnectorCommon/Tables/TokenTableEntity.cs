// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class TokenTableEntity : TableEntity
    {
        public TokenTableEntity(string jobId, string jobType)
            : base()
        {
            this.PartitionKey = jobId;
            this.RowKey = jobType;
        }

        public TokenTableEntity()
        {

        }

        public string Token { get; set; }
    }
}