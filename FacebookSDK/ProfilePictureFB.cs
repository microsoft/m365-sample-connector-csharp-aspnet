// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Profile Picture
    /// </summary>
    public class ProfilePictureFB
    {
        /// <summary>
        /// Height of profile picture
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }

        /// <summary>
        /// True if the profile picture is the default 'silhouette' picture
        /// </summary>
        [JsonProperty("is_silhouette")]
        public bool IsSilhouette { get; set; }

        /// <summary>
        /// URL for Profile Picture
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Width of profile picture
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
