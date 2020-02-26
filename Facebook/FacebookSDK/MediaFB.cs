// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper Class for Attachments
    /// </summary>
    public class MediaFB
    {
        /// <summary>
        /// Image attachment
        /// </summary>
        [JsonProperty("image")]
        public ImageFB Image { get; set; }

        /// <summary>
        /// If the attachment is a video, its URL is contained in this field
        /// </summary>
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}