// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// List of Pages Response
    /// </summary>
    public class PageListFB
    {
        /// <summary>
        /// List of Pages
        /// </summary>
        [JsonProperty("data")]
        public PageFB[] Data { get; set; }

        /// <summary>
        /// For Paginated Results
        /// </summary>
        [JsonProperty("paging")]
        public PagingFB Paging { get; set; }
    }
}