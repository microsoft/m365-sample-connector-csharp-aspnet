// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// FB Page Information
    /// </summary>
    public class UserLocationFB
    {
        /// <summary>
        /// Page ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Page Category
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// Number of Checkins
        /// </summary>
        [JsonProperty("checkins")]
        public string Checkins { get; set; }

        /// <summary>
        /// Page Description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Number of Likes
        /// </summary>
        [JsonProperty("likes")]
        public int Likes { get; set; }

        /// <summary>
        /// Link to Page
        /// </summary>
        [JsonProperty("link")]
        public string Link { get; set; }

        /// <summary>
        /// Page Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
