// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Page containing different fields
    /// </summary>
    public class PageFB
    {
        /// <summary>
        /// Gets or sets Page ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Access token
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Access token
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
