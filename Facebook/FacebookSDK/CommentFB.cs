// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Comment
    /// </summary>
    public class CommentFB
    {
        /// <summary>
        /// List of Facebook Comments Data
        /// </summary>
        [JsonProperty("data")]
        public CommentDataFB[] Data { get; set; }

        /// <summary>
        /// For Paginated Results
        /// </summary>
        [JsonProperty("paging")]
        public PagingFB Paging { get; set; }
    }
}