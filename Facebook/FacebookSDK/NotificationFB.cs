// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Page Notification
    /// </summary>
    public class NotificationFB
    {
        /// <summary>
        /// List of Notifications
        /// </summary>
        [JsonProperty("data")]
        public NotificationDataFB[] Data { get; set; }

        /// <summary>
        /// For Paginated Results
        /// </summary>
        [JsonProperty("paging")]
        public PagingFB Paging { get; set; }

        /// <summary>
        /// Summary Results
        /// </summary>
        [JsonProperty("summary")]
        public NotificationSummaryFB NotificationSummary { get; set; }
    }
}