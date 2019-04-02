// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Webhook Feed Data Model 
    /// </summary>
    public class WebhookFeedFB
    {
        /// <summary>
        /// An array containing an object describing the changes.
        /// Multiple changes from diff objects that are of same type may be batched together
        /// </summary>
        [JsonProperty("entry")]
        public List<Entry> Entry { get; set; }

        /// <summary>
        /// The object's type (eg. user, page)
        /// </summary>
        [JsonProperty("object")]
        public string Object { get; set; }
    }

    /// <summary>
    /// An object describing the changes.
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// An array containing an object describing the changed field and their new values
        /// </summary>
        [JsonProperty("changes")]
        public List<Change> Changes { get; set; }

        /// <summary>
        /// The object's id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// A UNIX timestamp indicating when the Event Notification was sent
        /// </summary>
        [JsonProperty("time")]
        public int Time { get; set; }
    }

    /// <summary>
    /// An object describing the changed field and their new values
    /// </summary>
    public class Change
    {
        /// <summary>
        /// Source of Webhook (eg. feed)
        /// </summary>
        [JsonProperty("field")]
        public string Field { get; set; }

        /// <summary>
        /// A collection of strings indicating the names of the fields that have been changed
        /// </summary>
        [JsonProperty("value")]
        public Value Value { get; set; }
    }

    /// <summary>
    /// A collection of strings indicating the names of the fields that have been changed
    /// </summary>
    public class Value
    {
        /// <summary>
        /// Post Id
        /// </summary>
        [JsonProperty("post_id")]
        public string PostId { get; set; }

        /// <summary>
        /// Name of source of Webhook
        /// </summary>
        [JsonProperty("sender_name")]
        public string SenderName { get; set; }

        /// <summary>
        /// Id of source of Webhook
        /// </summary>
        [JsonProperty("sender_id")]
        public string SenderId { get; set; }

        /// <summary>
        /// Changed Item
        /// </summary>
        [JsonProperty("item")]
        public string Item { get; set; }

        /// <summary>
        /// Action (eg. add, update, delete)
        /// </summary>
        [JsonProperty("verb")]
        public string Verb { get; set; }

        /// <summary>
        /// Published bit (0 or 1)
        /// </summary>
        [JsonProperty("published")]
        public int Published { get; set; }

        /// <summary>
        /// Change Created Time
        /// </summary>
        [JsonProperty("created_time")]
        public int CreatedTime { get; set; }
        
        /// <summary>
        /// Message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }

}