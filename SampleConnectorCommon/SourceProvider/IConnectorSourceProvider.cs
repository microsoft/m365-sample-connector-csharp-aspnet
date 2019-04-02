// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// interface for connector data sources 
    /// </summary>
    public interface IConnectorSourceProvider
    {
        /// <summary>
        /// Get OAuth Token For Resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="jobId"></param>
        Task<string> GetAuthTokenForResource(string resourceId, string jobId);

        /// <summary>
        /// Get EmailId
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task<string> GetEmailId(string jobId);

        /// <summary>
        /// Get Entities
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task<IEnumerable<ConnectorEntity>> GetEntities(string jobId);

        /// <summary>
        /// Store OAuth Token
        /// </summary>
        /// <param name="accessCode"></param>
        /// <param name="redirectUrl"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task StoreOAuthToken(string accessCode, string redirectUrl, string jobId);

        /// <summary>
        /// Delete Token
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task DeleteToken(ConnectorJobType jobType, string jobId);

        /// <summary>
        /// Get OAuthUrl
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        Task<string> GetOAuthUrl(string redirectUrl);

        /// <summary>
        /// post subscribe to get real time update
        /// </summary>
        /// <param name="sourceInfo">source info</param>
        /// <returns>whether it is subscribed or not</returns>
        Task<bool> Subscribe(string sourceInfo);

        /// <summary>
        /// unsubscribe for real time updates
        /// </summary>
        /// <param name="sourceInfo">source info</param>
        /// <returns>whether it is unsubscribed or not</returns>
        Task<bool> Unsubscribe(string sourceInfo);
    }
}
