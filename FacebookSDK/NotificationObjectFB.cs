// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Notification Object Data
    /// </summary>
    public class NotificationObjectFB
    {
        /// <summary>
        /// Facebook Object Id
        /// </summary>
        [JsonProperty("id")]
        public string ObjectID { get; set; }

        /// <summary>
        /// Facebook Object Created Time
        /// </summary>
        [JsonProperty("created_time")]
        public string CreatedTime { get; set; }

        /// <summary>
        /// Facebook Object Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Facebook Object Message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Facebook Object Story
        /// </summary>
        [JsonProperty("story")]
        public string Story { get; set; }
    }
}
