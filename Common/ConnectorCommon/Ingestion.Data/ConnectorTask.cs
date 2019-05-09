// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Task provided by Rest API
    /// </summary>
    public class ConnectorTask
    {
        /// <summary>
        /// Tenant Id
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Job Id
        /// </summary>
        public string JobId { get; set; }

        /// <summary>
        /// Task Id
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// Start Time for which, the data is to be ingested
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End Time upto which, the data is to be ingested
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// List of entities that got updated and needs to be re-fetched and pushed to store
        /// </summary>
        public List<string> DirtyEntities { get; set; }

        /// <summary>
        /// Blob SAS Uri
        /// </summary>
        public string BlobSasUri { get; set; }
    }
}
