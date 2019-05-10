// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Attachment
    /// </summary>
    public class AttachmentFB
    {
        /// <summary>
        /// List of Attachments
        /// </summary>
        [JsonProperty("data")]
        public AttachmentDataFB[] Data { get; set; }

        /// <summary>
        /// For Paginated Results
        /// </summary>
        [JsonProperty("paging")]
        public PagingFB Paging { get; set; }
    }
}