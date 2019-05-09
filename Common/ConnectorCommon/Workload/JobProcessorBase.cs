// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class JobProcessorBase
    {
        public abstract Task<List<ItemMetadata>> FetchData(ConnectorTask taskInfo, string sourceInfo);
    }
}