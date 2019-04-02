// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Sample.Connector.FacebookSDK
{
    using Newtonsoft.Json;

    /// <summary>
    /// Facebook Attachment Data
    /// </summary>
    public class AttachmentDataFB
    {
        /// <summary>
        /// Facebook Attachment Media, Require this for fetching media
        /// </summary>
        [JsonProperty("media")]
        public MediaFB Media { get; set; }

        /// <summary>
        /// Facebook Attachment Attribute
        /// </summary>
        [JsonProperty("target")]
        public TargetFB Target { get; set; }

        /// <summary>
        /// Facebook Attachment Type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Facebook Attachment Url
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Facebook Attachment Title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Facebook Attachment Description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Facebook Attachment Tags
        /// </summary>
        [JsonProperty("description_tags")]
        public TaggedUserFB[] DescriptionTags { get; set; }

        /// <summary>
        /// List of subattachments
        /// </summary>
        [JsonProperty("subattachments")]
        public AttachmentFB Subattachments { get; set; }
    }
}
