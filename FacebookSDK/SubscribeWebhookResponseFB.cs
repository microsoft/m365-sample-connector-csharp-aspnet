// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// FB Response Model
    /// </summary>
    public class SubscribeWebhookResponseFB
    {
        /// <summary>
        /// Gets or sets list of Pages
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

    }
}