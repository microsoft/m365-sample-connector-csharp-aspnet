// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Collections.Generic;

    /// <summary>
    /// interface which exposes methods for Connector Data source
    /// </summary>
    public class ConnectorEntity
    {
        /// <summary>
        /// Get or set Id
        /// </summary>
        public virtual string Id
        {
            get; set;
        }

        /// <summary>
        /// Get or set Name
        /// </summary>
        public virtual string Name
        {
            get; set;
        }

        /// <summary>
        /// Get or set AdditionalInfo
        /// </summary>
        public virtual Dictionary<string, string> AdditionalInfo
        {
            get; set;
        }

        public virtual bool AlreadyUsed
        {
            get; set;
        }
    }
}