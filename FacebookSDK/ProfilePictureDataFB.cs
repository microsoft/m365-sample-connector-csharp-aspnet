// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Profile Picture Data
    /// </summary>
    public class ProfilePictureDataFB
    {
        /// <summary>
        /// Profile Picture Information
        /// </summary>
        [JsonProperty("data")]
        public ProfilePictureFB Data { get; set; }
    }
}
