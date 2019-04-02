// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Used for Likes and Comments
    /// </summary>
    public class ObjectQueryFB
    {
        /// <summary>
        /// List of users liking an FB object
        /// </summary>
        [JsonProperty("data")]
        public UserFB[] Data { get; set; }

        /// <summary>
        /// Paginated Results
        /// </summary>
        [JsonProperty("paging")]
        public PagingFB Paging { get; set; }

        /// <summary>
        /// Approximate Summary of Likes, Reactions
        /// </summary>
        [JsonProperty("summary")]
        public ObjectSummaryFB Summary { get; set; }
    }
}