// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Post containing different fields
    /// </summary>
    public class PostFB
    {
        /// <summary>
        /// Facebook Post Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Facebook Post Created Time
        /// </summary>
        [JsonProperty("created_time")]
        public string CreatedTime { get; set; }

        /// <summary>
        /// Facebook User who posted
        /// </summary>
        [JsonProperty("from")]
        public UserFB From { get; set; }

        /// <summary>
        /// Facebook User on whose timeline the post is
        /// </summary>
        [JsonProperty("to")]
        public UserFB To { get; set; }

        /// <summary>
        /// Facebook Post Message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Facebook Post Story
        /// </summary>
        [JsonProperty("story")]
        public string Story { get; set; }

        /// <summary>
        /// Likes on post
        /// </summary>
        [JsonProperty("likes")]
        public ObjectQueryFB Likes { get; set; }

        /// <summary>
        /// Reactions on post
        /// </summary>
        [JsonProperty("reactions")]
        public ObjectQueryFB Reactions { get; set; }

        /// <summary>
        /// Comments on post
        /// </summary>
        [JsonProperty("comments")]
        public CommentFB Comments { get; set; }

        /// <summary>
        /// Facebook Post Video source, if there is a video in the post
        /// </summary>
        [JsonProperty("source")]
        public string Source { get; set; }

        /// <summary>
        /// Facebook Users, tagged in Post's Message
        /// </summary>
        [JsonProperty("message_tags")]
        public TaggedUserFB[] MessageTags { get; set; }

        /// <summary>
        /// Type of Post
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Type of status
        /// </summary>
        [JsonProperty("status_type")]
        public string StatusType { get; set; }

        /// <summary>
        /// Facebook Post Attachments
        /// </summary>
        [JsonProperty("attachments")]
        public AttachmentFB Attachments { get; set; }
    }
}
