// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Used in counting objects {likes, reactions, comments}
    /// </summary>
    public class ObjectSummaryFB
    {
        /// <summary>
        /// Total Count
        /// </summary>
        [JsonProperty("total_count")]
        public long TotalCount { get; set; }

        /// <summary>
        /// Can Like, For likes summary
        /// </summary>
        [JsonProperty("can_like")]
        public bool CanLike { get; set; }

        /// <summary>
        /// Has Liked, For likes summary
        /// </summary>
        [JsonProperty("has_liked")]
        public bool HasLiked { get; set; }

        /// <summary>
        /// Viewer's Reaction, For reactions summary
        /// </summary>
        [JsonProperty("viewer_reaction")]
        public string ViewerReaction { get; set; }

        /// <summary>
        /// Chronological or Reverse Chronological, For comments summary
        /// </summary>
        [JsonProperty("order")]
        public string Order { get; set; }

        /// <summary>
        /// Can comment, For comments summary
        /// </summary>
        [JsonProperty("can_comment")]
        public bool CanComment { get; set; }
    }
}
