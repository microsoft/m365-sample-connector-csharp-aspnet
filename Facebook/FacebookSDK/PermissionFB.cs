// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// To check what all permissions user has granted
    /// </summary>
    public class PermissionFB
    {
        /// <summary>
        /// permission type
        /// </summary>
        [JsonProperty("permission")]
        public string Permission { get; set; }

        /// <summary>
        /// granted or declined
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
