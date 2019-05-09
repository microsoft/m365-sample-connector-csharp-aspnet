// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Page Notification Summary
    /// </summary>
    public class NotificationSummaryFB
    {
        /// <summary>
        /// Unseen Notification Count
        /// </summary>
        [JsonProperty("unseen_count")]
        public Int32 UnseenNotificationCount { get; set; }

        /// <summary>
        /// Updated Time
        /// </summary>
        [JsonProperty("updated_time")]
        public string UpdatedTime { get; set; }
    }
}