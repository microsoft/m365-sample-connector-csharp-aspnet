// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook User tagged in message
    /// </summary>
    public class MessageTagsFB
    {
        /// <summary>
        /// List of users tagged in message
        /// </summary>
        [JsonProperty("data")]
        public TaggedUserFB[] Data { get; set; }

        /// <summary>
        /// For Paginated Results
        /// </summary>
        [JsonProperty("paging")]
        public PagingFB Paging { get; set; }
    }
}