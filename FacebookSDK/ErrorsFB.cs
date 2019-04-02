// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Errors
    /// </summary>
    public class ErrorsFB
    {
        /// <summary>
        /// Error returned by Facebook
        /// </summary>
        [JsonProperty("error")]
        public ErrorTypeFB Error { get; set; }
    }
}
