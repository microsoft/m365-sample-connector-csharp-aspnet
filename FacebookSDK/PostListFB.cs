// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Post List 
    /// </summary>
    public class PostListFB
    {
        /// <summary>
        /// List of Posts
        /// </summary>
        [JsonProperty("data")]
        public PostFB[] Data { get; set; }

        /// <summary>
        /// For Paginated Results
        /// </summary>
        [JsonProperty("paging")]
        public PagingFB Paging { get; set; }
    }
}