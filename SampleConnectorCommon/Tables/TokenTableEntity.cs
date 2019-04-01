// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class TokenTableEntity : TableEntity
    {
        public TokenTableEntity(string upn, string jobType)
            : base()
        {
            this.PartitionKey = upn;
            this.RowKey = jobType;
        }

        public TokenTableEntity()
        {

        }

        public string Token { get; set; }
    }
}