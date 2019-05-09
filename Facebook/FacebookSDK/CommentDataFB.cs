// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Comment
    /// </summary>
    public class CommentDataFB
    {
        /// <summary>
        /// Facebook user who commented
        /// </summary>
        [JsonProperty("from")]
        public UserFB From { get; set; }

        /// <summary>
        /// Comment Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Facebook Comment Created Time
        /// </summary>
        [JsonProperty("created_time")]
        public string CreatedTime { get; set; }

        /// <summary>
        /// Comment Message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Attachment in Comments
        /// </summary>
        [JsonProperty("attachment")]
        public AttachmentDataFB Attachment { get; set; }

        /// <summary>
        /// No of replies
        /// </summary>
        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        /// <summary>
        /// No of likes on comment
        /// </summary>
        [JsonProperty("like_count")]
        public int LikeCount { get; set; }

        /// <summary>
        /// Facebook Users, tagged in Comment's Message
        /// </summary>
        [JsonProperty("message_tags")]
        public TaggedUserFB[] MessageTags { get; set; }

        /// <summary>
        /// Contains information about Parent Comment, works only for replies
        /// </summary>
        [JsonProperty("parent")]
        public CommentDataFB Parent { get; set; }

        /// <summary>
        /// List of replies
        /// </summary>
        [JsonProperty("comments")]
        public CommentFB Comments { get; set; }
    }
}