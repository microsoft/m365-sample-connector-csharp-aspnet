// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    /// <summary>
    /// Factory class to produces object on the based on Job Type for connector data source
    /// </summary>
    public class ConnectorSourceFactory
    {
        /// <summary>
        /// Get Connector Source Instance
        /// </summary>
        /// <param name="jobType">Connector Job type</param>
        /// <returns>Connector source provider</returns>
        public IConnectorSourceProvider GetConnectorSourceInstance(ConnectorJobType jobType)
        {
            switch (jobType)
            {
                case ConnectorJobType.Facebook:
                    return new FacebookProvider();
                default:
                    return null;
            }
        }
    }
}