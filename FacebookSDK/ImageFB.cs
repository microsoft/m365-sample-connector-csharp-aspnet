// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Helper Class for Attachments, Provides actual url for media to be fetched
    /// </summary>
    public class ImageFB
    {
        /// <summary>
        /// Height of image
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }

        /// <summary>
        /// Url of image
        /// </summary>
        [JsonProperty("src")]
        public string Src { get; set; }

        /// <summary>
        /// Width of image
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}