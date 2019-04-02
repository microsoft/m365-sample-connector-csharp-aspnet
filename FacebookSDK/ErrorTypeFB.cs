// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// Facebook Documentation : https://developers.facebook.com/docs/facebook-login/access-tokens/debugging-and-error-handling

namespace Sample.Connector.FacebookSDK
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Error Information
    /// </summary>
    public class ErrorTypeFB
    {
        /// <summary>
        /// Error Message
        /// </summary>
        [JsonProperty("message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error Type
        /// </summary>
        [JsonProperty("type")]
        public string ErrorType { get; set; }

        /// <summary>
        /// Error Code
        /// </summary>
        [JsonProperty("code")]
        public Int32 ErrorCode { get; set; }

        /// <summary>
        /// Error Sub Code
        /// </summary>
        [JsonProperty("error_subcode")]
        public string ErrorSubCode { get; set; }

        /// <summary>
        /// Error Facebook Trace Id
        /// </summary>
        [JsonProperty("fbtrace_id")]
        public string ErrorFBTraceID { get; set; }
    }
}
