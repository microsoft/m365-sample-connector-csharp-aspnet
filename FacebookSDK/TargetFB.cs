// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper Class for Attachments
    /// </summary>
    public class TargetFB
    {
        /// <summary>
        /// Attachment Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Attachment FB URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}