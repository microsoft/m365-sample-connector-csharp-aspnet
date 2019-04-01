// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Notification Data
    /// </summary>
    public class NotificationDataFB
    {
        /// <summary>
        /// Facebook Notification Id
        /// </summary>
        [JsonProperty("id")]
        public string NotificationId { get; set; }

        /// <summary>
        /// Facebook Notification Created Time
        /// </summary>
        [JsonProperty("created_time")]
        public string CreatedTime { get; set; }

        /// <summary>
        /// Facebook Notification Updated Time
        /// </summary>
        [JsonProperty("updated_time")]
        public string UpdatedTime { get; set; }

        /// <summary>
        /// Facebook User responsible for notification
        /// </summary>
        [JsonProperty("from")]
        public UserFB From { get; set; }

        /// <summary>
        /// Facebook User who got notification
        /// </summary>
        [JsonProperty("to")]
        public UserFB To { get; set; }

        /// <summary>
        /// Facebook Notification Title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Facebook Notification Link
        /// </summary>
        [JsonProperty("link")]
        public string Link { get; set; }

        /// <summary>
        /// Facebook Notification Object
        /// </summary>
        [JsonProperty("object")]
        public NotificationObjectFB NotificationObject { get; set; }

        /// <summary>
        /// Notification Read or Unread
        /// </summary>
        [JsonProperty("unread")]
        public Int32 Unread { get; set; }
    }
}
